# Breaking Changes

This release corrects the library's handling of **special values** (infinity, zero, and NaN)
to match **IEEE 754-2019**. Where IEEE 754 leaves a function's special cases open (`Pow`, `RootN`),
the C99 Annex F results are used. These corrections change observable results for edge-case
inputs. If your code depends on the previous behavior for these inputs, update it.

The baseline is release **0.5.2**. Every row below was confirmed by evaluating each public
static operation of `Float128` on 0.5.2 and on this release over the inputs `NaN`, `±inf`, `±0`,
`±1`, `±2`, `±0.5`, `±2.5`, `±3`, and the smallest subnormal `±ε`. In that comparison, results for
finite inputs changed only for `Tanh` and `Asinh` (see §4).

## 1. NaN results now have the sign bit clear

The library has two NaN values. Both have payload 1, and they differ only in the sign bit:

- `qNaN` (sign bit clear), and
- `sNaN` (sign bit set).

Most invalid operations used to return `sNaN`. They now return `qNaN`, as IEEE 754 requires for
an invalid operation. The NaN propagation cases listed below also return `qNaN` now.

`IsNaN` is unchanged and returns `true` for both. Only the sign bit of the result changes, so
this matters only if you inspect the raw bits or the sign of a NaN.

> **Note.** Neither NaN sets the IEEE 754 quiet bit (the most significant bit of the trailing
> significand), so both are signaling NaNs by IEEE bit encoding. This release does not change
> that encoding.

| Operation | Inputs whose NaN result now has the sign bit clear |
|-----------|----------------------------------------------------|
| `+` / `-` | `inf − inf` (for example `inf + (−inf)`) |
| `*` | `±inf · ±0` |
| `/` | `±0 / ±0`, `±inf / ±inf` |
| `Ieee754Remainder` | `±inf` with a finite nonzero divisor |
| `FusedMultiplyAdd` | `±inf · ±0` (any addend); an infinite product plus the opposite infinity |
| `Sin` / `Cos` / `SinPi` / `CosPi` | `±inf` |
| `Asin` / `AsinPi` / `Acos` / `AcosPi` | `\|x\| > 1`, including `±inf`; also a NaN input for `Asin` / `Acos` |
| `Atan2` | a NaN operand |
| `Acosh` | `x < 1` |
| `Atanh` | `\|x\| > 1`, including `±inf`; a NaN input |
| `Log` / `Log2` / `Log10` | `x ≤ 0` |
| `Log(y, newBase)` | `y ≤ 0` or `newBase ≤ 0` |
| `Sqrt` | `x < 0` |
| `RootN` | `n == 0`; `x < 0` with even `n` |

## 2. Edge cases now return the IEEE 754 result

### Arithmetic

| Operation | Input | Before | After |
|-----------|-------|--------|-------|
| `+` / `-` | `±inf` with a NaN operand | `±inf` | `qNaN` |
| `/` | `±inf / ±0` | `sNaN` | `±inf` (sign is the XOR of the operand signs) |
| `/` | nonzero subnormal `/ ±0` | throws `DivideByZeroException` | `±inf` (sign is the XOR of the operand signs) |
| `%` and `Ieee754Remainder` | `±inf` with a divisor of `±0`, `±inf`, or NaN | `±inf` | `qNaN` |
| `%` and `Ieee754Remainder` | nonzero subnormal with a divisor of `±0` | throws `DivideByZeroException` | `qNaN` |

### Powers and roots

