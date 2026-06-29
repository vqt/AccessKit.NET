using System;

namespace AccessKit.Macos;

/// <summary>
/// Wraps <c>accesskit_macos_adapter</c> — the low-level macOS adapter for applications that
/// implement the <c>NSAccessibility</c> protocol on their own <c>NSView</c> subclass. Forward the
/// view's accessibility messages to <see cref="ViewChildren"/>, <see cref="Focus"/> and
/// <see cref="HitTest"/>, and keep focus in sync via <see cref="UpdateViewFocusState"/>.
///
/// <para>All methods must be called on the main thread. The pointers returned by the bridge methods
/// are autoreleased Cocoa objects owned by AccessKit — return them directly from your
/// <c>NSAccessibility</c> overrides; do not release them.</para>
/// </summary>
public sealed class MacosAdapter : IDisposable
{
    private IntPtr _handle;

    // Held as fields so the native side can invoke them at any time without the delegates being
    // collected or relocated.
    private readonly ActionHandlerCallback _actionThunk;
    private readonly ActivationHandlerCallback _activationThunk;

    private readonly Func<TreeUpdate> _buildInitialTree;
    private readonly Action<ActionRequest> _onAction;

    /// <param name="nsView">A valid, unreleased pointer to the host <c>NSView</c>.</param>
    /// <param name="isViewFocused">Whether the view currently has keyboard focus.</param>
    /// <param name="buildInitialTree">Builds the full tree the first time an AT connects.</param>
    /// <param name="onAction">Invoked on the main thread when an AT requests an action.</param>
    public MacosAdapter(IntPtr nsView, bool isViewFocused, Func<TreeUpdate> buildInitialTree, Action<ActionRequest> onAction)
    {
        _buildInitialTree = buildInitialTree ?? throw new ArgumentNullException(nameof(buildInitialTree));
        _onAction = onAction ?? throw new ArgumentNullException(nameof(onAction));

        _activationThunk = OnActivation;
        _actionThunk = OnAction;

        _handle = Interop.accesskit_macos_adapter_new(nsView, isViewFocused, _actionThunk, IntPtr.Zero);
        if (_handle == IntPtr.Zero)
            throw new InvalidOperationException("accesskit_macos_adapter_new returned null.");
    }

    /// <summary>Push a tree update if an AT is currently listening. <paramref name="factory"/> runs only when active.</summary>
    public void UpdateIfActive(Func<TreeUpdate> factory)
    {
        if (_handle == IntPtr.Zero) return;
        TreeUpdateFactory thunk = _ => factory().Release();
        IntPtr queued = Interop.accesskit_macos_adapter_update_if_active(_handle, thunk, IntPtr.Zero);
        MacosQueuedEvents.Raise(queued);
        GC.KeepAlive(thunk);
    }

    /// <summary>Inform AccessKit that the view's focus state changed.</summary>
    public void UpdateViewFocusState(bool isFocused)
    {
        if (_handle == IntPtr.Zero) return;
        MacosQueuedEvents.Raise(Interop.accesskit_macos_adapter_update_view_focus_state(_handle, isFocused));
    }

    /// <summary>
    /// Returns an <c>NSArray</c> of the view's accessibility children — return it from your
    /// <c>accessibilityChildren</c> override. Ownership is not transferred.
    /// </summary>
    public IntPtr ViewChildren() =>
        _handle == IntPtr.Zero ? IntPtr.Zero
            : Interop.accesskit_macos_adapter_view_children(_handle, _activationThunk, IntPtr.Zero);

    /// <summary>
    /// Returns the focused accessibility element (an <c>NSObject</c>) — return it from your
    /// <c>accessibilityFocusedUIElement</c> override. Ownership is not transferred.
    /// </summary>
    public IntPtr Focus() =>
        _handle == IntPtr.Zero ? IntPtr.Zero
            : Interop.accesskit_macos_adapter_focus(_handle, _activationThunk, IntPtr.Zero);

    /// <summary>
    /// Returns the accessibility element at the given screen point (an <c>NSObject</c>), or null —
    /// return it from your <c>accessibilityHitTest:</c> override. Ownership is not transferred.
    /// </summary>
    public IntPtr HitTest(double x, double y) =>
        _handle == IntPtr.Zero ? IntPtr.Zero
            : Interop.accesskit_macos_adapter_hit_test(_handle, x, y, _activationThunk, IntPtr.Zero);

    /// <summary>A debug representation of the adapter.</summary>
    public string? Debug() => _handle == IntPtr.Zero ? null : Interop.TakeString(Interop.accesskit_macos_adapter_debug(_handle));

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
            Interop.accesskit_macos_adapter_free(_handle);
            _handle = IntPtr.Zero;
        }
    }
}

/// <summary>Internal helper for raising the queued events returned by the macOS adapters.</summary>
internal static class MacosQueuedEvents
{
    public static void Raise(IntPtr queued)
    {
        if (queued != IntPtr.Zero)
            Interop.accesskit_macos_queued_events_raise(queued);
    }
}
