using System;
using System.Runtime.InteropServices;
using System.Text;

namespace AccessKit;

internal static partial class Interop
{
    /// <summary>UTF-8 encode a string (no trailing NUL); paired with a length argument.</summary>
    public static byte[] Utf8(string s) => Encoding.UTF8.GetBytes(s);

    /// <summary>UTF-8 encode a string with a trailing NUL, for the null-terminated <c>const char*</c> setters.</summary>
    public static byte[] Utf8Z(string s)
    {
        var bytes = Encoding.UTF8.GetBytes(s);
        var z = new byte[bytes.Length + 1];
        Array.Copy(bytes, z, bytes.Length);
        return z;
    }

    /// <summary>Decode a NUL-terminated UTF-8 string at <paramref name="ptr"/> without freeing it.</summary>
    public static string? ReadStringUtf8(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero) return null;
        int len = 0;
        while (Marshal.ReadByte(ptr, len) != 0) len++;
        if (len == 0) return string.Empty;
        var bytes = new byte[len];
        Marshal.Copy(ptr, bytes, 0, len);
        return Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    /// Decode a NUL-terminated UTF-8 string returned by AccessKit and free it with
    /// <c>accesskit_string_free</c>. Returns null for a null pointer.
    /// </summary>
    public static string? TakeString(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero) return null;
        try { return ReadStringUtf8(ptr); }
        finally { accesskit_string_free(ptr); }
    }
}
