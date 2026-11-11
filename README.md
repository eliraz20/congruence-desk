# Congruence Desk

Explore exact solution sets for modular equations in C#. Try a quadratic with a repeated root, a two-variable equation with an xy term, or a rectangular linear system over a composite modulus.

```sh
dotnet run -- example.tsv
```

Requires the .NET 10 SDK. Pass `-` to read from standard input. Separate fields with spaces or tabs. Blank lines and lines beginning with `#` are ignored.

| Input | Equation |
|---|---|
| `a b m` or `linear a b m` | ax ≡ b (mod m) |
| `quad a b c m` | ax² + bx + c ≡ 0 (mod m) |
| `conic A B C D E F m` | Ax² + Bxy + Cy² + Dx + Ey + F ≡ 0 (mod m) |
| `system m n` | Read any number of rows with n coefficients followed by a right-hand side. Finish with `end`. All rows share modulus m. |

## Worked examples

`quad 1 0 -1 8` gives four roots modulo 8. The solver can print these as smaller residue classes. For example, `1 (mod 4)` denotes both 1 and 5 modulo 8. The reported count always refers to the input modulus.

`conic 1 1 1 0 0 -1 5` groups solutions to x² + xy + y² ≡ 1 by x and reports the number of ordered pairs. At x = 0, y can be 1 or 4.

```text
system 6 3
1 1 1 3
0 2 2 4
end
```

The first row gives x + y + z ≡ 3. The second gives y + z ≡ 2 (mod 3). There are 12 solutions modulo 6. The output gives a base vector and independent direction vectors. Choose an integer in each printed parameter range, add the scaled vectors to the base, and reduce each coordinate modulo 6.

## Algorithms and limits

- Linear queries use extended Euclid with arbitrary-precision integer arithmetic.
- Linear systems use invertible row and column operations over the shared modulus, then solve the diagonal congruences with extended Euclid. This handles free variables and noninvertible pivots. Solution families stay compact even when the solution count is large.
- Quadratics factor the modulus, find base-field roots, lift to prime powers by polynomial substitution, and combine disjoint classes with the Chinese remainder theorem. The modulus can be 1 through 1,000,000. The largest prime factor determines the cost of the base-field scan.
- Two-variable quadratic queries examine each x and apply the quadratic solver to y. Their modulus can be 1 through 512. Linear systems support any number of variables that fit in memory.

Run `dotnet run --project checks/Checks.csproj` from this folder to check the CLI against direct enumeration of 1,770 small solution sets.
