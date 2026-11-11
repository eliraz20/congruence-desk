using System.Numerics;

readonly record struct Residue(BigInteger Value, BigInteger Period);

static class Modular
{
    public static BigInteger Mod(BigInteger value, BigInteger modulus)
    {
        var result = value % modulus;
        return result.Sign < 0 ? result + modulus : result;
    }

    public static Residue? Linear(BigInteger coefficient, BigInteger rhs, BigInteger modulus)
    {
        BigInteger r = modulus, next = Mod(coefficient, modulus), t = 0, nextT = 1;
        while (next != 0)
        {
            var q = r / next;
            (r, next) = (next, r - q * next);
            (t, nextT) = (nextT, t - q * nextT);
        }
        if (rhs % r != 0) return null;
        var period = modulus / r;
        return new Residue(Mod(t * (rhs / r), period), period);
    }
}

static class Quadratics
{
    public static List<Residue> Solve(BigInteger a, BigInteger b, BigInteger c, int modulus)
    {
        var roots = new List<Residue> { new(0, 1) };
        int remaining = modulus;
        for (int p = 2; p <= remaining / p; p++)
        {
            if (remaining % p != 0) continue;
            int power = 1;
            do { remaining /= p; power *= p; } while (remaining % p == 0);
            roots = Combine(roots, PrimePower(a, b, c, p, power));
        }
        if (remaining > 1) roots = Combine(roots, PrimePower(a, b, c, remaining, remaining));
        return roots;
    }

    private static List<Residue> Combine(List<Residue> left, List<Residue> right)
    {
        var merged = new List<Residue>();
        foreach (var u in left)
        foreach (var v in right)
        {
            // Distinct prime-power factors have coprime periods.
            var shift = Modular.Linear(u.Period, v.Value - u.Value, v.Period)!.Value;
            merged.Add(new(u.Value + u.Period * shift.Value, u.Period * v.Period));
        }
        return merged;
    }

    private static List<Residue> PrimePower(BigInteger a, BigInteger b, BigInteger c, int p, int power)
    {
        if (power == 1) return [new(0, 1)];
        a = Modular.Mod(a, power);
        b = Modular.Mod(b, power);
        c = Modular.Mod(c, power);
        if (a.IsZero)
        {
            var linear = Modular.Linear(b, -c, power);
            return linear.HasValue ? [linear.Value] : [];
        }
        if (a % p == 0 && b % p == 0)
            return c % p == 0 ? PrimePower(a / p, b / p, c / p, p, power / p) : [];

        var firstDigits = new List<int>();
        if (a % p == 0)
        {
            firstDigits.Add((int)Modular.Linear(b, -c, p)!.Value.Value);
        }
        else
        {
            // Enumerate only the base field. Use finite differences to avoid multiplying at each digit.
            long value = (long)(c % p), delta = (long)((a + b) % p), step = (long)(2 * a % p);
            for (int r = 0; r < p; r++)
            {
                if (value == 0) firstDigits.Add(r);
                value = (value + delta) % p;
                delta = (delta + step) % p;
            }
        }

        var lifted = new List<Residue>();
        foreach (var r in firstDigits)
        {
            // Substitute x = r + p*t and divide the whole polynomial by p.
            var tail = PrimePower(a * p, 2 * a * r + b, (a * r * r + b * r + c) / p, p, power / p);
            foreach (var root in tail) lifted.Add(new(r + p * root.Value, p * root.Period));
        }
        return lifted;
    }
}
