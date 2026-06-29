using System;
using System.Runtime.InteropServices;

namespace AccessKit;

/// <summary>An 8-bit-per-channel sRGB color with alpha. Mirrors <c>accesskit_color</c>.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct Color
{
    public readonly byte Red;
    public readonly byte Green;
    public readonly byte Blue;
    public readonly byte Alpha;
    public Color(byte red, byte green, byte blue, byte alpha) { Red = red; Green = green; Blue = blue; Alpha = alpha; }

    public override string ToString() => $"Color(#{Red:X2}{Green:X2}{Blue:X2}{Alpha:X2})";
}

/// <summary>The style and color of a text decoration. Mirrors <c>accesskit_text_decoration</c>.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct TextDecoration
{
    public readonly TextDecorationStyle Style;
    public readonly Color Color;
    public TextDecoration(TextDecorationStyle style, Color color) { Style = style; Color = color; }
}

/// <summary>A position within a text run, as a node id and a character offset. Mirrors <c>accesskit_text_position</c>.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct TextPosition
{
    public readonly ulong Node;
    public readonly UIntPtr CharacterIndex;
    public TextPosition(ulong node, int characterIndex) { Node = node; CharacterIndex = (UIntPtr)(uint)characterIndex; }
    public int CharacterIndexValue => (int)(uint)CharacterIndex;
}

/// <summary>A text selection (caret/anchor pair). Mirrors <c>accesskit_text_selection</c>.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct TextSelection
{
    public readonly TextPosition Anchor;
    public readonly TextPosition Focus;
    public TextSelection(TextPosition anchor, TextPosition focus) { Anchor = anchor; Focus = focus; }
}

/// <summary>
/// A 128-bit tree identifier (a UUID in big-endian byte order). Mirrors <c>accesskit_tree_id</c>.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct TreeId
{
    private fixed byte _bytes[16];

    public TreeId(byte[] bytes)
    {
        if (bytes is null || bytes.Length != 16)
            throw new ArgumentException("TreeId requires exactly 16 bytes.", nameof(bytes));
        for (int i = 0; i < 16; i++) _bytes[i] = bytes[i];
    }

    public byte[] ToBytes()
    {
        var result = new byte[16];
        for (int i = 0; i < 16; i++) result[i] = _bytes[i];
        return result;
    }

    public Guid ToGuid() => new Guid(ToBytes());
}

// ----------------------------------------------------------------------------
// Internal interop-only structs. They keep an exact, blittable layout match with
// the C `accesskit_opt_*` / collection types so they can be returned by value, and
// expose helpers the idiomatic wrappers use to decode them.
// ----------------------------------------------------------------------------

[StructLayout(LayoutKind.Sequential)]
internal readonly struct NodeIds
{
    public readonly UIntPtr Length;
    public readonly IntPtr Values; // const accesskit_node_id* (ulong array)

    public ulong[] ToArray()
    {
        int n = (int)(uint)Length;
        var result = new ulong[n];
        for (int i = 0; i < n; i++)
            result[i] = unchecked((ulong)Marshal.ReadInt64(Values, i * sizeof(ulong)));
        return result;
    }
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct Lengths
{
    public readonly UIntPtr Length;
    public readonly IntPtr Values; // const uint8_t*

    public byte[] ToArray()
    {
        int n = (int)(uint)Length;
        var result = new byte[n];
        if (n > 0) Marshal.Copy(Values, result, 0, n);
        return result;
    }
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct OptCoords
{
    public readonly byte HasValue;
    public readonly UIntPtr Length;
    public readonly IntPtr Values; // const float*

    public float[]? ToArray()
    {
        if (HasValue == 0) return null;
        int n = (int)(uint)Length;
        var result = new float[n];
        if (n > 0) Marshal.Copy(Values, result, 0, n);
        return result;
    }
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct OptNodeId { public readonly byte HasValue; public readonly ulong Value; }

[StructLayout(LayoutKind.Sequential)]
internal readonly struct OptTreeId { public readonly byte HasValue; public readonly TreeId Value; }

[StructLayout(LayoutKind.Sequential)]
internal readonly struct OptDouble { public readonly byte HasValue; public readonly double Value; }

[StructLayout(LayoutKind.Sequential)]
internal readonly struct OptFloat { public readonly byte HasValue; public readonly float Value; }

[StructLayout(LayoutKind.Sequential)]
internal readonly struct OptIndex { public readonly byte HasValue; public readonly UIntPtr Value; }

[StructLayout(LayoutKind.Sequential)]
internal readonly struct OptColor { public readonly byte HasValue; public readonly Color Value; }

[StructLayout(LayoutKind.Sequential)]
internal readonly struct OptBool { public readonly byte HasValue; public readonly byte Value; }

[StructLayout(LayoutKind.Sequential)]
internal readonly struct OptInvalid { public readonly byte HasValue; public readonly Invalid Value; }

[StructLayout(LayoutKind.Sequential)]
internal readonly struct OptToggled { public readonly byte HasValue; public readonly Toggled Value; }

[StructLayout(LayoutKind.Sequential)]
internal readonly struct OptLive { public readonly byte HasValue; public readonly Live Value; }

[StructLayout(LayoutKind.Sequential)]
internal readonly struct OptTextDirection { public readonly byte HasValue; public readonly TextDirection Value; }

[StructLayout(LayoutKind.Sequential)]
internal readonly struct OptOrientation { public readonly byte HasValue; public readonly Orientation Value; }

[StructLayout(LayoutKind.Sequential)]
internal readonly struct OptSortDirection { public readonly byte HasValue; public readonly SortDirection Value; }

[StructLayout(LayoutKind.Sequential)]
internal readonly struct OptAriaCurrent { public readonly byte HasValue; public readonly AriaCurrent Value; }

[StructLayout(LayoutKind.Sequential)]
internal readonly struct OptAutoComplete { public readonly byte HasValue; public readonly AutoComplete Value; }

[StructLayout(LayoutKind.Sequential)]
internal readonly struct OptHasPopup { public readonly byte HasValue; public readonly HasPopup Value; }

[StructLayout(LayoutKind.Sequential)]
internal readonly struct OptListStyle { public readonly byte HasValue; public readonly ListStyle Value; }

[StructLayout(LayoutKind.Sequential)]
internal readonly struct OptTextAlign { public readonly byte HasValue; public readonly TextAlign Value; }

[StructLayout(LayoutKind.Sequential)]
internal readonly struct OptVerticalOffset { public readonly byte HasValue; public readonly VerticalOffset Value; }

[StructLayout(LayoutKind.Sequential)]
internal readonly struct OptTextDecoration { public readonly byte HasValue; public readonly TextDecoration Value; }

[StructLayout(LayoutKind.Sequential)]
internal readonly struct OptRect { public readonly byte HasValue; public readonly Rect Value; }

[StructLayout(LayoutKind.Sequential)]
internal readonly struct OptTextSelection { public readonly byte HasValue; public readonly TextSelection Value; }

[StructLayout(LayoutKind.Sequential)]
internal readonly struct OptLResult { public readonly byte HasValue; public readonly IntPtr Value; }
