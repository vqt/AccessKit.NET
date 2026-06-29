using System;

namespace AccessKit.Macos;

/// <summary>
/// Wraps <c>accesskit_macos_subclassing_adapter</c>. It subclasses an <c>NSView</c> (or a window's
/// content view) so the <c>NSAccessibility</c> protocol is answered automatically — no manual
/// message forwarding required, unlike <see cref="MacosAdapter"/>.
///
/// <para>All methods must be called on the main thread.</para>
/// </summary>
public sealed class MacosSubclassingAdapter : IDisposable
{
    private IntPtr _handle;

    // Held as fields so the native side can invoke them at any time without the delegates being
    // collected or relocated.
    private readonly ActivationHandlerCallback _activationThunk;
    private readonly ActionHandlerCallback _actionThunk;

    private readonly Func<TreeUpdate> _buildInitialTree;
    private readonly Action<ActionRequest> _onAction;

    private MacosSubclassingAdapter(IntPtr viewOrWindow, bool forWindow, Func<TreeUpdate> buildInitialTree, Action<ActionRequest> onAction)
    {
        _buildInitialTree = buildInitialTree ?? throw new ArgumentNullException(nameof(buildInitialTree));
        _onAction = onAction ?? throw new ArgumentNullException(nameof(onAction));

        _activationThunk = OnActivation;
        _actionThunk = OnAction;

        _handle = forWindow
            ? Interop.accesskit_macos_subclassing_adapter_for_window(viewOrWindow, _activationThunk, IntPtr.Zero, _actionThunk, IntPtr.Zero)
            : Interop.accesskit_macos_subclassing_adapter_new(viewOrWindow, _activationThunk, IntPtr.Zero, _actionThunk, IntPtr.Zero);

        if (_handle == IntPtr.Zero)
            throw new InvalidOperationException("accesskit_macos_subclassing_adapter_* returned null.");
    }

    /// <summary>Subclass the given <c>NSView</c> directly.</summary>
    /// <param name="nsView">A valid, unreleased pointer to an <c>NSView</c>.</param>
    public static MacosSubclassingAdapter ForView(IntPtr nsView, Func<TreeUpdate> buildInitialTree, Action<ActionRequest> onAction) =>
        new MacosSubclassingAdapter(nsView, forWindow: false, buildInitialTree, onAction);

    /// <summary>
    /// Subclass the content view of the given <c>NSWindow</c>. The window must already have a
    /// content view (the native call panics otherwise).
    /// </summary>
    /// <param name="nsWindow">A valid, unreleased pointer to an <c>NSWindow</c>.</param>
    public static MacosSubclassingAdapter ForWindow(IntPtr nsWindow, Func<TreeUpdate> buildInitialTree, Action<ActionRequest> onAction) =>
        new MacosSubclassingAdapter(nsWindow, forWindow: true, buildInitialTree, onAction);

    /// <summary>Push a tree update if an AT is currently listening. <paramref name="factory"/> runs only when active.</summary>
    public void UpdateIfActive(Func<TreeUpdate> factory)
    {
        if (_handle == IntPtr.Zero) return;
        TreeUpdateFactory thunk = _ => factory().Release();
        IntPtr queued = Interop.accesskit_macos_subclassing_adapter_update_if_active(_handle, thunk, IntPtr.Zero);
        MacosQueuedEvents.Raise(queued);
        GC.KeepAlive(thunk);
    }

    /// <summary>Inform AccessKit that the view's focus state changed.</summary>
    public void UpdateViewFocusState(bool isFocused)
    {
        if (_handle == IntPtr.Zero) return;
        MacosQueuedEvents.Raise(Interop.accesskit_macos_subclassing_adapter_update_view_focus_state(_handle, isFocused));
    }

    /// <summary>
    /// Patches the given <c>NSWindow</c> subclass to forward <c>accessibilityFocusedUIElement</c> to
    /// its content view. Needed for windowing libraries (e.g. SDL) that put keyboard focus on the
    /// window rather than the content view. This cannot be undone, so the library must not be
    /// unloaded afterwards.
    /// </summary>
    public static void AddFocusForwarderToWindowClass(string className)
    {
        if (className is null) throw new ArgumentNullException(nameof(className));
        var bytes = Interop.Utf8(className);
        Interop.accesskit_macos_add_focus_forwarder_to_window_class_with_length(bytes, (UIntPtr)(uint)bytes.Length);
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
            Interop.accesskit_macos_subclassing_adapter_free(_handle);
            _handle = IntPtr.Zero;
        }
    }
}
