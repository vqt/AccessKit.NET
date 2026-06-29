using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace AccessKit;

/// <summary>
/// A single accessibility node (one UI element). Build it, set its properties, then hand it to a
/// <see cref="TreeUpdate"/> via <see cref="TreeUpdate.PushNode"/> — which takes ownership.
///
/// <para>The bulk of this type's single-value properties live in <c>Node.Generated.cs</c>; this file
/// holds the lifetime members and everything that needs custom marshaling (strings, node-id lists,
/// custom actions, the transform, and the action set).</para>
/// </summary>
public sealed partial class Node : IDisposable
{
    private IntPtr _handle;

    public Node(Role role) => _handle = Interop.accesskit_node_new(role);

    /// <summary>The node's semantic role.</summary>
    public Role Role
    {
        get => Interop.accesskit_node_role(_handle);
        set => Interop.accesskit_node_set_role(_handle, value);
    }

    /// <summary>A debug representation of the node (for diagnostics).</summary>
    public string? Debug() => Interop.TakeString(Interop.accesskit_node_debug(_handle));

    // ---- actions ----

    /// <summary>Declare that this node supports an action (e.g. <see cref="AkAction.Click"/>).</summary>
    public void AddAction(AkAction action) => Interop.accesskit_node_add_action(_handle, action);

    /// <summary>Remove support for an action.</summary>
    public void RemoveAction(AkAction action) => Interop.accesskit_node_remove_action(_handle, action);

    /// <summary>Remove support for all actions.</summary>
    public void ClearActions() => Interop.accesskit_node_clear_actions(_handle);

    /// <summary>Whether this node supports the given action.</summary>
    public bool SupportsAction(AkAction action) => Interop.accesskit_node_supports_action(_handle, action);

    /// <summary>Declare an action supported on this node's direct children in the filtered tree.</summary>
    public void AddChildAction(AkAction action) => Interop.accesskit_node_add_child_action(_handle, action);

    /// <summary>Remove a child-supported action.</summary>
    public void RemoveChildAction(AkAction action) => Interop.accesskit_node_remove_child_action(_handle, action);

    /// <summary>Remove all child-supported actions.</summary>
    public void ClearChildActions() => Interop.accesskit_node_clear_child_actions(_handle);

    /// <summary>Whether the given action is supported on this node's direct children.</summary>
    public bool ChildSupportsAction(AkAction action) => Interop.accesskit_node_child_supports_action(_handle, action);

    // ---- children & other node-id relations ----

    /// <summary>Append a child by id. Children must also be pushed into the same <see cref="TreeUpdate"/>.</summary>
    public void AddChild(ulong childId) => Interop.accesskit_node_push_child(_handle, childId);

    /// <summary>The node's children, in order.</summary>
    public ulong[] Children
    {
        get => Interop.accesskit_node_children(_handle).ToArray();
        set => SetIds(Interop.accesskit_node_set_children, value);
    }
    /// <summary>Remove all children.</summary>
    public void ClearChildren() => Interop.accesskit_node_clear_children(_handle);

    public ulong[] Controls { get => Interop.accesskit_node_controls(_handle).ToArray(); set => SetIds(Interop.accesskit_node_set_controls, value); }
    public void AddControlled(ulong id) => Interop.accesskit_node_push_controlled(_handle, id);
    public void ClearControls() => Interop.accesskit_node_clear_controls(_handle);

    public ulong[] Details { get => Interop.accesskit_node_details(_handle).ToArray(); set => SetIds(Interop.accesskit_node_set_details, value); }
    public void AddDetail(ulong id) => Interop.accesskit_node_push_detail(_handle, id);
    public void ClearDetails() => Interop.accesskit_node_clear_details(_handle);

    public ulong[] DescribedBy { get => Interop.accesskit_node_described_by(_handle).ToArray(); set => SetIds(Interop.accesskit_node_set_described_by, value); }
    public void AddDescribedBy(ulong id) => Interop.accesskit_node_push_described_by(_handle, id);
    public void ClearDescribedBy() => Interop.accesskit_node_clear_described_by(_handle);

    public ulong[] FlowTo { get => Interop.accesskit_node_flow_to(_handle).ToArray(); set => SetIds(Interop.accesskit_node_set_flow_to, value); }
    public void AddFlowTo(ulong id) => Interop.accesskit_node_push_flow_to(_handle, id);
    public void ClearFlowTo() => Interop.accesskit_node_clear_flow_to(_handle);

    public ulong[] LabelledBy { get => Interop.accesskit_node_labelled_by(_handle).ToArray(); set => SetIds(Interop.accesskit_node_set_labelled_by, value); }
    public void AddLabelledBy(ulong id) => Interop.accesskit_node_push_labelled_by(_handle, id);
    public void ClearLabelledBy() => Interop.accesskit_node_clear_labelled_by(_handle);

