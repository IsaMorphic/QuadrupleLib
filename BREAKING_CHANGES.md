# Breaking Changes

This release corrects the library's handling of **special values** (infinity, zero, and NaN)
to match **IEEE 754-2019**. Where IEEE 754 leaves a function's special cases open (`Pow`, `RootN`),
the C99 Annex F results are used. It also corrects the **rounding** of finite results, so that
arithmetic, conversions, and parsing return the correctly rounded value (§5). These corrections
change observable results. If your code depends on the previous behavior, update it.

The baseline is release **0.5.2**. Every row below was confirmed by evaluating each public
static operation of `Float128` on 0.5.2 and on this release over the inputs `NaN`, `±inf`, `±0`,
`±1`, `±2`, `±0.5`, `±2.5`, `±3`, and the smallest subnormal `±ε`. In that comparison, apart from
the rounding corrections in §5, results for finite inputs changed only for `Tanh` and `Asinh`
(see §4).

## 1. NaN results now have the sign bit clear

The library has two NaN values:

- `qNaN`, which is `Float128.NaN`: sign bit clear, trailing significand 1.
- `sNaN`: in 0.5.2 this was the same pattern with the sign bit set. It now has the sign bit clear
  and only the most significant bit of the trailing significand (the IEEE 754 quiet bit) set.
  `Parse` returns it for a string that is not a number.

Most invalid operations used to return the 0.5.2 `sNaN`. They now return `qNaN`. The NaN
propagation cases listed below also return `qNaN` now.

`IsNaN` is unchanged and returns `true` for both. Only the bit pattern of the result changes, so
this matters only if you inspect the raw bits or the sign of a NaN.

> **Note.** `qNaN` does not set the IEEE 754 quiet bit, so by IEEE bit encoding it is a signaling
> NaN. This release does not change that encoding.

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
| `Cosh` | `±0` | `+0` | `1` |

### Comparison

| Operation | Input | Before | After |
|-----------|-------|--------|-------|
| `CompareTo` | `x.CompareTo(NaN)`, `x` not NaN | `int.MinValue`, so NaN sorted above every value | positive: NaN sorts below every value, as `double.CompareTo` does |
| `CompareTo` | `NaN.CompareTo(NaN)` | nonzero | `0` |

### `UInt256`

| Operation | Input | Before | After |
|-----------|-------|--------|-------|
| explicit conversion from `BigInteger` | `2^128 ≤ n < 2^256` | `OverflowException` | the exact value |

### Byte layout (`IFloatingPoint<T>`)

| Operation | Input | Before | After |
|-----------|-------|--------|-------|
| `GetSignificandByteCount` | any value | `14` | `16` (the significand is written as a full `UInt128`, as `double` writes a full `ulong`) |
| `TryWriteSignificandBigEndian`, `TryWriteSignificandLittleEndian` | destination of 16 bytes or more | `false`, nothing written (the 16-byte `UInt128` did not fit the 14-byte slice) | `true`, 16 bytes written |
| `TryWriteExponent*`, `TryWriteSignificand*` | destination shorter than the byte count | `ArgumentOutOfRangeException` | `false`, `bytesWritten` is `0` |

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

## 5. Correctly rounded results

The operations below now round to nearest, ties to even, as IEEE 754 requires. Before, the
rounding step read the guard, round, and sticky bits at the wrong positions, and several
operations dropped or mis-set the sticky bit. A result could be one unit in the last place off,
and an exact tie did not always round to even.

