using System.Numerics;

sealed class LinearSystem
{
    private readonly BigInteger[,] matrix;
    private readonly BigInteger[,] basis;
    private readonly BigInteger[] rhs;
    private readonly int width;
    private readonly BigInteger modulus;

    private LinearSystem(List<BigInteger[]> rows, int width, BigInteger modulus)
    {
        this.width = width;
        this.modulus = modulus;
        matrix = new BigInteger[rows.Count, width];
        rhs = new BigInteger[rows.Count];
        basis = new BigInteger[width, width];
        for (int i = 0; i < rows.Count; i++)
        {
            for (int j = 0; j < width; j++) matrix[i, j] = Modular.Mod(rows[i][j], modulus);
            rhs[i] = Modular.Mod(rows[i][width], modulus);
        }
        for (int j = 0; j < width; j++) basis[j, j] = 1;
    }

    public static void Print(List<BigInteger[]> rows, int width, BigInteger modulus)
    {
        var system = new LinearSystem(rows, width, modulus);
        int rank = system.Diagonalize();
        system.PrintFamily(rank);
    }

    private int Diagonalize()
    {
        int pivot = 0;
        for (; pivot < Math.Min(width, rhs.Length); pivot++)
        {
            var (row, column) = FindPivot(pivot);
            if (row < 0) break;
            SwapRows(pivot, row);
            SwapColumns(pivot, column);

            // A nonzero Euclidean remainder trades places with the pivot and strictly reduces it.
            // Column swaps can bring entries back below the pivot, so revisit both axes.
            bool exchanged;
            do
            {
                exchanged = false;
                for (int i = pivot + 1; i < rhs.Length; i++)
                {
                    if (matrix[i, pivot].IsZero) continue;
                    var q = matrix[i, pivot] / matrix[pivot, pivot];
                    for (int j = pivot; j < width; j++)
                        matrix[i, j] = Modular.Mod(matrix[i, j] - q * matrix[pivot, j], modulus);
                    rhs[i] = Modular.Mod(rhs[i] - q * rhs[pivot], modulus);
                    if (matrix[i, pivot].IsZero) continue;
                    SwapRows(i, pivot);
                    exchanged = true;
                    break;
                }
                if (exchanged) continue;

                for (int j = pivot + 1; j < width; j++)
                {
                    if (matrix[pivot, j].IsZero) continue;
                    var q = matrix[pivot, j] / matrix[pivot, pivot];
                    for (int i = pivot; i < rhs.Length; i++)
                        matrix[i, j] = Modular.Mod(matrix[i, j] - q * matrix[i, pivot], modulus);
                    for (int i = 0; i < width; i++)
                        basis[i, j] = Modular.Mod(basis[i, j] - q * basis[i, pivot], modulus);
                    if (matrix[pivot, j].IsZero) continue;
                    SwapColumns(j, pivot);
                    exchanged = true;
                    break;
                }
            } while (exchanged);
        }
        return pivot;
    }

    private (int, int) FindPivot(int start)
    {
        for (int i = start; i < rhs.Length; i++)
        for (int j = start; j < width; j++)
            if (!matrix[i, j].IsZero) return (i, j);
        return (-1, -1);
    }

    private void SwapRows(int a, int b)
    {
        if (a == b) return;
        for (int j = 0; j < width; j++)
            (matrix[a, j], matrix[b, j]) = (matrix[b, j], matrix[a, j]);
        (rhs[a], rhs[b]) = (rhs[b], rhs[a]);
    }

    private void SwapColumns(int a, int b)
    {
        if (a == b) return;
        for (int i = 0; i < rhs.Length; i++)
            (matrix[i, a], matrix[i, b]) = (matrix[i, b], matrix[i, a]);
        for (int i = 0; i < width; i++)
            (basis[i, a], basis[i, b]) = (basis[i, b], basis[i, a]);
    }

    private void PrintFamily(int rank)
    {
        var origin = new BigInteger[width];
        var directions = new List<(BigInteger[] vector, BigInteger order)>();
        BigInteger count = 1;
        if (rhs.Skip(rank).Any(v => !v.IsZero))
        {
            Console.WriteLine("No solution.");
            return;
        }
        for (int j = 0; j < width; j++)
        {
            Residue? coordinate = j < rank ? Modular.Linear(matrix[j, j], rhs[j], modulus) : new Residue(0, 1);
            if (coordinate is null)
            {
                Console.WriteLine("No solution.");
                return;
            }
            var order = modulus / coordinate.Value.Period;
            var direction = new BigInteger[width];
            for (int i = 0; i < width; i++)
            {
                origin[i] = Modular.Mod(origin[i] + basis[i, j] * coordinate.Value.Value, modulus);
                direction[i] = Modular.Mod(basis[i, j] * coordinate.Value.Period, modulus);
            }
            if (order > 1) directions.Add((direction, order));
            count *= order;
        }
        Console.WriteLine($"Base = ({string.Join(", ", origin)}) modulo {modulus}");
        for (int i = 0; i < directions.Count; i++)
        {
            var (vector, order) = directions[i];
            Console.WriteLine($"  + t{i + 1} * ({string.Join(", ", vector)}), 0 <= t{i + 1} < {order}");
        }
        Console.WriteLine($"{count} distinct solution(s)");
    }
}