    public ulong[] Owns { get => Interop.accesskit_node_owns(_handle).ToArray(); set => SetIds(Interop.accesskit_node_set_owns, value); }
    public void AddOwned(ulong id) => Interop.accesskit_node_push_owned(_handle, id);
    public void ClearOwns() => Interop.accesskit_node_clear_owns(_handle);

    public ulong[] RadioGroup { get => Interop.accesskit_node_radio_group(_handle).ToArray(); set => SetIds(Interop.accesskit_node_set_radio_group, value); }
    public void AddToRadioGroup(ulong id) => Interop.accesskit_node_push_to_radio_group(_handle, id);
    public void ClearRadioGroup() => Interop.accesskit_node_clear_radio_group(_handle);

    private delegate void SetIdsFn(IntPtr node, UIntPtr length, ulong[] values);
    private void SetIds(SetIdsFn fn, ulong[] values)
    {
        values ??= Array.Empty<ulong>();
        fn(_handle, (UIntPtr)(uint)values.Length, values);
    }

    // ---- string properties ----

    /// <summary>The accessible name announced by screen readers (e.g. a button's caption).</summary>
    public string? Label { get => GetStr(Interop.accesskit_node_label); set => SetStr(Interop.accesskit_node_set_label_with_length, Interop.accesskit_node_clear_label, value); }
    public string? Description { get => GetStr(Interop.accesskit_node_description); set => SetStr(Interop.accesskit_node_set_description_with_length, Interop.accesskit_node_clear_description, value); }
    /// <summary>The node's value (e.g. the text of a text field).</summary>
    public string? Value { get => GetStr(Interop.accesskit_node_value); set => SetStr(Interop.accesskit_node_set_value_with_length, Interop.accesskit_node_clear_value, value); }
    public string? AccessKey { get => GetStr(Interop.accesskit_node_access_key); set => SetStr(Interop.accesskit_node_set_access_key_with_length, Interop.accesskit_node_clear_access_key, value); }
    public string? AuthorId { get => GetStr(Interop.accesskit_node_author_id); set => SetStr(Interop.accesskit_node_set_author_id_with_length, Interop.accesskit_node_clear_author_id, value); }
    public string? ClassName { get => GetStr(Interop.accesskit_node_class_name); set => SetStr(Interop.accesskit_node_set_class_name_with_length, Interop.accesskit_node_clear_class_name, value); }
    public string? FontFamily { get => GetStr(Interop.accesskit_node_font_family); set => SetStr(Interop.accesskit_node_set_font_family_with_length, Interop.accesskit_node_clear_font_family, value); }
    public string? HtmlTag { get => GetStr(Interop.accesskit_node_html_tag); set => SetStr(Interop.accesskit_node_set_html_tag_with_length, Interop.accesskit_node_clear_html_tag, value); }
    public string? InnerHtml { get => GetStr(Interop.accesskit_node_inner_html); set => SetStr(Interop.accesskit_node_set_inner_html_with_length, Interop.accesskit_node_clear_inner_html, value); }
    public string? KeyboardShortcut { get => GetStr(Interop.accesskit_node_keyboard_shortcut); set => SetStr(Interop.accesskit_node_set_keyboard_shortcut_with_length, Interop.accesskit_node_clear_keyboard_shortcut, value); }
    public string? Language { get => GetStr(Interop.accesskit_node_language); set => SetStr(Interop.accesskit_node_set_language_with_length, Interop.accesskit_node_clear_language, value); }
    public string? Placeholder { get => GetStr(Interop.accesskit_node_placeholder); set => SetStr(Interop.accesskit_node_set_placeholder_with_length, Interop.accesskit_node_clear_placeholder, value); }
    public string? RoleDescription { get => GetStr(Interop.accesskit_node_role_description); set => SetStr(Interop.accesskit_node_set_role_description_with_length, Interop.accesskit_node_clear_role_description, value); }
    public string? StateDescription { get => GetStr(Interop.accesskit_node_state_description); set => SetStr(Interop.accesskit_node_set_state_description_with_length, Interop.accesskit_node_clear_state_description, value); }
    public string? Tooltip { get => GetStr(Interop.accesskit_node_tooltip); set => SetStr(Interop.accesskit_node_set_tooltip_with_length, Interop.accesskit_node_clear_tooltip, value); }
    public string? Url { get => GetStr(Interop.accesskit_node_url); set => SetStr(Interop.accesskit_node_set_url_with_length, Interop.accesskit_node_clear_url, value); }
    public string? RowIndexText { get => GetStr(Interop.accesskit_node_row_index_text); set => SetStr(Interop.accesskit_node_set_row_index_text_with_length, Interop.accesskit_node_clear_row_index_text, value); }
    public string? ColumnIndexText { get => GetStr(Interop.accesskit_node_column_index_text); set => SetStr(Interop.accesskit_node_set_column_index_text_with_length, Interop.accesskit_node_clear_column_index_text, value); }
    public string? BrailleLabel { get => GetStr(Interop.accesskit_node_braille_label); set => SetStr(Interop.accesskit_node_set_braille_label_with_length, Interop.accesskit_node_clear_braille_label, value); }
    public string? BrailleRoleDescription { get => GetStr(Interop.accesskit_node_braille_role_description); set => SetStr(Interop.accesskit_node_set_braille_role_description_with_length, Interop.accesskit_node_clear_braille_role_description, value); }

