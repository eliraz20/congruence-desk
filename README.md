# Congruence Desk

Solve integer congruences ax ≡ b (mod m), including noninvertible coefficients.

## Try it

Install the .NET 10 SDK, open this folder, and run:

```sh
dotnet run -- example.tsv
```

Look for `x = 6 (mod 7)` in the output. To use your own data, copy the example and pass the new filename. Use `-` to read from standard input.

## Input

Integer coefficient a, integer right-hand side b, positive modulus m. Each solution describes one residue class modulo the reduced modulus. Separate columns with actual tabs, omit a header row, and use a period for decimal fractions.

Built for mathematics and statistics coursework. [ISCED-F field 054](https://uis.unesco.org/sites/default/files/documents/isced-fields-of-education-and-training-2013-en.pdf).
