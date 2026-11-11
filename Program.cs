using System.Globalization;
using System.Numerics;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
if (args.Length != 1 || args[0] == "--help")
{
    Console.WriteLine("Usage: dotnet run -- <input-file|->");
    return args.Length == 1 ? 0 : 2;
}

try
{
    string[] lines = args[0] == "-" ? Console.In.ReadToEnd().ReplaceLineEndings("\n").TrimEnd('\n').Split('\n') : File.ReadAllLines(args[0]);
    Check(lines.Length >= 1 && lines.Any(line => !string.IsNullOrWhiteSpace(line)), "Input is empty or missing records.");
    foreach (var f in lines.Select(s => Fields(s, 3)))
    {
        var a = BigInteger.Parse(f[0]);
        var b = BigInteger.Parse(f[1]);
        var m = BigInteger.Parse(f[2]);
        Check(m > 0, "Modulus must be positive.");
        BigInteger r = m, next = (a % m + m) % m, t = 0, nextT = 1;
        while (next != 0)
        {
            var q = r / next;
            (r, next) = (next, r - q * next);
            (t, nextT) = (nextT, t - q * nextT);
        }

        if (b % r != 0)
        {
            Console.WriteLine("No solution.");
            continue;
        }

        var period = m / r;
        var x = ((t * (b / r)) % period + period) % period;
        Console.WriteLine($"x = {x} (mod {period}); {r} residue(s) modulo {m}");
    }

    return 0;
}
catch (Exception e)when (e is IOException or FormatException or OverflowException or ArgumentException or UnauthorizedAccessException or DivideByZeroException)
{
    Console.Error.WriteLine(e.Message);
    return 1;
}

static void Check(bool valid, string message)
{
    if (!valid)
        throw new FormatException(message);
}

static string[] Fields(string line, int count)
{
    var fields = line.Split('\t', StringSplitOptions.TrimEntries);
    Check(fields.Length == count && fields.All(s => s.Length > 0), $"Expected {count} tab-separated values in '{line}'.");
    return fields;
}
