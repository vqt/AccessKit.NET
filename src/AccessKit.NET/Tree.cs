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

    /// <summary>The name of the UI toolkit driving this tree (optional, for diagnostics).</summary>
    public string? ToolkitName
    {
        get => Interop.TakeString(Interop.accesskit_tree_get_toolkit_name(_handle));
        set
        {
            if (value is null) { Interop.accesskit_tree_clear_toolkit_name(_handle); return; }
            var bytes = Interop.Utf8(value);
            Interop.accesskit_tree_set_toolkit_name_with_length(_handle, bytes, (UIntPtr)(uint)bytes.Length);
        }
    }

    /// <summary>The version of the UI toolkit driving this tree (optional, for diagnostics).</summary>
    public string? ToolkitVersion
    {
        get => Interop.TakeString(Interop.accesskit_tree_get_toolkit_version(_handle));
        set
        {
            if (value is null) { Interop.accesskit_tree_clear_toolkit_version(_handle); return; }
            var bytes = Interop.Utf8(value);
            Interop.accesskit_tree_set_toolkit_version_with_length(_handle, bytes, (UIntPtr)(uint)bytes.Length);
        }
    }

    /// <summary>A debug representation of the tree metadata.</summary>
    public string? Debug() => Interop.TakeString(Interop.accesskit_tree_debug(_handle));

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