| Operation | Input | Before | After |
|-----------|-------|--------|-------|
| `Pow` | `x = ±0`, `y = ±0` | `sNaN` | `1` |
| `Pow` | `x = 1`, `y` NaN or `±inf` | `qNaN` | `1` |
| `Pow` | `x = −1`, `y = ±inf` | `qNaN` | `1` |
| `Pow` | `x = ±0`, `y < 0` (including `−inf`) | `sNaN` | `+inf`; `−inf` when `x = −0` and `y` is an odd negative integer |
| `Pow` | `x = ±0`, `y = +inf` or a positive non-integer | `qNaN` | `+0` |
| `Pow` | `x = −inf`, non-integer `y` | `qNaN` | `+inf` for `y > 0`; `+0` for `y < 0` |
| `Pow` | `x` a nonzero subnormal, `y = ±inf` | `qNaN` | `+0` for `y = +inf`; `+inf` for `y = −inf` |
| `Sqrt` | `−0` | `+0` | `−0` |
| `Cbrt` | `±inf` | `qNaN` | `±inf` |
| `Cbrt` | `−0` | `+0` | `−0` |
| `RootN` | `x = ±0`, `n < 0` | `+0` | `+inf`; `−inf` when `x = −0` and `n` is odd |
| `RootN` | `x = −0`, odd `n > 0` | `+0` | `−0` |
| `RootN` | `x = ±inf`, `n = 3` | `qNaN` | `±inf` |

### Logarithms

| Operation | Input | Before | After |
|-----------|-------|--------|-------|
| `Log(y, newBase)` | `y = +inf`, `0 < newBase < 1` | `+inf` | `−inf` |
| `Log(y, newBase)` | `newBase = 1`, `y ≠ 1` | `−inf` (`+inf` for `y = +inf`) | `qNaN`, as `System.Math.Log(a, 1)` returns NaN |

`Log(1, 1)` still returns `0`.

### Trigonometric and hyperbolic

| Operation | Input | Before | After |
|-----------|-------|--------|-------|
| `Atan2(y, x)` | `(−0, +0)` | `+0` | `−0` |
| `Atan2(y, x)` | `(+0, −0)` | `+0` | `π` |
| `Atan2(y, x)` | `(−0, −0)` | `+0` | `−π` |
| `Atan2Pi(y, x)` | `(−0, +0)`, `(+0, −0)`, `(−0, −0)` | `+0` | `−0`, `1`, `−1` |
| `Tanh` | `±inf` | `sNaN` | `±1` |
| `Tanh` / `Asinh` | `−0` and negative subnormals | `+0` | `−0` (the input) |
| `Sinh` / `Atanh` | `−0` | `+0` | `−0` |
| `Atanh` | `−1` | `qNaN` | `−inf` |

## 3. Classification methods

| Method | Input | Before | After |
|--------|-------|--------|-------|
| `IsInteger` | `±inf` | `true` | `false` |
| `IsInteger` | NaN | `true` | `false` |

## 4. Finite results that differ in the last digits

- **`Tanh`** is now computed as `CopySign((1 − e) / (1 + e), x)` with `e = Exp(−2·|x|)`. This form
  is exactly odd, returns `±1` at `±inf` instead of NaN, and no longer divides one infinity by
  another. Finite results differ from 0.5.2 at about the 17th significant digit. Both forms are
  within the existing error of `Exp`, and the largest error seen in the comparison improved
  (`Tanh(2)`: about `5.5e-15` before, about `1.2e-19` after).
- **`Asinh`** of a negative `x` is now computed as `−Asinh(|x|)`, so it is exactly odd and keeps
  the sign of `−0`. Results for negative inputs differ from 0.5.2 at about the 17th significant
  digit. Results for positive inputs are unchanged.

## Migration guidance

- **Most consumers are unaffected.** Apart from the `Tanh` and `Asinh` digits in §4, only results
  for special-value inputs (infinity, zero, NaN) change.
- If you compare NaN bit patterns or read a NaN's sign, expect the sign bit to be clear (§1).
- Code that caught `DivideByZeroException` from dividing a subnormal by zero, or from `%` with a
  zero divisor, now gets an infinity or a NaN instead (§2).
- `IsNaN`, `IsInfinity`, `IsZero`, and `IsFinite` keep their contracts. Only the values produced
  by the operations above differ.

## References

- IEEE 754-2019, §6 (infinity, NaNs, and the sign bit) and §9.2 (recommended operations).
- ISO/IEC 9899 (C99), Annex F.9 (special cases of `pow`, `atan2`, and the hyperbolic functions).
