using System;
using System.Runtime.InteropServices;

namespace AccessKit;

/// <summary>The kind of payload carried by an <see cref="ActionRequest"/>. Mirrors <c>accesskit_action_data_Tag</c>.</summary>
public enum ActionDataKind
{
    CustomAction = 0,
    Value = 1,
    NumericValue = 2,
    ScrollUnit = 3,
    ScrollHint = 4,
    ScrollToPoint = 5,
    SetScrollOffset = 6,
    SetTextSelection = 7,
}

/// <summary>
/// A request from assistive technology to perform an action on a node (e.g. a screen reader
/// activating a button, or setting a control's value). Decoded from the native
/// <c>accesskit_action_request</c> struct, including its optional tagged-union payload.
/// </summary>
public sealed class ActionRequest
{
    /// <summary>The requested action.</summary>
    public AkAction Action { get; }

    /// <summary>The id of the tree the action targets.</summary>
    public TreeId TargetTree { get; }

    /// <summary>The id of the node the action targets.</summary>
    public ulong TargetNode { get; }

    /// <summary>The kind of payload, or null if the request carries no data.</summary>
    public ActionDataKind? DataKind { get; }

    /// <summary>Payload for <see cref="AkAction.CustomAction"/> — the custom action's id.</summary>
    public int? CustomActionId { get; }

    /// <summary>Payload for <see cref="AkAction.SetValue"/> / replace-text — the new string value.</summary>
    public string? Value { get; }

    /// <summary>Payload for a numeric <see cref="AkAction.SetValue"/> — the new numeric value.</summary>
    public double? NumericValue { get; }

    /// <summary>Payload for the scroll-by-unit actions.</summary>
    public ScrollUnit? ScrollUnit { get; }

    /// <summary>Payload for <see cref="AkAction.ScrollIntoView"/> — the preferred resting position.</summary>
    public ScrollHint? ScrollHint { get; }

    /// <summary>Payload for <see cref="AkAction.ScrollToPoint"/> — the target point in the tree's container.</summary>
    public Point? ScrollToPoint { get; }

    /// <summary>Payload for <see cref="AkAction.SetScrollOffset"/> — the new scroll offset.</summary>
    public Point? SetScrollOffset { get; }

    /// <summary>Payload for <see cref="AkAction.SetTextSelection"/> — the requested selection.</summary>
    public TextSelection? SetTextSelection { get; }

    // accesskit_action_request layout (win-x64):
    //   offset  0 : accesskit_action       action       (u8)
    //   offset  1 : accesskit_tree_id       target_tree  (u8[16], align 1)
    //   offset 24 : accesskit_node_id       target_node  (u64, align 8 -> padded from 17)
    //   offset 32 : accesskit_opt_action_data data
    //                 +0  bool   has_value
    //                 +8  action_data { Tag tag (+0, i32); union value (+8) }   -> tag at 40, union at 48
    internal ActionRequest(IntPtr request)
    {
        Action = (AkAction)Marshal.ReadByte(request, 0);

        var treeBytes = new byte[16];
        Marshal.Copy(request + 1, treeBytes, 0, 16);
        TargetTree = new TreeId(treeBytes);

        TargetNode = unchecked((ulong)Marshal.ReadInt64(request, 24));

        bool hasData = Marshal.ReadByte(request, 32) != 0;
        if (!hasData) return;

        const int TagOffset = 40;
        const int UnionOffset = 48;
        var kind = (ActionDataKind)Marshal.ReadInt32(request, TagOffset);
        DataKind = kind;

        switch (kind)
        {
            case ActionDataKind.CustomAction:
                CustomActionId = Marshal.ReadInt32(request, UnionOffset);
                break;
            case ActionDataKind.Value:
                Value = Interop.ReadStringUtf8(Marshal.ReadIntPtr(request, UnionOffset));
                break;
            case ActionDataKind.NumericValue:
                NumericValue = ReadDouble(request, UnionOffset);
                break;
            case ActionDataKind.ScrollUnit:
                ScrollUnit = (ScrollUnit)Marshal.ReadByte(request, UnionOffset);
                break;
            case ActionDataKind.ScrollHint:
                ScrollHint = (ScrollHint)Marshal.ReadByte(request, UnionOffset);
                break;
            case ActionDataKind.ScrollToPoint:
                ScrollToPoint = ReadPoint(request, UnionOffset);
                break;
            case ActionDataKind.SetScrollOffset:
                SetScrollOffset = ReadPoint(request, UnionOffset);
                break;
            case ActionDataKind.SetTextSelection:
                SetTextSelection = ReadTextSelection(request, UnionOffset);
                break;
        }
    }

    private static double ReadDouble(IntPtr p, int off) =>
        BitConverter.Int64BitsToDouble(Marshal.ReadInt64(p, off));

    private static Point ReadPoint(IntPtr p, int off) =>
        new Point(ReadDouble(p, off), ReadDouble(p, off + 8));

    private static TextPosition ReadTextPosition(IntPtr p, int off) =>
        new TextPosition(
            unchecked((ulong)Marshal.ReadInt64(p, off)),
            (int)(uint)(ulong)Marshal.ReadIntPtr(p, off + 8));

    private static TextSelection ReadTextSelection(IntPtr p, int off) =>
        new TextSelection(ReadTextPosition(p, off), ReadTextPosition(p, off + 16));
}
