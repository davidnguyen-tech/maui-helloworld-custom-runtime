using System.Runtime.InteropServices;

namespace NativeInterop;

/// <summary>
/// Calls native functions via LibraryImport (source-generated marshalling).
/// Marshalling code is emitted at compile time, so R2R can precompile it.
/// No interpreter fallback needed on iOS.
/// </summary>
public static partial class LibraryImportCaller
{
    private const string Lib = "__Internal";

    [LibraryImport(Lib)]
    public static partial int native_add(int a, int b);

    [LibraryImport(Lib)]
    public static unsafe partial void native_fill_buffer(int* buf, int len);

    [LibraryImport(Lib, StringMarshalling = StringMarshalling.Utf8)]
    public static partial int native_string_length(string s);

    [LibraryImport(Lib)]
    public static partial double native_point_distance(Point2D a, Point2D b);

    [LibraryImport(Lib)]
    public static partial double native_rect_area(Rect2D r);

    [LibraryImport(Lib)]
    public static partial Rect2D native_rect_offset(Rect2D r, Point2D offset);

    public static void RunAll()
    {
        Console.WriteLine("[LibraryImport] native_add(3, 4) = " + native_add(3, 4));

        unsafe
        {
            int* buf = stackalloc int[5];
            native_fill_buffer(buf, 5);
            Console.WriteLine($"[LibraryImport] native_fill_buffer: [{buf[0]}, {buf[1]}, {buf[2]}, {buf[3]}, {buf[4]}]");
        }

        Console.WriteLine("[LibraryImport] native_string_length(\"hello\") = " + native_string_length("hello"));

        var p1 = new Point2D { X = 1, Y = 2 };
        var p2 = new Point2D { X = 4, Y = 6 };
        Console.WriteLine($"[LibraryImport] native_point_distance = {native_point_distance(p1, p2)}");

        var rect = new Rect2D { X = 10, Y = 20, Width = 30, Height = 40 };
        Console.WriteLine($"[LibraryImport] native_rect_area = {native_rect_area(rect)}");

        var offset = new Point2D { X = 5, Y = 5 };
        var moved = native_rect_offset(rect, offset);
        Console.WriteLine($"[LibraryImport] native_rect_offset = ({moved.X}, {moved.Y}, {moved.Width}, {moved.Height})");
    }
}
