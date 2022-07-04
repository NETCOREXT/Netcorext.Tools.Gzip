using System.IO.Compression;
using System.Text;

if (args.Length == 0)
{
    Console.WriteLine(DisplayHelp());

    return;
}

if (args.Any(t => t is "-?" or "-h" or "--help"))
{
    Console.WriteLine(DisplayHelp());

    return;
}

var filePath = Path.GetFullPath(args[0]);

if (string.IsNullOrWhiteSpace(filePath))
{
    Console.WriteLine(DisplayHelp());

    return;
}

if (!File.Exists(filePath))
{
    Console.WriteLine("File not found.");
    Console.WriteLine(DisplayHelp());

    return;
}

var fileName = Path.GetFileName(filePath);
var isDecompress = false;
var isBase64 = false;
var output = Path.GetDirectoryName(filePath);

for (var i = 1; i < args.Length; i++)
{
    switch (args[i])
    {
        case "-c":
        case "--compress":
            isDecompress = false;

            break;
        case "-d":
        case "--decompress":
            isDecompress = true;

            break;
        case "--base64":
            isBase64 = true;

            break;
        case "-o":
        case "-output":
            i++;
            
            output = Path.GetFullPath(args[i]);

            Directory.CreateDirectory(output);

            break;
    }
}

try
{
    var buffers = File.ReadAllBytes(filePath);
    
    if (isDecompress && isBase64)
    {
        buffers = Convert.FromBase64String(Encoding.UTF8.GetString(buffers));
    }

    using var ms = new MemoryStream();
    
    if (isDecompress)
    {
        using var msTemp = new MemoryStream(buffers);
        using var gzip = new GZipStream(msTemp, CompressionMode.Decompress);
        gzip.CopyTo(ms);    
    }
    else
    {
        using var msTemp = new MemoryStream(buffers);
        using var gzip = new GZipStream(ms, CompressionMode.Compress);
        msTemp.CopyTo(gzip);
    }
    
    buffers = ms.GetBuffer();
    
    if (!isDecompress && isBase64)
    {
        var base64 = Convert.ToBase64String(buffers, Base64FormattingOptions.InsertLineBreaks);
        buffers = Encoding.UTF8.GetBytes(base64);
    }
    
    using var fs = File.Open(output + "/" + fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
    fs.SetLength(0);
    fs.Write(buffers, 0, buffers.Length);
}
catch (Exception e)
{
    Console.Error.WriteLine(e.ToString());
}

string DisplayHelp()
{
    var sbHelp = new StringBuilder();
    sbHelp.AppendLine();
    sbHelp.AppendLine("Usage: dotnet gzip PATH-TO-FILE");
    sbHelp.AppendLine("Usage: dotnet gzip PATH-TO-FILE [OPTIONS]");
    sbHelp.AppendLine();
    sbHelp.AppendLine("path-to-file:");
    sbHelp.AppendLine("  The path to an file to execute.");
    sbHelp.AppendLine();
    sbHelp.AppendLine("Options:");
    sbHelp.AppendLine("  -c,     --compress       Compress.");
    sbHelp.AppendLine("  -d,     --decompress     Decompress.");
    sbHelp.AppendLine("  -o,     --output         Output path.");
    sbHelp.AppendLine("          --base64         Input/Output format of base64 string");
    sbHelp.AppendLine("  -?, -h, --help           Display help.");

    return sbHelp.ToString();
}