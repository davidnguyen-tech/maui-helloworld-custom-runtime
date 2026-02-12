using System.Runtime.InteropServices;

namespace NativeInterop;

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
    }
}
