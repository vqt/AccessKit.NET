using System;
using System.Runtime.InteropServices;

namespace AccessKit;

/// <summary>
/// A request from assistive technology to perform an action on a node (e.g. a screen reader
/// activating a button). Decoded from the native <c>accesskit_action_request</c> struct.
/// </summary>
public sealed class ActionRequest
{
    /// <summary>The requested action.</summary>
    public AkAction Action { get; }

    /// <summary>The id of the node the action targets.</summary>
    public ulong TargetNode { get; }

    // accesskit_action_request layout (action data omitted — not needed yet):
    //   offset 0  : accesskit_action  action      (u8)
    //   offset 1  : accesskit_tree_id target_tree (u8[16], align 1)
    //   offset 24 : accesskit_node_id target_node (u64, align 8 -> padded from 17)
    internal ActionRequest(IntPtr request)
    {
        Action = (AkAction)Marshal.ReadByte(request, 0);
        TargetNode = unchecked((ulong)Marshal.ReadInt64(request, 24));
    }
}
