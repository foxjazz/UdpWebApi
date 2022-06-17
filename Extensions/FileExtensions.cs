using System.Net;
using System.IO;
namespace Extensions;

public static class FileAppender
{
    public static void AppendToLog(this string text)
    {
        var path = Directory.GetCurrentDirectory();
        var fn = Path.Combine(path, "Udp.log");
        System.IO.File.AppendAllText(fn, text);
    }
}