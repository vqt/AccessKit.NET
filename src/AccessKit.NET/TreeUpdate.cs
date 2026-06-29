using System;

namespace AccessKit;

/// <summary>
/// A batch of changes pushed to the platform adapter: the nodes that changed, optionally the tree
/// metadata (first update only), and which node has focus. Ownership is transferred to AccessKit when
/// returned from an activation handler or update factory — call <see cref="Release"/> at that point.
/// </summary>
public sealed class TreeUpdate : IDisposable
{
    private IntPtr _handle;

    private TreeUpdate(IntPtr handle) => _handle = handle;

    /// <summary>Create an update whose focus is the given node id.</summary>
    public static TreeUpdate WithFocus(ulong focus) =>
        new TreeUpdate(Interop.accesskit_tree_update_with_focus(focus));

    /// <summary>Create an update pre-sized for <paramref name="capacity"/> nodes.</summary>
    public static TreeUpdate WithCapacityAndFocus(int capacity, ulong focus) =>
        new TreeUpdate(Interop.accesskit_tree_update_with_capacity_and_focus((UIntPtr)(uint)capacity, focus));

    /// <summary>Append a node under the given id. Takes ownership of <paramref name="node"/>.</summary>
    public void PushNode(ulong id, Node node) =>
        Interop.accesskit_tree_update_push_node(_handle, id, node.Release());

    /// <summary>Attach tree metadata (required on the first update). Takes ownership of <paramref name="tree"/>.</summary>
    public void SetTree(Tree tree) =>
        Interop.accesskit_tree_update_set_tree(_handle, tree.Release());

    /// <summary>Set which node currently has accessibility focus.</summary>
    public void SetFocus(ulong focus) => Interop.accesskit_tree_update_set_focus(_handle, focus);

    /// <summary>Relinquish ownership of the native update so AccessKit can consume and free it.</summary>
    internal IntPtr Release()
    {
        var h = _handle;
        _handle = IntPtr.Zero;
        return h;
    }

    public void Dispose()
    {
        if (_handle != IntPtr.Zero)
        {
            Interop.accesskit_tree_update_free(_handle);
            _handle = IntPtr.Zero;
        }
    }
}
