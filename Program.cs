using System.Globalization;
using System.Numerics;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
if (args.Length != 1 || args[0] == "--help")
{
    Console.WriteLine("Usage: dotnet run -- <input-file|->  (see example.tsv for linear, quad, conic and system)");
    return args.Length == 1 ? 0 : 2;
}

int lineNumber = 0;
try
{
    using var reader = args[0] == "-" ? Console.In : File.OpenText(args[0]);
    string[]? Next()
    {
        while (reader.ReadLine() is { } line)
        {
            lineNumber++;
            var trimmed = line.Trim();
            if (trimmed.Length > 0 && !trimmed.StartsWith('#'))
                return trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        }
        return null;
    }

    int queries = 0;
    while (Next() is { } tokens)
    {
        queries++;
        if (queries > 1) Console.WriteLine();
        switch (tokens[0])
        {
            case "quad":
            {
                var v = Numbers(tokens[1..], 4);
                var modulus = LimitedModulus(v[3], 1_000_000);
                WriteRoots("x", Quadratics.Solve(v[0], v[1], v[2], modulus), modulus);
                break;
            }
            case "conic":
            {
                var v = Numbers(tokens[1..], 7);
                int modulus = LimitedModulus(v[6], 512);
                BigInteger count = 0;
                for (int x = 0; x < modulus; x++)
                {
                    var roots = Quadratics.Solve(v[2], v[1] * x + v[4], v[0] * x * x + v[3] * x + v[5], modulus);
                    if (roots.Count == 0) continue;
                    Console.Write($"x = {x}: ");
                    count += WriteRoots("y", roots, modulus);
                }
                Console.WriteLine($"{count} ordered pair(s) modulo {modulus}");
                break;
            }
            case "system":
            {
                var config = Numbers(tokens[1..], 2);
                if (config[0] <= 0 || config[1] < 1 || config[1] > int.MaxValue - 1)
                    throw new FormatException("Use a positive modulus and variable count.");
                int variables = (int)config[1];
                var rows = new List<BigInteger[]>();
                while (true)
                {
                    var row = Next() ?? throw new FormatException("Missing end after system rows.");
                    if (row is ["end"]) break;
                    rows.Add(Numbers(row, variables + 1));
                }
                if (rows.Count == 0) throw new FormatException("A system needs at least one equation.");
                LinearSystem.Print(rows, variables, config[0]);
                break;
            }
            default:
            {
                var v = Numbers(tokens[0] == "linear" ? tokens[1..] : tokens, 3);
                if (v[2] <= 0) throw new FormatException("Modulus must be positive.");
                var root = Modular.Linear(v[0], v[1], v[2]);
                if (root is null) Console.WriteLine("No solution.");
                else WriteRoots("x", [root.Value], v[2]);
                break;
            }
        }
    }
    if (queries == 0) throw new FormatException("Input is empty.");
    return 0;
}
catch (Exception e) when (e is IOException or FormatException or OverflowException or ArgumentException or UnauthorizedAccessException)
{
    Console.Error.WriteLine($"Line {lineNumber}: {e.Message}");
    return 1;
}

static int LimitedModulus(BigInteger value, int max)
{
    if (value < 1 || value > max) throw new FormatException($"This query supports moduli from 1 through {max}.");
    return (int)value;
}

static BigInteger[] Numbers(string[] fields, int count)
{
    if (fields.Length != count) throw new FormatException($"Expected {count} integers.");
    return fields.Select(s => BigInteger.Parse(s, CultureInfo.InvariantCulture)).ToArray();
}

static BigInteger WriteRoots(string name, List<Residue> roots, BigInteger modulus)
{
    if (roots.Count == 0)
    {
        Console.WriteLine("No solution.");
        return 0;
    }
    BigInteger count = 0;
    foreach (var root in roots)
    {
        count += modulus / root.Period;
    }
    Console.WriteLine($"{name} = {string.Join(" or ", roots.OrderBy(r => r.Value).Select(r => $"{r.Value} (mod {r.Period})"))}; {count} residue(s) modulo {modulus}");
    return count;
}
