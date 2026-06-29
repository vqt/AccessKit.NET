using System;

namespace AccessKit.Windows;

/// <summary>
/// Wraps <c>accesskit_windows_adapter</c> — the low-level Windows adapter for applications that run
/// their own window procedure. Unlike <see cref="WindowsSubclassingAdapter"/>, you must forward
/// <c>WM_GETOBJECT</c> to <see cref="HandleWmGetObject"/> yourself and keep the focus state in sync
/// via <see cref="UpdateWindowFocusState"/>.
/// </summary>
public sealed class WindowsAdapter : IDisposable
{
    private IntPtr _handle;

    // Held as fields so the native side can invoke them at any time without the delegates being
    // collected or relocated.
    private readonly ActionHandlerCallback _actionThunk;
    private readonly ActivationHandlerCallback _activationThunk;

    private readonly Func<TreeUpdate> _buildInitialTree;
    private readonly Action<ActionRequest> _onAction;

    /// <param name="hwnd">The native window handle (HWND).</param>
    /// <param name="isWindowFocused">Whether the window currently has keyboard focus.</param>
    /// <param name="buildInitialTree">Builds the full tree the first time an AT connects (via <c>WM_GETOBJECT</c>).</param>
    /// <param name="onAction">Invoked when an AT requests an action (may run on a UIA thread).</param>
    public WindowsAdapter(IntPtr hwnd, bool isWindowFocused, Func<TreeUpdate> buildInitialTree, Action<ActionRequest> onAction)
    {
        _buildInitialTree = buildInitialTree ?? throw new ArgumentNullException(nameof(buildInitialTree));
        _onAction = onAction ?? throw new ArgumentNullException(nameof(onAction));

        _activationThunk = OnActivation;
        _actionThunk = OnAction;

        _handle = Interop.accesskit_windows_adapter_new(hwnd, isWindowFocused, _actionThunk, IntPtr.Zero);
        if (_handle == IntPtr.Zero)
            throw new InvalidOperationException("accesskit_windows_adapter_new returned null.");
    }

    /// <summary>
    /// Handle a <c>WM_GETOBJECT</c> message. Returns the <c>LRESULT</c> to return from your window
    /// procedure, or null if AccessKit did not handle the message (fall through to <c>DefWindowProc</c>).
    /// </summary>
    public IntPtr? HandleWmGetObject(IntPtr wParam, IntPtr lParam)
    {
        if (_handle == IntPtr.Zero) return null;
        var result = Interop.accesskit_windows_adapter_handle_wm_getobject(
            _handle, wParam, lParam, _activationThunk, IntPtr.Zero);
        return result.HasValue != 0 ? result.Value : (IntPtr?)null;
    }

    /// <summary>Push a tree update if an AT is currently listening. <paramref name="factory"/> runs only when active.</summary>
    public void UpdateIfActive(Func<TreeUpdate> factory)
    {
        if (_handle == IntPtr.Zero) return;
        TreeUpdateFactory thunk = _ => factory().Release();
        IntPtr queued = Interop.accesskit_windows_adapter_update_if_active(_handle, thunk, IntPtr.Zero);
        RaiseQueued(queued);
        GC.KeepAlive(thunk);
    }

    /// <summary>Inform AccessKit that the window's focus state changed.</summary>
    public void UpdateWindowFocusState(bool isFocused)
    {
        if (_handle == IntPtr.Zero) return;
        IntPtr queued = Interop.accesskit_windows_adapter_update_window_focus_state(_handle, isFocused);
        RaiseQueued(queued);
    }

    /// <summary>A debug representation of the adapter.</summary>
    public string? Debug() => _handle == IntPtr.Zero ? null : Interop.TakeString(Interop.accesskit_windows_adapter_debug(_handle));

    private static void RaiseQueued(IntPtr queued)
    {
        if (queued != IntPtr.Zero)
            Interop.accesskit_windows_queued_events_raise(queued);
    }

    private IntPtr OnActivation(IntPtr userdata) => _buildInitialTree().Release();

    private void OnAction(IntPtr request, IntPtr userdata)
    {
        try { _onAction(new ActionRequest(request)); }
        finally { Interop.accesskit_action_request_free(request); }
    }

    public void Dispose()
    {
        if (_handle != IntPtr.Zero)
        {
            Interop.accesskit_windows_adapter_free(_handle);
            _handle = IntPtr.Zero;
        }
    }
}
