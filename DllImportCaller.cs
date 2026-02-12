using System.Runtime.InteropServices;

namespace NativeInterop;

[StructLayout(LayoutKind.Sequential)]
public struct Point2D
{
    public double X;
    public double Y;
}

[StructLayout(LayoutKind.Sequential)]
public struct Rect2D
{
    public double X;
    public double Y;
    public double Width;
    public double Height;
}

/// <summary>
/// Calls native functions via DllImport (runtime-generated IL stubs).
/// On iOS without JIT, these stubs fall back to the interpreter.
/// </summary>
public static class DllImportCaller
{
    private const string Lib = "__Internal";

    [DllImport(Lib)]
    public static extern int native_add(int a, int b);

    [DllImport(Lib)]
    public static extern unsafe void native_fill_buffer(int* buf, int len);

    [DllImport(Lib, CharSet = CharSet.Ansi)]
    public static extern int native_string_length(string s);

    [DllImport(Lib)]
    public static extern double native_point_distance(Point2D a, Point2D b);

    [DllImport(Lib)]
    public static extern double native_rect_area(Rect2D r);

    [DllImport(Lib)]
    public static extern Rect2D native_rect_offset(Rect2D r, Point2D offset);

    public static void RunAll()
    {
        Console.WriteLine("[DllImport] native_add(3, 4) = " + native_add(3, 4));

        unsafe
        {
            int* buf = stackalloc int[5];
            native_fill_buffer(buf, 5);
            Console.WriteLine($"[DllImport] native_fill_buffer: [{buf[0]}, {buf[1]}, {buf[2]}, {buf[3]}, {buf[4]}]");
        }

        Console.WriteLine("[DllImport] native_string_length(\"hello\") = " + native_string_length("hello"));

        var p1 = new Point2D { X = 1, Y = 2 };
        var p2 = new Point2D { X = 4, Y = 6 };
        Console.WriteLine($"[DllImport] native_point_distance = {native_point_distance(p1, p2)}");

        var rect = new Rect2D { X = 10, Y = 20, Width = 30, Height = 40 };
        Console.WriteLine($"[DllImport] native_rect_area = {native_rect_area(rect)}");

        var offset = new Point2D { X = 5, Y = 5 };
        var moved = native_rect_offset(rect, offset);
        Console.WriteLine($"[DllImport] native_rect_offset = ({moved.X}, {moved.Y}, {moved.Width}, {moved.Height})");
    }
}