    /// <summary>Convenience for the common case of setting just the label.</summary>
    public void SetLabel(string label) => Label = label;
    /// <summary>Convenience for the common case of setting just the value.</summary>
    public void SetValue(string value) => Value = value;

    private delegate IntPtr GetStrFn(IntPtr node);
    private delegate void SetStrFn(IntPtr node, byte[] value, UIntPtr length);
    private delegate void ClearFn(IntPtr node);

    private string? GetStr(GetStrFn get) => Interop.TakeString(get(_handle));
    private void SetStr(SetStrFn set, ClearFn clear, string? value)
    {
        if (value is null) { clear(_handle); return; }
        var bytes = Interop.Utf8(value);
        set(_handle, bytes, (UIntPtr)(uint)bytes.Length);
    }

    // ---- text layout arrays ----

    public byte[] CharacterLengths { get => Interop.accesskit_node_character_lengths(_handle).ToArray(); set => SetBytes(Interop.accesskit_node_set_character_lengths, value); }
    public void ClearCharacterLengths() => Interop.accesskit_node_clear_character_lengths(_handle);

    public byte[] WordStarts { get => Interop.accesskit_node_word_starts(_handle).ToArray(); set => SetBytes(Interop.accesskit_node_set_word_starts, value); }
    public void ClearWordStarts() => Interop.accesskit_node_clear_word_starts(_handle);

    public float[]? CharacterPositions { get => Interop.accesskit_node_character_positions(_handle).ToArray(); set => SetFloats(Interop.accesskit_node_set_character_positions, value); }
    public void ClearCharacterPositions() => Interop.accesskit_node_clear_character_positions(_handle);

    public float[]? CharacterWidths { get => Interop.accesskit_node_character_widths(_handle).ToArray(); set => SetFloats(Interop.accesskit_node_set_character_widths, value); }
    public void ClearCharacterWidths() => Interop.accesskit_node_clear_character_widths(_handle);

    private delegate void SetBytesFn(IntPtr node, UIntPtr length, byte[] values);
    private void SetBytes(SetBytesFn fn, byte[] values)
    {
        values ??= Array.Empty<byte>();
        fn(_handle, (UIntPtr)(uint)values.Length, values);
    }

    private delegate void SetFloatsFn(IntPtr node, UIntPtr length, float[] values);
    private void SetFloats(SetFloatsFn fn, float[]? values)
    {
        values ??= Array.Empty<float>();
        fn(_handle, (UIntPtr)(uint)values.Length, values);
    }

    // ---- transform (affine) ----

    /// <summary>An affine transform applied to this node's coordinate space (null = none).</summary>
    public Affine? Transform
    {
        get
        {
            IntPtr p = Interop.accesskit_node_transform(_handle);
            return p == IntPtr.Zero ? (Affine?)null : Marshal.PtrToStructure<Affine>(p);
        }
        set
        {
            if (value.HasValue) Interop.accesskit_node_set_transform(_handle, value.Value);
            else Interop.accesskit_node_clear_transform(_handle);
        }
    }

    // ---- custom actions ----

    /// <summary>
    /// Append a custom action. Takes ownership of <paramref name="action"/> (do not dispose it
    /// afterwards). Declare <see cref="AkAction.CustomAction"/> on the node as well.
    /// </summary>
    public void AddCustomAction(CustomAction action) =>
        Interop.accesskit_node_push_custom_action(_handle, action.Release());

    /// <summary>Replace the node's custom actions. Takes ownership of each <see cref="CustomAction"/>.</summary>
    public void SetCustomActions(IReadOnlyList<CustomAction> actions)
    {
        if (actions is null) { ClearCustomActions(); return; }
        var ptrs = new IntPtr[actions.Count];
        for (int i = 0; i < actions.Count; i++) ptrs[i] = actions[i].Release();

        // The native function copies the pointer array (taking ownership of the actions they point
        // to), so a short-lived unmanaged buffer is sufficient.
        IntPtr buf = Marshal.AllocHGlobal(IntPtr.Size * ptrs.Length);
        try
        {
            Marshal.Copy(ptrs, 0, buf, ptrs.Length);
            Interop.accesskit_node_set_custom_actions(_handle, (UIntPtr)(uint)ptrs.Length, buf);
        }
        finally
        {
            Marshal.FreeHGlobal(buf);
        }
    }

    /// <summary>Remove all custom actions.</summary>
    public void ClearCustomActions() => Interop.accesskit_node_clear_custom_actions(_handle);

    // ---- lifetime ----

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
