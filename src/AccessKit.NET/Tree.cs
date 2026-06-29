using System;

namespace AccessKit;

/// <summary>
/// Tree-wide metadata, identified by its root node id. Handed to a <see cref="TreeUpdate"/> via
/// <see cref="TreeUpdate.SetTree"/> (which takes ownership) on the first update.
/// </summary>
public sealed class Tree : IDisposable
{
    private IntPtr _handle;

    public Tree(ulong rootId) => _handle = Interop.accesskit_tree_new(rootId);

    /// <summary>Relinquish ownership of the native tree (called when attached to a tree update).</summary>
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
            Interop.accesskit_tree_free(_handle);
            _handle = IntPtr.Zero;
        }
    }
}
