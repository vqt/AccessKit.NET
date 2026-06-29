using System;

namespace AccessKit;

/// <summary>
/// A custom action a node can advertise (beyond the built-in <see cref="AkAction"/> set), identified
/// by an integer id and a human-readable description. Mirrors <c>accesskit_custom_action</c>.
///
/// <para>Ownership transfers to a <see cref="Node"/> when added via
/// <see cref="Node.AddCustomAction"/> / <see cref="Node.SetCustomActions"/>; do not dispose it after
/// that. Until then, dispose it to free the native object.</para>
/// </summary>
public sealed class CustomAction : IDisposable
{
    private IntPtr _handle;

    public CustomAction(int id) => _handle = Interop.accesskit_custom_action_new(id);

    public CustomAction(int id, string description) : this(id) => Description = description;

    /// <summary>The action's integer id, unique within the node's custom-action set.</summary>
    public int Id
    {
        get => Interop.accesskit_custom_action_id(_handle);
        set => Interop.accesskit_custom_action_set_id(_handle, value);
    }

    /// <summary>The action's human-readable description, announced by assistive technology.</summary>
    public string? Description
    {
        get => Interop.TakeString(Interop.accesskit_custom_action_description(_handle));
        set
        {
            var bytes = Interop.Utf8(value ?? string.Empty);
            Interop.accesskit_custom_action_set_description_with_length(_handle, bytes, (UIntPtr)(uint)bytes.Length);
        }
    }

    /// <summary>Relinquish ownership of the native action (called when added to a node).</summary>
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
            Interop.accesskit_custom_action_free(_handle);
            _handle = IntPtr.Zero;
        }
    }
}
