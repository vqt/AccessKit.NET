using System;
using System.Runtime.InteropServices;

namespace AccessKit;

/// <summary>
/// Builds and returns ownership of an initial <c>accesskit_tree_update*</c> when an assistive
/// technology first connects. Mirrors <c>accesskit_activation_handler_callback</c>.
/// </summary>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr ActivationHandlerCallback(IntPtr userdata);

/// <summary>
/// Receives an <c>accesskit_action_request*</c> (ownership transferred — must be freed) when an AT
/// requests an action. Mirrors <c>accesskit_action_handler_callback</c>.
/// </summary>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void ActionHandlerCallback(IntPtr request, IntPtr userdata);

/// <summary>
/// Builds and returns ownership of an <c>accesskit_tree_update*</c> describing what changed.
/// Mirrors <c>accesskit_tree_update_factory</c>.
/// </summary>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr TreeUpdateFactory(IntPtr userdata);
