using System.Runtime.InteropServices;

namespace AccessKit;

/// <summary>A 2D point. Mirrors <c>accesskit_point</c>.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct Point
{
    public readonly double X;
    public readonly double Y;
    public Point(double x, double y) { X = x; Y = y; }

    public Vec2 ToVec2() { var self = this; return Interop.accesskit_point_to_vec2(self); }
    public Point Add(Vec2 v) { var self = this; return Interop.accesskit_point_add_vec2(self, v); }
    public Point Sub(Vec2 v) { var self = this; return Interop.accesskit_point_sub_vec2(self, v); }
    public Vec2 Sub(Point b) { var self = this; return Interop.accesskit_point_sub_point(self, b); }

    public override string ToString() => $"Point({X}, {Y})";
}

/// <summary>A 2D size. Mirrors <c>accesskit_size</c>.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct Size
{
    public readonly double Width;
    public readonly double Height;
    public Size(double width, double height) { Width = width; Height = height; }

    public Vec2 ToVec2() { var self = this; return Interop.accesskit_size_to_vec2(self); }
    public Size Scale(double scalar) { var self = this; return Interop.accesskit_size_scale(self, scalar); }
    public Size Add(Size b) { var self = this; return Interop.accesskit_size_add(self, b); }
    public Size Sub(Size b) { var self = this; return Interop.accesskit_size_sub(self, b); }

    public override string ToString() => $"Size({Width}, {Height})";
}

/// <summary>A 2D vector (also used as a translation). Mirrors <c>accesskit_vec2</c>.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct Vec2
{
    public readonly double X;
    public readonly double Y;
    public Vec2(double x, double y) { X = x; Y = y; }

    public Point ToPoint() { var self = this; return Interop.accesskit_vec2_to_point(self); }
    public Size ToSize() { var self = this; return Interop.accesskit_vec2_to_size(self); }
    public Vec2 Add(Vec2 b) { var self = this; return Interop.accesskit_vec2_add(self, b); }
    public Vec2 Sub(Vec2 b) { var self = this; return Interop.accesskit_vec2_sub(self, b); }
    public Vec2 Scale(double scalar) { var self = this; return Interop.accesskit_vec2_scale(self, scalar); }
    public Vec2 Negate() { var self = this; return Interop.accesskit_vec2_neg(self); }

    public override string ToString() => $"Vec2({X}, {Y})";
}

/// <summary>
/// A rectangle in the window's coordinate space (y-down). Mirrors <c>accesskit_rect</c>:
/// <c>(X0, Y0)</c> is the top-left, <c>(X1, Y1)</c> the bottom-right.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct Rect
{
    public readonly double X0;
    public readonly double Y0;
    public readonly double X1;
    public readonly double Y1;

    public Rect(double x0, double y0, double x1, double y1) { X0 = x0; Y0 = y0; X1 = x1; Y1 = y1; }

    public static Rect FromPoints(Point p0, Point p1) => Interop.accesskit_rect_from_points(p0, p1);
    public static Rect FromOriginSize(Point origin, Size size) => Interop.accesskit_rect_from_origin_size(origin, size);

    public double Width { get { var self = this; return Interop.accesskit_rect_width(self); } }
    public double Height { get { var self = this; return Interop.accesskit_rect_height(self); } }
    public double MinX { get { var self = this; return Interop.accesskit_rect_min_x(self); } }
    public double MaxX { get { var self = this; return Interop.accesskit_rect_max_x(self); } }
    public double MinY { get { var self = this; return Interop.accesskit_rect_min_y(self); } }
    public double MaxY { get { var self = this; return Interop.accesskit_rect_max_y(self); } }
    public double Area { get { var self = this; return Interop.accesskit_rect_area(self); } }
    public bool IsEmpty { get { var self = this; return Interop.accesskit_rect_is_empty(self); } }
    public Point Origin { get { var self = this; return Interop.accesskit_rect_origin(self); } }
    public Size Size { get { var self = this; return Interop.accesskit_rect_size(self); } }

    public Rect WithOrigin(Point origin) { var self = this; return Interop.accesskit_rect_with_origin(self, origin); }
    public Rect WithSize(Size size) { var self = this; return Interop.accesskit_rect_with_size(self, size); }
    public Rect Abs() { var self = this; return Interop.accesskit_rect_abs(self); }
    public bool Contains(Point p) { var self = this; return Interop.accesskit_rect_contains(self, p); }
    public Rect Union(Rect other) { var self = this; return Interop.accesskit_rect_union(self, other); }
    public Rect Union(Point pt) { var self = this; return Interop.accesskit_rect_union_pt(self, pt); }
    public Rect Intersect(Rect other) { var self = this; return Interop.accesskit_rect_intersect(self, other); }
    public Rect Translate(Vec2 translation) => Interop.accesskit_rect_translate(this, translation);

    public override string ToString() => $"Rect({X0}, {Y0}, {X1}, {Y1})";
}

/// <summary>
/// A 2D affine transform (six coefficients), derived from kurbo. Mirrors <c>accesskit_affine</c>.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct Affine
{
    public readonly double M0, M1, M2, M3, M4, M5;

    public Affine(double m0, double m1, double m2, double m3, double m4, double m5)
    {
        M0 = m0; M1 = m1; M2 = m2; M3 = m3; M4 = m4; M5 = m5;
    }

    public static Affine Identity() => Interop.accesskit_affine_identity();
    public static Affine FlipX() => Interop.accesskit_affine_flip_x();
    public static Affine FlipY() => Interop.accesskit_affine_flip_y();
    public static Affine Scale(double s) => Interop.accesskit_affine_scale(s);
    public static Affine ScaleNonUniform(double sx, double sy) => Interop.accesskit_affine_scale_non_uniform(sx, sy);
    public static Affine Translate(Vec2 p) => Interop.accesskit_affine_translate(p);
    public static Affine MapUnitSquare(Rect rect) => Interop.accesskit_affine_map_unit_square(rect);

    public double Determinant() => Interop.accesskit_affine_determinant(this);
    public Affine Inverse() => Interop.accesskit_affine_inverse(this);
    public Rect TransformRectBBox(Rect rect) => Interop.accesskit_affine_transform_rect_bbox(this, rect);
    public bool IsFinite { get { var self = this; return Interop.accesskit_affine_is_finite(self); } }
    public bool IsNaN { get { var self = this; return Interop.accesskit_affine_is_nan(self); } }
    public Affine Mul(Affine b) => Interop.accesskit_affine_mul(this, b);
    public Point TransformPoint(Point p) => Interop.accesskit_affine_transform_point(this, p);

    public override string ToString() => $"Affine({M0}, {M1}, {M2}, {M3}, {M4}, {M5})";
}