| Operation | Before | After |
|-----------|--------|-------|
| `+`, `-`, `*`, `/` | could be off by one unit in the last place, in either direction | correctly rounded |
| `FusedMultiplyAdd` | only the top bits of the product were kept before the addition; a product that cancels against the addend, or a subnormal product with a zero addend, could give `0` | the exact product is added, then rounded once |
| `ScaleB` to a subnormal result | could be off by one unit in the last place | correctly rounded |
| conversion of a subnormal `double`, `float`, or `Half` to `Float128` | a quarter of the correct value | exact |
| conversion of `Float128` to `double`, `float`, or `Half` | could be off by one unit in the last place; a subnormal result was four times the correct value, so the round trip only worked because both errors cancelled | correctly rounded |
| conversion of `int.MinValue`, `long.MinValue`, or `Int128.MinValue` to `Float128` | a wrong value (for example about `−1.7E+38` for `int.MinValue`) | exact |
| conversion of `Float128` to an integer type, when the value is at least `2^112` | `0` or a wrong value (for example `(Int128)(Float128)Int128.MinValue` was `0`) | exact; out of range, a `checked` conversion throws `OverflowException`, and an unchecked conversion keeps the low bits of the two's complement value, as a conversion between integer types does (`(uint)(Float128)(-1)` is `uint.MaxValue`); NaN and infinities throw `OverflowException` in both |
| `Round` | `0.5 < \|x\| < 1` gave `0`; a value above a half with further fraction bits (for example `2.6`) rounded down | nearest integer, ties to even |
| `Round` | a result of zero was always `+0` | keeps the sign of the input (`Round(−0.3)` is `−0`) |
| `Parse` | could be off by one unit in the last place (for example `"0.1"`) | correctly rounded |
| `Parse` with an exponent | the decimal point moved to the wrong place when the whole part had more than one digit, or the string had a sign (`"12.5e1"` gave `12.5`, `"-1.25e1"` gave `-1.25`); an exponent beyond the `int` range was ignored (`"1e99999999999"` gave `1`) | correctly rounded; an exponent beyond the range of the format gives `±inf` or `±0` |
| `ToString` with an exponent, for values below 1 | the exponent was one too large (`1E-30` printed as `1.0…E-29`) | correct exponent |
| `ToString` whenever `NumberDecimalDigits` cuts digits off | 0.5.2: one digit too few was kept, and a carry or a value below 10 could print a wrong magnitude (`9.96` at 3 digits gave `"1"`, `0.123` with `InvariantCulture` gave `"0"`); 0 or 1 digits threw `IndexOutOfRangeException` for values below 10. 0.6.3: the digits were truncated rather than rounded (`9.96` at 2 digits gave `"9.9"`), values below 1 kept one decimal place too few (`0.123` with `InvariantCulture` gave `"0.1"`), and 0 digits could print a wrong magnitude (`9.96` gave `"1"`, `1.5E30` gave `"0E+30"`) | `NumberDecimalDigits` is the number of digits after the decimal point (of the mantissa, in E notation), and the exact value is rounded half to even, as `double.ToString("F")` and `double.ToString("E")` do |
| `ToString()` with no provider (38 decimal places) | 0.5.2: values below 10 showed 36 decimal places and larger values 37; 0.6.3: 37 decimal places, truncated. In both, E notation kept the trailing zeros of values of 10^23 and above (`1.5E+30` printed as `1.500000000000000000000000000000E+30`) | 38 decimal places for every value in plain notation; trailing zeros are trimmed in E notation too (`1.5E+30`) |
| `Exp`, `Exp10` | `NaN` when the result overflows or underflows | `+inf` or `0` |
| `Hypot` | `+inf` when `x² + y²` overflows, even if the result does not | the scaled result, without the intermediate overflow |
| `Pow` with an integer exponent outside `[-4, 4]` | computed as `Exp(y · Log(x))`, with a relative error up to about `1E-12` (`Pow(3, 5)` was not exactly `243`) | computed by repeated squaring: exact when the result is representable; otherwise a few units in the last place for small exponents, with an error that grows with the exponent |
| `RootN` with `\|n\| ≥ 4` | computed as `Pow(x, 1 / n)`, with a relative error up to about `1E-18` (`RootN(32, 5)` was not exactly `2`) | refined with Newton's method: exact when the result is representable, otherwise within one unit in the last place |

`Pow` with an integer exponent and `RootN` are not correctly rounded, but are now within a few
units in the last place. The repeated squaring error grows with the exponent (`0.9^100` is about 6
units off). `Pow` with a non-integer exponent still goes through `Exp` and `Log`, and is unchanged.

Two existing tests expected `2·2^-16382 ± 1·2^-16382` to be subnormal. The exact result is
`2^-16382`, the smallest normal number, so those tests now use operands whose result is subnormal.

## Migration guidance

- **Most consumers are unaffected.** Apart from the `Tanh` and `Asinh` digits in §4 and the
  rounding corrections in §5, only results for special-value inputs (infinity, zero, NaN)
  change. The rounding corrections change finite results by at most one unit in the last place,
  except where §5 lists a larger error that is now fixed.
- If you compare NaN bit patterns or read a NaN's sign, expect the sign bit to be clear (§1).
- Code that caught `DivideByZeroException` from dividing a subnormal by zero, or from `%` with a
  zero divisor, now gets an infinity or a NaN instead (§2).
- `IsNaN`, `IsInfinity`, `IsZero`, and `IsFinite` keep their contracts. Only the values produced
  by the operations above differ.

## References

- IEEE 754-2019, §6 (infinity, NaNs, and the sign bit) and §9.2 (recommended operations).
- ISO/IEC 9899 (C99), Annex F.9 (special cases of `pow`, `atan2`, and the hyperbolic functions).
