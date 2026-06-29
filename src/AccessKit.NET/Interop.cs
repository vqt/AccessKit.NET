using System;
using System.Runtime.InteropServices;

namespace AccessKit;

/// <summary>
/// Raw P/Invoke surface for accesskit-c 0.22.2. Names mirror the C API one-to-one.
/// Prefer the idiomatic wrappers (<see cref="Node"/>, <see cref="Tree"/>,
/// <see cref="TreeUpdate"/>, <see cref="Windows.WindowsSubclassingAdapter"/>) over calling these directly.
/// </summary>
internal static class Interop
{
    // The native library is shipped as accesskit.dll (win-x64). The runtime maps the
    // bare name to the platform file (accesskit.dll / libaccesskit.so / libaccesskit.dylib).
    private const string Lib = "accesskit";
    private const CallingConvention Cc = CallingConvention.Cdecl;

    // ---- node ----
    [DllImport(Lib, CallingConvention = Cc)] public static extern IntPtr accesskit_node_new(byte role);
    [DllImport(Lib, CallingConvention = Cc)] public static extern void accesskit_node_free(IntPtr node);
    [DllImport(Lib, CallingConvention = Cc)] public static extern void accesskit_node_set_label_with_length(IntPtr node, byte[] value, UIntPtr length);
    [DllImport(Lib, CallingConvention = Cc)] public static extern void accesskit_node_set_value_with_length(IntPtr node, byte[] value, UIntPtr length);
    [DllImport(Lib, CallingConvention = Cc)] public static extern void accesskit_node_add_action(IntPtr node, byte action);
    [DllImport(Lib, CallingConvention = Cc)] public static extern void accesskit_node_set_selected(IntPtr node, [MarshalAs(UnmanagedType.I1)] bool value);
    [DllImport(Lib, CallingConvention = Cc)] public static extern void accesskit_node_push_child(IntPtr node, ulong child);
    [DllImport(Lib, CallingConvention = Cc)] public static extern void accesskit_node_set_bounds(IntPtr node, Rect value);

    // ---- tree ----
    [DllImport(Lib, CallingConvention = Cc)] public static extern IntPtr accesskit_tree_new(ulong root);
    [DllImport(Lib, CallingConvention = Cc)] public static extern void accesskit_tree_free(IntPtr tree);

    // ---- tree update ----
    [DllImport(Lib, CallingConvention = Cc)] public static extern IntPtr accesskit_tree_update_with_focus(ulong focus);
    [DllImport(Lib, CallingConvention = Cc)] public static extern IntPtr accesskit_tree_update_with_capacity_and_focus(UIntPtr capacity, ulong focus);
    [DllImport(Lib, CallingConvention = Cc)] public static extern void accesskit_tree_update_free(IntPtr update);
    [DllImport(Lib, CallingConvention = Cc)] public static extern void accesskit_tree_update_push_node(IntPtr update, ulong id, IntPtr node);
    [DllImport(Lib, CallingConvention = Cc)] public static extern void accesskit_tree_update_set_tree(IntPtr update, IntPtr tree);
    [DllImport(Lib, CallingConvention = Cc)] public static extern void accesskit_tree_update_set_focus(IntPtr update, ulong focus);

    // ---- action request ----
    [DllImport(Lib, CallingConvention = Cc)] public static extern void accesskit_action_request_free(IntPtr request);

    // ---- windows subclassing adapter ----
    [DllImport(Lib, CallingConvention = Cc)]
    public static extern IntPtr accesskit_windows_subclassing_adapter_new(
        IntPtr hwnd,
        ActivationHandlerCallback activationHandler, IntPtr activationUserdata,
        ActionHandlerCallback actionHandler, IntPtr actionUserdata);
    [DllImport(Lib, CallingConvention = Cc)] public static extern void accesskit_windows_subclassing_adapter_free(IntPtr adapter);
    [DllImport(Lib, CallingConvention = Cc)]
    public static extern IntPtr accesskit_windows_subclassing_adapter_update_if_active(
        IntPtr adapter, TreeUpdateFactory updateFactory, IntPtr userdata);

    // ---- windows queued events ----
    [DllImport(Lib, CallingConvention = Cc)] public static extern void accesskit_windows_queued_events_raise(IntPtr events);
}

/// <summary>Builds and returns ownership of an initial <c>accesskit_tree_update*</c> when an AT first connects.</summary>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr ActivationHandlerCallback(IntPtr userdata);

/// <summary>Receives an <c>accesskit_action_request*</c> (ownership transferred — must be freed) when an AT requests an action.</summary>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void ActionHandlerCallback(IntPtr request, IntPtr userdata);

/// <summary>Builds and returns ownership of an <c>accesskit_tree_update*</c> describing what changed.</summary>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr TreeUpdateFactory(IntPtr userdata);

/// <summary>A rectangle in the window's coordinate space (y-down). Matches <c>accesskit_rect</c>.</summary>
[StructLayout(LayoutKind.Sequential)]
public struct Rect
{
    public double X0, Y0, X1, Y1;
    public Rect(double x0, double y0, double x1, double y1) { X0 = x0; Y0 = y0; X1 = x1; Y1 = y1; }
}
