using System;

namespace AccessKit.Windows;

/// <summary>
/// Wraps <c>accesskit_windows_subclassing_adapter</c>. It subclasses the given window's procedure to
/// answer UI Automation's <c>WM_GETOBJECT</c>, so no manual wnd-proc hooking is required.
///
/// <para><b>Important:</b> the native adapter panics if the window is already visible when constructed.
/// Create the window hidden, construct this adapter, then show the window.</para>
///
/// <para>The adapter is lazy: <see cref="UpdateIfActive"/> does nothing until an assistive-technology
/// client connects, at which point the activation handler supplies the initial tree.</para>
/// </summary>
public sealed class WindowsSubclassingAdapter : IDisposable
{
    private IntPtr _handle;

    // These delegates are passed to native code and must outlive every native call that can invoke
    // them, so they are held as fields to keep them from being collected or relocated.
    private readonly ActivationHandlerCallback _activationThunk;
    private readonly ActionHandlerCallback _actionThunk;

    private readonly Func<TreeUpdate> _buildInitialTree;
    private readonly Action<ActionRequest> _onAction;

    /// <param name="hwnd">The native window handle (HWND). The window must not be visible yet.</param>
    /// <param name="buildInitialTree">Builds the full tree the first time an AT connects.</param>
    /// <param name="onAction">Invoked when an AT requests an action (may run on a UIA thread).</param>
    public WindowsSubclassingAdapter(IntPtr hwnd, Func<TreeUpdate> buildInitialTree, Action<ActionRequest> onAction)
    {
        _buildInitialTree = buildInitialTree ?? throw new ArgumentNullException(nameof(buildInitialTree));
        _onAction = onAction ?? throw new ArgumentNullException(nameof(onAction));

        _activationThunk = OnActivation;
        _actionThunk = OnAction;

        _handle = Interop.accesskit_windows_subclassing_adapter_new(
            hwnd, _activationThunk, IntPtr.Zero, _actionThunk, IntPtr.Zero);

        if (_handle == IntPtr.Zero)
            throw new InvalidOperationException("accesskit_windows_subclassing_adapter_new returned null.");
    }

    /// <summary>
    /// Push a tree update if an AT is currently listening. <paramref name="factory"/> is only invoked
    /// when active, so it is cheap to call every time UI state changes.
    /// </summary>
    public void UpdateIfActive(Func<TreeUpdate> factory)
    {
        if (_handle == IntPtr.Zero) return;

        TreeUpdateFactory thunk = _ => factory().Release();
        IntPtr queued = Interop.accesskit_windows_subclassing_adapter_update_if_active(_handle, thunk, IntPtr.Zero);
        if (queued != IntPtr.Zero)
            Interop.accesskit_windows_queued_events_raise(queued);
        GC.KeepAlive(thunk);
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
            Interop.accesskit_windows_subclassing_adapter_free(_handle);
            _handle = IntPtr.Zero;
        }
    }
}
