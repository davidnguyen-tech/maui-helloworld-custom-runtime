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
    // Testing which combination of library + entrypoint forces IL stubs.
    // The runtime checks: library == "/usr/lib/libobjc.dylib" AND
    // entrypoint starts with "objc_msgSend".

    // Test A: libobjc + objc_msgSend → EXPECT stub (both conditions match)
    [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
    public static extern nint test_A_libobjc_msgSend(nint a, nint b);

    // Test B: __Internal + native_add → EXPECT no stub (neither condition)
    [DllImport(Lib, EntryPoint = "native_add")]
    public static extern nint test_B_internal_native(nint a, nint b);

    // Test C: libobjc + objc_msgSendSuper → EXPECT stub (both conditions)
    [DllImport(LibObjC, EntryPoint = "objc_msgSendSuper")]
    public static extern nint test_C_libobjc_msgSendSuper(nint a, nint b);

    // Test D: libobjc + sel_registerName → EXPECT no stub (library matches but entrypoint doesn't)
    [DllImport(LibObjC, EntryPoint = "sel_registerName")]
    public static extern nint test_D_libobjc_selRegister(nint name);

    // Test E: __Internal + entrypoint named "objc_msgSend" → EXPECT no stub (entrypoint matches but library doesn't)
    // We point it at native_add since __Internal!objc_msgSend doesn't exist
    [DllImport(Lib, EntryPoint = "native_add")]
    public static extern nint test_E_internal_fakeObjc(nint a, nint b);

    // Test F: libobjc + class_getName → EXPECT no stub (library matches but entrypoint doesn't)
    [DllImport(LibObjC, EntryPoint = "class_getName")]
    public static extern nint test_F_libobjc_className(nint cls);

    // Test G: SetLastError=true on __Internal → EXPECT stub (SetLastError forces it)
    [DllImport(Lib, EntryPoint = "native_add", SetLastError = true)]
    public static extern nint test_G_setLastError(nint a, nint b);

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
        nint classHandle = ObjCRuntime.Class.GetHandle("NSObject");
        nint allocSel = ObjCRuntime.Selector.GetHandle("alloc");
        nint releaseSel = ObjCRuntime.Selector.GetHandle("release");

        // Test A: libobjc + objc_msgSend → expect stub
        nint obj = test_A_libobjc_msgSend(classHandle, allocSel);
        Console.WriteLine($"[Test A] libobjc+msgSend = 0x{obj:X}");

        // Test B: __Internal + native_add → expect no stub
        Console.WriteLine($"[Test B] internal+native = {test_B_internal_native(3, 4)}");

        // Test C: libobjc + objc_msgSendSuper → expect stub
        // Can't safely call msgSendSuper without a proper super struct, just declare
        // it to see if the stub is generated. Skip actual call.
        Console.WriteLine("[Test C] libobjc+msgSendSuper — declared only");

        // Test D: libobjc + sel_registerName → expect no stub
        nint sel = test_D_libobjc_selRegister(allocSel);
        Console.WriteLine($"[Test D] libobjc+selRegister = 0x{sel:X}");

        // Test E: __Internal + "native_add" (pretend objc_msgSend) → expect no stub
        Console.WriteLine($"[Test E] internal+fakeObjc = {test_E_internal_fakeObjc(10, 20)}");

        // Test F: libobjc + class_getName → expect no stub
        nint name = test_F_libobjc_className(classHandle);
        Console.WriteLine($"[Test F] libobjc+className = 0x{name:X}");

        // Test G: SetLastError=true → expect stub
        Console.WriteLine($"[Test G] setLastError = {test_G_setLastError(5, 6)}");

        // Cleanup
        test_A_libobjc_msgSend(obj, releaseSel);
        Console.WriteLine("[Tests] done");
    }
}
