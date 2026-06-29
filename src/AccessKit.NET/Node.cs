using System;
using System.Text;

namespace AccessKit;

/// <summary>
/// A single accessibility node (one UI element). Build it, set its properties, then hand it to a
/// <see cref="TreeUpdate"/> via <see cref="TreeUpdate.PushNode"/> — which takes ownership.
/// </summary>
public sealed class Node : IDisposable
{
    private IntPtr _handle;

    public Node(Role role) => _handle = Interop.accesskit_node_new((byte)role);

    /// <summary>The accessible name announced by screen readers (e.g. a button's caption).</summary>
    public void SetLabel(string label)
    {
        var bytes = Encoding.UTF8.GetBytes(label);
        Interop.accesskit_node_set_label_with_length(_handle, bytes, (UIntPtr)bytes.Length);
    }

    /// <summary>The node's value (e.g. the text of a text field).</summary>
    public void SetValue(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        Interop.accesskit_node_set_value_with_length(_handle, bytes, (UIntPtr)bytes.Length);
    }

    /// <summary>Declare that this node supports an action (e.g. <see cref="AkAction.Click"/>).</summary>
    public void AddAction(AkAction action) => Interop.accesskit_node_add_action(_handle, (byte)action);

    /// <summary>Mark this node as selected (e.g. the active tab in a tab list).</summary>
    public void SetSelected(bool value) => Interop.accesskit_node_set_selected(_handle, value);

    /// <summary>Append a child by id. Children must also be pushed into the same <see cref="TreeUpdate"/>.</summary>
    public void AddChild(ulong childId) => Interop.accesskit_node_push_child(_handle, childId);

    /// <summary>Bounding box in the window's coordinate space (y-down).</summary>
    public void SetBounds(Rect bounds) => Interop.accesskit_node_set_bounds(_handle, bounds);

    /// <summary>Relinquish ownership of the native node (called when pushed into a tree update).</summary>
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
            Interop.accesskit_node_free(_handle);
            _handle = IntPtr.Zero;
        }
    }
}
