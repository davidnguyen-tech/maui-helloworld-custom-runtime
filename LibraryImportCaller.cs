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
    }
}
