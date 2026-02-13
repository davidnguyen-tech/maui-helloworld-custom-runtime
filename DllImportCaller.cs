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
    private const string LibObjC = "/usr/lib/libobjc.dylib";

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

    // --- objc_msgSend experiments ---
    // These mimic how dotnet/macios bindings declare objc_msgSend.
    // All signatures are fully blittable (nint only).

    // Test A: objc_msgSend via libobjc.dylib (should force IL stub)
    [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
    public static extern nint objc_msgSend_nint(nint receiver, nint selector);

    // Test B: same blittable signature but via __Internal (should NOT force IL stub)
    [DllImport(Lib, EntryPoint = "native_add")]
    public static extern nint internal_nint_nint(nint a, nint b);

    // Test C: objc_msgSend with 3 args
    [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
    public static extern nint objc_msgSend_nint_nint(nint receiver, nint selector, nint arg1);

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

        // --- objc_msgSend experiments ---
        // Get a real ObjC class and selector to call safely
        var nsObjectClass = ObjCRuntime.Runtime.GetNSObject(ObjCRuntime.Class.GetHandle("NSObject"));
        nint classHandle = ObjCRuntime.Class.GetHandle("NSObject");
        nint allocSel = ObjCRuntime.Selector.GetHandle("alloc");
        nint releaseSel = ObjCRuntime.Selector.GetHandle("release");
        nint retainCountSel = ObjCRuntime.Selector.GetHandle("retainCount");

        // Test A: objc_msgSend via libobjc → expect IL stub
        nint obj = objc_msgSend_nint(classHandle, allocSel);
        Console.WriteLine($"[DllImport] objc_msgSend(NSObject, alloc) = 0x{obj:X}");

        // Test B: same signature via __Internal → expect no IL stub
        nint addResult = internal_nint_nint(3, 4);
        Console.WriteLine($"[DllImport] internal_nint_nint(3, 4) = {addResult}");

        // Test C: objc_msgSend with 3 args → expect IL stub
        nint retainCount = objc_msgSend_nint_nint(obj, retainCountSel, 0);
        Console.WriteLine($"[DllImport] objc_msgSend(obj, retainCount, 0) = {retainCount}");

        // Release the object
        objc_msgSend_nint(obj, releaseSel);
        Console.WriteLine("[DllImport] objc_msgSend(obj, release) done");
    }
}
