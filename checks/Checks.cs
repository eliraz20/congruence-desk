using System.Diagnostics;
using System.Text.RegularExpressions;

// Run from the repository root after dotnet build.
static (int Code, string Output) Run(string input, params string[] options)
{
    var start = new ProcessStartInfo("dotnet")
    {
        RedirectStandardInput = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true
    };
    start.ArgumentList.Add(Path.Combine("bin", "Debug", "net10.0", "congruence-desk.dll"));
    start.ArgumentList.Add("-");
    foreach (var option in options)
        start.ArgumentList.Add(option);
    using var child = Process.Start(start)!;
    var stdout = child.StandardOutput.ReadToEndAsync();
    var stderr = child.StandardError.ReadToEndAsync();
    child.StandardInput.Write(input);
    child.StandardInput.Close();
    child.WaitForExit();
    return (child.ExitCode, (stdout.Result + stderr.Result).ReplaceLineEndings("\n").Trim());
}

var random = new Random(241);
var cases = new List<(string input, int modulus, int width, Func<int[], bool> valid, string kind)>();
for (int i = 0; i < 1200; i++)
{
    int m = random.Next(1, 110), a = random.Next(-12, 13), b = random.Next(-12, 13), c = random.Next(-12, 13);
    cases.Add(($"quad {a} {b} {c} {m}", m, 1, v => (a * v[0] * v[0] + b * v[0] + c) % m == 0, "quad"));
}

for (int i = 0; i < 450; i++)
{
    int m = random.Next(1, 10), n = random.Next(1, 5), height = random.Next(1, 6);
    var rows = Enumerable.Range(0, height).Select(_ => Enumerable.Range(0, n + 1).Select(_ => random.Next(-9, 10)).ToArray()).ToArray();
    string input = $"system {m} {n}\n" + string.Join("\n", rows.Select(row => string.Join(" ", row))) + "\nend";
    cases.Add((input, m, n, v => rows.All(row => (row.Take(n).Zip(v, (x, y) => x * y).Sum() - row[n]) % m == 0), "system"));
}

for (int i = 0; i < 120; i++)
{
    int m = random.Next(1, 18);
    var k = Enumerable.Range(0, 6).Select(_ => random.Next(-10, 11)).ToArray();
    cases.Add(($"conic {string.Join(" ", k)} {m}", m, 2, v =>
    {
        var(x, y) = (v[0], v[1]);
        return (k[0] * x * x + k[1] * x * y + k[2] * y * y + k[3] * x + k[4] * y + k[5]) % m == 0;
    }, "conic"));
}

var(code, output) = Run(string.Join("\n", cases.Select(c => c.input)));
if (code != 0)
    throw new Exception(output);
var answers = output.Split("\n\n");
if (answers.Length != cases.Count)
    throw new Exception("Missing result.");
for (int i = 0; i < cases.Count; i++)
{
    var(input, m, n, valid, kind) = cases[i];
    var expected = new HashSet<string>();
    int space = (int)Math.Pow(m, n);
    for (int j = 0; j < space; j++)
    {
        var v = new int[n];
        int value = j;
        for (int d = 0; d < n; d++)
        {
            v[d] = value % m;
            value /= m;
        }

        if (valid(v))
            expected.Add(string.Join(",", v));
    }

    string answer = answers[i];
    var actual = new List<string>();
    if (answer != "No solution.")
    {
        if (kind == "system")
        {
            static int[] Vector(string line) => Regex.Match(line, @"\(([\d, ]+)\)").Groups[1].Value.Split(',').Select(int.Parse).ToArray();
            var lines = answer.Split('\n');
            var family = new List<int[]>
            {
                Vector(lines[0])
            };
            foreach (string line in lines[1..^1])
            {
                int[] step = Vector(line);
                int count = int.Parse(line.Split(' ')[^1]);
                family = family.SelectMany(v => Enumerable.Range(0, count).Select(t => v.Zip(step, (x, s) => (x + t * s) % m).ToArray())).ToList();
            }

            actual.AddRange(family.Select(v => string.Join(",", v)));
            if (int.Parse(lines[^1].Split(' ')[0]) != expected.Count)
                throw new Exception(input);
        }
        else
        {
            foreach (string line in answer.Split('\n'))
            {
                if (line.Contains("ordered pair"))
                {
                    if (int.Parse(line.Split(' ')[0]) != expected.Count)
                        throw new Exception(input);
                    continue;
                }

                var prefix = kind == "conic" ? Regex.Match(line, @"^x = (\d+): ").Groups[1].Value + "," : "";
                int before = actual.Count;
                foreach (Match root in Regex.Matches(line, @"(\d+) \(mod (\d+)\)"))
                    for (int r = int.Parse(root.Groups[1].Value); r < m; r += int.Parse(root.Groups[2].Value))
                        actual.Add(prefix + r);
                if (int.Parse(Regex.Match(line, @"; (\d+) residue").Groups[1].Value) != actual.Count - before)
                    throw new Exception(input);
            }
        }
    }

    if (actual.Count != expected.Count || !expected.SetEquals(actual))
        throw new Exception(input + "\n" + answer);
}

if (Run("quad 1 0 -1 1000001").Code == 0 || Run("system 3 2\n1 2 0").Code == 0)
    throw new Exception("Invalid input accepted.");
Console.WriteLine($"{cases.Count} solution sets match independent enumeration, including multiplicities and parameter ranges.");
