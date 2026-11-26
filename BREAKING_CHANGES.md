# Breaking Changes

This release corrects the library's handling of **special values** (infinity, zero, and NaN)
to match **IEEE 754-2019**, except that the library NaN has no quiet bit (see §1). Where IEEE 754 leaves a function's special cases open (`Pow`, `Atan2`, and the hyperbolic functions), the C99 Annex F.9 results are used. `RootN` follows the `rootN` operation in IEEE 754-2019 §9.2. It also corrects the **rounding** of finite results, so that
arithmetic, conversions, and parsing return the correctly rounded value (§5). These corrections
change observable results. If your code depends on the previous behavior, update it.

This document covers the release after 0.7.1. Unless a row or bullet names 0.6.3 or 0.7.1, the "Before" columns show the **0.5.2** baseline, and changes made in 0.6.0 through 0.7.1 are recorded only where a row or bullet names that release. If you upgrade from 0.7.1, compare your current results with the "After" column, because 0.7.1 may already differ from the "Before" column. A "not recorded" row was not measured at 0.5.2, so only its "After" value applies.

Some finite results change in the last digits. They do so for `Tanh` and `Asinh` (§4), for the
rounding corrections (§5), and for the transcendental functions listed in §6. Other finite results change by more; §2 and §5 list them.

A **double-double** value is an unevaluated sum `hi + lo` of two binary128 values. Sections 4 to 6 use the term.

## 1. Most NaN results now have the sign bit clear

Results carry one of three NaN patterns:

- library NaN, which is `Float128.NaN`: sign bit clear, trailing significand 1.
- quiet NaN: sign bit clear and only the most significant bit of the trailing significand (the IEEE 754 quiet bit) set.
  The generic `Parse` and `TryParse` return it for a string that is not a number.
- 0.5.2 NaN: in 0.5.2 this was the library NaN pattern with the sign bit set. The operations in §2 and the generic
  `Parse` and `TryParse` no longer return it. The non-generic `Parse` and `TryParse` return this pattern for a string that is not a number (see the note after the table). Unary `-` and `CopySign` set the sign bit of a NaN, as IEEE 754 does, so they can still produce it, for example `-Float128.NaN`.

Most invalid operations used to return the 0.5.2 NaN. They now return the library NaN. The NaN
propagation cases listed below also return the library NaN now. In the Before columns of §2, "library NaN" is the sign-clear pattern that 0.5.2 already returned for that case; "0.5.2 NaN" is the sign-set pattern.

`IsNaN` is unchanged and returns `true` for all three NaN patterns, so this matters only if you inspect the raw bits or the sign of a NaN.

> **Note.** The library NaN does not set the IEEE 754 quiet bit, so by IEEE bit encoding it is a signaling
> NaN. This release does not change that encoding.

| Operation | Inputs whose NaN result now has the sign bit clear |
|-----------|----------------------------------------------------|
| `+` / `-` | `inf − inf` (for example `inf + (−inf)`) |
| `*` | `±inf · ±0` |
| `/` | `±0 / ±0`, `±inf / ±inf` |
| `%` and `Ieee754Remainder` | `±inf` with a finite nonzero divisor |
| `FusedMultiplyAdd` | `±inf · ±0` (any addend); an infinite product plus the opposite infinity |
| `Sin` / `Cos` / `SinCos` / `Tan` / `SinPi` / `CosPi` / `SinCosPi` / `TanPi` | `±inf` |
| `Asin` / `AsinPi` / `Acos` / `AcosPi` | `\|x\| > 1`, including `±inf`; also a NaN input for `Asin` / `Acos` |
| `Atan2` | a NaN operand |
| `Acosh` | `x < 1` |
| `Atanh` | `\|x\| > 1`, including `±inf`; a NaN input |
| `Pow` | finite `x < 0` with a non-integer `y` |
| `Log` / `Log2` / `Log10` | `x < 0` |
| `Log(y, newBase)` | `y < 0`, `newBase < 0`, `newBase = 0` with `y ≠ 1`, `newBase = 1`, or `newBase = +inf` with `y ≠ 1` |
| `Sqrt` | `x < 0` |
| `RootN` | `n == 0`; `x < 0` with even `n` |
| `Parse`, `TryParse` (generic) | a string that is not a number (returns the quiet NaN) |

> **Known inconsistency.** For a string that is not a number, the non-generic `Parse` and `TryParse` return the sign-set
> signaling NaN (`FFFF0000…0001`, the 0.5.2 NaN pattern), not the quiet NaN that the generic methods return. This release
> keeps both behaviours.

## 2. Edge cases now return the IEEE 754 result

### Arithmetic

| Operation | Input | Before | After |
|-----------|-------|--------|-------|
| `+` / `-` | `±inf` with a NaN operand | `±inf` | library NaN |
| `/` | `±inf / ±0` | 0.5.2 NaN | `±inf` (sign is the XOR of the operand signs) |
| `/` | nonzero subnormal `/ ±0` | throws `DivideByZeroException` | `±inf` (sign is the XOR of the operand signs) |
| `%` and `Ieee754Remainder` | `±inf` with a divisor of `±0`, `±inf`, or NaN | `±inf` | library NaN |
| `%` and `Ieee754Remainder` | nonzero subnormal with a divisor of `±0` | throws `DivideByZeroException` | library NaN |

A NaN operand gives library NaN for both `%` and `Ieee754Remainder`. The two differ only in the quotient:
`%` truncates it, so the result has the sign of the dividend, and `Ieee754Remainder` rounds it to
nearest, ties to even.

### Powers and roots

| Operation | Input | Before | After |
|-----------|-------|--------|-------|
| `Pow` | `x = ±0`, `y = ±0` | 0.5.2 NaN | `1` |
| `Pow` | `x = 1`, `y` NaN or `±inf` | library NaN | `1` |
| `Pow` | `x = −1`, `y = ±inf` | library NaN | `1` |
| `Pow` | `x = ±0`, `y < 0` (including `−inf`) | 0.5.2 NaN | `+inf`; `−inf` when `x = −0` and `y` is an odd negative integer |
| `Pow` | `x = ±0`, `y = +inf` or a positive non-integer | library NaN | `+0` |
| `Pow` | `x = −inf`, non-integer `y` | library NaN | `+inf` for `y > 0`; `+0` for `y < 0` |
| `Pow` | `x` a nonzero subnormal, `y = ±inf` | library NaN | `+0` for `y = +inf`; `+inf` for `y = −inf` |
| `Pow` | `x` a positive subnormal, finite non-integer `y` | library NaN | a finite value (`Pow(Epsilon, 0.5)` is about `2.5E-2483`) |
| `Pow` | `x` a subnormal, integer `y` with `\|y\| ≥ 5` | library NaN, except `Pow(−Epsilon, 5)` and `Pow(−Epsilon, −5)`, which are 0.5.2 NaNs | `Pow(Epsilon, 5)` is `+0`; `Pow(Epsilon, −5)` is `+inf`; `Pow(−Epsilon, 5)` is `−0`; `Pow(−Epsilon, −5)` is `−inf` |
| `Sqrt` | `−0` | `+0` | `−0` |
| `Cbrt` | `±inf` | library NaN | `±inf` |
| `Cbrt` | `−0` | `+0` | `−0` |
| `RootN` | `x = ±0`, `n < 0` | `+0` | `+inf`; `−inf` when `x = −0` and `n` is odd |
| `RootN` | `x = −0`, odd `n > 0` | `+0` | `−0` |
| `RootN` | `x = ±inf`, `n = 3` | library NaN | `±inf` |

### Logarithms

| Operation | Input | Before | After |
|-----------|-------|--------|-------|
| `Log`, `Log2`, `Log10` | `±0` | 0.5.2 NaN | `−inf` |
| `Log`, `Log10` | a positive subnormal | library NaN | a finite value (`Log(Epsilon)` is about `−11432.77`; `Log10(Epsilon)` is about `−4965.19`) |
| `Log(y, newBase)` | `y = ±0` | 0.5.2 NaN | `+inf` for `0 < newBase < 1`; `−inf` for `1 < newBase < +inf`; library NaN for `newBase ≤ 0` |
| `Log(y, newBase)` | `y = +inf`, `0 < newBase < 1` | `+inf` | `−inf` |
| `Log(y, newBase)` | `y = 1`, `0 < newBase < 1` | `0` | `−0` (`+0` divided by a negative `ln(newBase)`) |
| `Log(y, newBase)` | `y = 1`, `newBase = ±0` | 0.5.2 NaN | `−0` (`+0` divided by `ln(±0) = −inf`, as `System.Math.Log(1, ±0)`) |
| `Log(y, newBase)` | `newBase = 1`, any `y` | `0` for `y = 1`; `+inf` for `y = +inf`; `−inf` for `0 < y < +inf`, `y ≠ 1`; 0.5.2 NaN for `y ≤ 0`; library NaN for NaN `y` | library NaN, as `System.Math.Log(a, 1)` returns NaN |
| `Log(y, newBase)` | `newBase = +inf` (any `y`) | `+0` for `y = 1`; `+inf` for `y = +inf`; library NaN otherwise | library NaN for `y ≠ 1`; `Log(1, +inf)` is `+0` |
| `Log(y, newBase)` | `y` a positive subnormal, `newBase = 10` or `2` | library NaN for `newBase = 10`; `+16469` for `newBase = 2` (wrong; the value is `−16494`) | `Log(Epsilon, 2)` is `−16494`; `Log(Epsilon, 10)` is about `−4965.19` |

### Trigonometric and hyperbolic

| Operation | Input | Before | After |
|-----------|-------|--------|-------|
| `Atan2(y, x)` | `(−0, +0)` | `+0` | `−0` |
| `Atan2(y, x)` | `(+0, −0)` | `+0` | `π` |
| `Atan2(y, x)` | `(−0, −0)` | `+0` | `−π` |
| `Atan2(y, x)` | `(−0, x)` for `x < 0`, for example `(−0, −1)` and `(−0, −inf)` | `+π` (`3.14159265364…` for `(−0, −1)`, about `5E-11` above `π`) | `−π` |
| `Atan2(y, x)` | `(+inf, −inf)` | library NaN | `+3π/4` |
| `Atan2(y, x)` | `(−inf, −inf)` | library NaN | `−3π/4` |
| `Atan2(y, x)` | `(+inf, +inf)` | library NaN | `+π/4` |
| `Atan2(y, x)` | `(−inf, +inf)` | library NaN | `−π/4` |
| `Atan2(y, x)` | `(±inf, x)` for finite `x` | `±π/2` (already correct) | `±π/2` |
| `Atan2(y, x)` | `(y, +inf)` for finite `y` | `+5.18E-11` for both signs of `y` (not `±0`) | `±0` |
| `Atan2(y, x)` | `(y, −inf)` for finite `y` | `+π` for `y = +1` and `−π` for `y = −1`, each off by about `5E-11` | `±π` |
| `Atan2Pi(y, x)` | `(−0, +0)`, `(+0, −0)`, `(−0, −0)` | `+0` | `−0`, `1`, `−1` |
| `AcosPi(x)` | `x = 1` | `1.6480E-11` (not `0`) | `+0` |
| `AcosPi(x)` | `x = −1` | `1 + 1.6480E-11` (not exactly `1`) | `1` |
| `Tanh` | `±inf` | 0.5.2 NaN | `±1` |
| `Tanh` / `Asinh` | `−0` | `+0` | `−0` |
| `Tanh` / `Asinh` | a negative subnormal | `+0` | returned unchanged |
| `Tanh` / `Asinh` | every `x` with `\|x\| < 2^-57` | wrong for `x ≠ 0`: `Tanh(1E-18)` is `9.9806E-19` and `Asinh(1E-18)` is `8.3390E-19` (the value is `1E-18`) | returned unchanged |
| `Sinh` / `Atanh` | `−0` | `+0` | `−0` |
| `Atanh` | `−1` | library NaN | `−inf` |
| `Sin`, `Cos`, `SinCos` | finite `\|x\| ≥ 2^112` | a finite value, but wrong: `Sin(1E+34)` is `−0.9093` (the value is `−0.6861`), and `Sin(1E+300)` is `+0` | library NaN (sign bit clear) since 0.6.0; §6 |
| `Tan` | finite `\|x\| ≥ 2^112` | a finite value, but wrong: `Tan(1E+34)` is `2.185` (the value is `0.9431`) | library NaN (sign bit clear) since 0.6.0; §6 |
| `SinPi`, `CosPi`, `SinCosPi`, `TanPi` | finite `x` with `\|x\|` about `1.65E+33` or more (where `x · Pi` reaches 2^112) | `+0` for `SinPi`, `SinCosPi`, and `TanPi` and `1` for `CosPi` at `1E+34` (exact) | library NaN (sign bit clear) since 0.6.0; §6 |
| `SinPi`, `CosPi`, `SinCosPi`, `TanPi` | `x` NaN or ±inf | 0.5.2 NaN for `±inf` in `SinPi`, `CosPi`, `SinCosPi`; library NaN for `TanPi`, and for a NaN input | library NaN (sign bit clear) for each output |

### Comparison

| Operation | Input | Before | After |
|-----------|-------|--------|-------|
| `CompareTo` | `x.CompareTo(NaN)`, `x` not NaN | a negative value, so NaN sorted above every value (`−32767` for `One`, `−1` for `−One`) | a positive value, so NaN sorts below every value, as `double.CompareTo` does |
| `CompareTo` | `x.CompareTo(y)`, with `x` and `y` both NaN and different sign bits | nonzero (NaNs with the same sign bit already returned `0`) | `0` |

### `UInt256`

| Operation | Input | Before | After |
|-----------|-------|--------|-------|
| explicit conversion from `BigInteger` | `2^128 ≤ n < 2^256` | `OverflowException` | the exact value |

### Byte layout (`IFloatingPoint<T>`)

| Operation | Input | Before | After |
|-----------|-------|--------|-------|
| `GetSignificandByteCount` | any value | `14` | `16` (the significand is written as a full `UInt128`, as `double` writes a full `ulong`) |
| `TryWriteSignificandBigEndian`, `TryWriteSignificandLittleEndian` | destination of 16 bytes or more | `false`, nothing written (the 16-byte `UInt128` did not fit the 14-byte slice) | `true`, 16 bytes written |
| `TryWriteExponent*`, `TryWriteSignificand*` | destination shorter than the byte count | `ArgumentOutOfRangeException` below 2 bytes (exponent) or 14 bytes (significand); for a 14- or 15-byte significand destination, `false`, nothing written, and `bytesWritten` set to `14`, the old byte count | `false`, nothing written, and `bytesWritten` is `0` |
| `TryWriteSignificand*` | destination of 14 bytes or more | `false` for every destination: the 14-byte slice is too short for the 16-byte value, so nothing is written and `bytesWritten` is set to `14`, the old byte count | `true` for a destination of 16 bytes or more (16 bytes written) |

## 3. Classification methods

| Method | Input | Before | After |
|--------|-------|--------|-------|
| `IsInteger` | `±inf` | `true` | `false` |
| `IsInteger` | NaN | `true` | `false` |

## 4. Finite results that differ in the last digits

- **`Tanh`** is now computed as `CopySign((e2 − 1) / (e2 + 1), x)` with `e2 = e^(2·|x|)` in
  double-double (a `hi + lo` pair, defined in the introduction), and returns `±1` for `|x| ≥ 45`. This form is exactly
  odd, returns `±1` at `±inf` instead of NaN, and no longer divides one infinity by another. Finite
  results differ from 0.5.2 in the last digits.
- **`Asinh`** of a negative `x` is now computed as `−Asinh(|x|)`, so it is exactly odd and keeps
  the sign of `−0`. Results for negative inputs differ from 0.5.2 in the last digits. Results for
  positive inputs change only as described in §6.

## 5. Rounding and overflow changes

Operations whose results changed. Arithmetic, conversions, and parsing now round to nearest, ties to even, as IEEE 754 requires. Before, the rounding step read the guard bit (the first bit below the rounding position), the round bit (the second), and the sticky bit at the wrong positions, and several operations dropped or mis-set the sticky bit (a flag that records whether any nonzero bit was discarded below the rounding position). A result could be one unit in the last place off, and an exact tie did not always round to even. `Pow`, `RootN`, `Exp10`, and `Hypot` are not guaranteed to be correctly rounded in every case. The other functions in §6 are not either, except `Sqrt`. Here `ilogb x` is the unbiased binary exponent of `x`.

| Operation | Before | After |
|-----------|--------|-------|
| `+`, `-`, `*`, `/` | could be off by one unit in the last place, in either direction. A difference of nearly equal values could also come out as `0`: `1 − (1 − 2^-113)` returned `0`, not `2^-113` | correctly rounded |
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
| `ToString` whenever `NumberDecimalDigits` cuts digits off | 0.5.2: one digit too few was kept, and a carry or a value below 10 could print a wrong magnitude (`9.96` at 3 digits gave `"1"`, `0.123` with `InvariantCulture` gave `"0"`); 0 or 1 digits threw `IndexOutOfRangeException` for values below 10. 0.6.3: the digits were truncated rather than rounded (`9.96` at 2 digits gave `"9.9"`), values below 1 kept one decimal place too few (`0.123` with `InvariantCulture` gave `"0.1"`), and 0 digits could print a wrong magnitude (`9.96` gave `"1"`, `1.5E30` gave `"0E+30"`) | `NumberDecimalDigits` is the number of digits after the decimal point (of the mantissa, in E notation), and the exact value is rounded half to even |
| `ToString()` with no provider (38 decimal places) | 0.5.2: values below 10 showed 36 decimal places and larger values 37; 0.6.3: 37 decimal places, truncated. In both, E notation kept the trailing zeros of values of 10^23 and above (`1.5E+30` printed as `1.500000000000000000000000000000E+30`) | 38 decimal places for every value in plain notation; trailing zeros are trimmed in E notation too (`1.5E+30`) |
| `Exp`, `Exp10`, `Exp2` | library NaN when the result overflows or underflows. For `Exp2` with an integer `y`, 0.5.2 gave `+inf` on overflow and a wrong finite value on underflow (`Exp2(-20000)` gave `1.957E-4941`) | `+inf` or `0` |
| `Hypot` | `+inf` when `x² + y²` overflows, even if the result does not | the scaled result, without the intermediate overflow |
| `Pow` with an integer exponent | `\|y\| ≤ 4`: not recorded. `\|y\| > 4`: computed as `Exp(y · Log(x))`, with a relative error up to about `1E-12` (`Pow(3, 5)` was not exactly `243`) | for `y = 2`, `x · x`, correctly rounded; for other integer `y` with `\|y\| < 2^20` and `\|y\| · (\|ilogb x\| + 1) < 16380`, repeated squaring in double-double (`Pow(3, 5)` is `243`); otherwise `2^(y · log2\|x\|)` in double-double (`Pow(2, 9000)` takes this path) |
| `RootN`, by `n` | `\|n\| ≥ 4`: computed as `Pow(x, 1 / n)`, with a relative error up to about `1E-18` (`RootN(32, 5)` was not exactly `2`); `n = 1, 2, 3, −1, −2, −3`: not recorded | `n = 1, 2, 3, −1` use `x`, `Sqrt`, `Cbrt`, and `1 / x`; other `n` use `2^(log2(x) / n)` in double-double, rounded once from a double-double value; not guaranteed correctly rounded |
| `Sqrt` | not recorded; the previous code used a Newton iteration | correctly rounded: the exact integer square root, with the remainder as a sticky bit, rounded once to nearest, ties to even |
| `%` (finite result) | not recorded | the exact remainder, computed in integers; the quotient is truncated, so the result has the sign of the dividend |
| `Ieee754Remainder` (finite result) | not recorded | the exact remainder, computed in integers; the quotient is rounded to nearest, ties to even |
| `%` and `Ieee754Remainder`, finite `x` with a `±inf` divisor | not recorded at 0.5.2; NaN in 0.7.1 | `x` |
| `IAccelerator.DivRem` (public API) | a wrong quotient or remainder for some dividends of 225 or more bits, when the divisor's top 32-bit digit is small (for example `2^255` divided by `2^96 + 1`, which returned quotient 0 and remainder 0) | correct for the tested dividends and divisors, including `2^255` divided by `2^96 + 1`, checked against `BigInteger.DivRem` |

These are computed in double-double and rounded once at the end; the double-double error means they are not guaranteed to be correctly rounded:

- `Pow` with an integer `y` other than `2`. For `|y| < 2^20` and `|y| · (|ilogb x| + 1) < 16380`, the
  result is built by repeated squaring, and its error grows with the number of multiplications, about
  2 · log2|y|. Otherwise, and for a non-integer `y`, the result is `2^t` with `t = y · log2|x|`.
- `RootN` with `n` other than `1`, `2`, `3`, `−1`: the result is `2^t` with `t = log2(x) / n`. `n = 3`
  uses `Cbrt`, which has the same form.
- `Exp10` and `Hypot`: not guaranteed to be correctly rounded.

In the repeated-squaring path the error grows with log2|y| (about 2 · log2|y| roundings); in the 2^t path it grows with |t|. No end-to-end error bound is recorded for these paths. §6 gives the truncation bounds of the double-double series they use, and the reference tests in §6 are the only measured checks.

## 6. Transcendental functions

Each double-double series has a truncation bound, stated in the comment above it in `QuadrupleLib/Modules/DoubleDoubleOperations.cs` (for example, the omitted terms of the `ln(1 + z)` series are below 2^-130 relative). These bound series truncation only, not the total error of a function (see §5). The tolerances the tests check are listed below. The measured checks are the reference tests, such as `ExpOfReferenceValuesIsWithinTwoUlpsOfReference` (ExponentialTests) and `LogOfReferenceValuesIsWithinTwoUlpsOfReference` (LogarithmTests), which compare fixed inputs to references of 40 to 70 significant digits (`Exp`, `Log`; `Sinh` at 60 digits), and allow an error of at most 2 ulp (units in the last place) for `Exp`, `Log` and the hyperbolic functions; the double-reference tests use the relative bounds below. The functions these tolerances cover are `Exp`, `Exp2`, `Exp10`, `Pow`, `RootN`, `Cbrt`, `Log`, `Log2`, `Log10`, `Log(x, base)`, `Hypot`, the inverse trigonometric functions, `Atan2`, and the hyperbolic functions. The `Sin`, `Cos`, `Tan`, `SinPi`, `CosPi`, `SinCosPi`, and `TanPi` tests compare one entry point with another or check quadrant signs and special values, and have no reference-value tolerance (see the trigonometric paragraphs below). `AsinPi`, `AcosPi`, `AtanPi`, and `Atan2Pi` are checked against reference values at 1E-32.

`Exp`, `Log`, `Log2`, `Log10`, `Log(x, base)`, `Exp2`, `Exp10`, `Cbrt`, `RootN` (`n` other than `1`, `2`,
`3`, `−1`), `Hypot`, the non-integer `Pow`, `Asin`, `Acos`, `Atan`, and `Atan2`, and the
hyperbolic functions (including `Asinh` and `Acosh`) evaluate in double-double arithmetic, then round the final `hi + lo` to binary128 once. Because of the double-double error before that step, the results are not guaranteed to be correctly rounded. The `WithinUlps` tests in
`HyperbolicTests` check results to within 2 ulp (`SinhAndTanhAtSmallArgumentThresholdIsWithinOneUlp` checks 1 ulp at 2^-57), and the double-reference tests check to 1E-15 relative (`ExponentialTests`, `HyperbolicTests`). The tests that compare against a fixed reference with a relative bound, or that check a round trip, use a 1E-32 relative tolerance in `ExponentialTests`, `HyperbolicTests`, `LogarithmTests`, and `TrigonometryTests`, except where a test names its own bound (`TrigonometryTests`: 1E-15 in one test; `ExponentialTests`: 1E-22 in one; `PowerAndRootTests`: 1E-33 in one). The reference-value tests `ExpOfReferenceValuesIsWithinTwoUlpsOfReference` and `LogOfReferenceValuesIsWithinTwoUlpsOfReference` instead allow 2 ulp through `WithinUlps`.

In the double-double functions, a result scaled into the subnormal range is scaled a second time. Such a result is
within one unit in the last place and is not guaranteed to be correctly rounded. Arithmetic and `ScaleB` round a subnormal result once (§5).

The Pi variants (`TanPi`, `AtanPi`, `Atan2Pi`, `AsinPi`, and `AcosPi`) round more than once, because each divides two rounded values.
`SinPi`, `CosPi`, `SinCosPi`, and `TanPi` multiply `x` by `Pi` before the `Sin` and `Cos` kernel, so they do not
reduce `x` modulo 2 exactly. `AtanPi` and `Atan2Pi` divide a rounded result by `Pi` in binary128, and `AsinPi` and
`AcosPi` use `Atan2Pi`.

`Sin`, `Cos`, `SinCos`, `Tan`, `SinPi`, `CosPi`, `SinCosPi`, and `TanPi` are computed by a CoRDiC kernel in binary128
arithmetic, not in double-double. They are not correctly rounded, and they are not accurate to binary128. The argument
is reduced against the binary128 value of 2π, so the reduction error grows with `|x|`. No error bound is measured for this release, and some results are wrong by much more, including results
for 2^80 ≤ `|x|` < 2^112. Finite arguments with `|x|` ≥ 2^112 return library NaN, and so do the pi variants from `|x|` about 1.65E+33 (where `x · Pi` reaches 2^112), as §2 records. This is a known
limitation of this release.

`Pi` is the binary128 rounding of π. It is below π by about 8.7E-35. `SinPi`, `CosPi`, `SinCosPi`, and `TanPi`
include that rounding, because they multiply `x` by `Pi`.

`Tan` and `TanPi` divide two rounded values, so they round more than once. `Sqrt` takes the exact
integer square root of the input, with the remainder as a sticky bit, and rounds it once, to nearest,
ties to even.

`Exp` scales its argument by `1/ln 2` in double-double and rounds once at the final `hi + lo`. Its
result is not guaranteed to be correctly rounded. The high part of the ln 2 split has its set bits spread over 91 bit positions, so `k` times it is exact for `|k| <= 2^20`. `Sinh` and `Cosh` evaluate `e^|x|` in double-double,
and for `|x| ≥ 45` use half of `e^|x|`. `Tanh` evaluates `e^(2|x|)`.

Results for these functions can change in the last digits. Speed relative to 0.5.2 has not been measured.

## Migration guidance

- **Special-value results change** as listed in §1–§3. **Finite results change in the last digits**
  for `Tanh` and `Asinh` (§4), for the rounding corrections (§5), and for the functions in §6. Where
  §5 lists a larger error that is now fixed, the change is larger. Some finite results change by more than the last digit (§2).
- Some zero and subnormal results change sign or value, so a NaN check does not find them. `Sqrt(−0)`, `Cbrt(−0)`, `Atan2(−0, +0)`, `Atan2Pi(−0, +0)`, `Sinh(−0)`, `Asinh(−0)`, `Atanh(−0)`, and `Tanh(−0)` now return `−0` where 0.5.2 returned `+0`; `Tanh` and `Asinh` return a negative subnormal unchanged where 0.5.2 returned `+0` (§2).
- If you compare NaN bit patterns or read a NaN's sign, expect most NaN results to have the sign bit clear (§1).
- `Log(1, 1)` now returns NaN, as `System.Math.Log(1, 1)` does, instead of `0`. Code that expected
  `0` must change (§2).
- `Log`, `Log2`, and `Log10` of ±0 return `−inf` (§2). Code that tested for NaN after taking the log
  of zero should test `IsNegativeInfinity(x)` instead.
- `Log(y, newBase)` with `y = ±0` returns an infinity instead of NaN when `0 < newBase < +inf` and `newBase ≠ 1`.
  With `y = 1` and `newBase = ±0`, it returns `−0` instead of NaN (§2). Code that tested for NaN there should test the value instead.
- `Log` and `Log10` of a positive subnormal return a finite value where 0.5.2 returned NaN. `Log(y, newBase)` with a subnormal `y` returns a finite value for `newBase = 10` where 0.5.2 returned NaN, and for `newBase = 2` it changes `+16469` to `−16494` (§2).
- `Exp` and `Exp10`, and `Exp2` with a non-integer exponent, return `+inf` or `0` instead of library NaN when the
  result overflows or underflows (§5). `Exp2` with an integer exponent returned a wrong finite value on underflow in 0.5.2; it now returns `+0` (§5).
  Code that tested for `NaN` after an overflow should test `IsInfinity` or `IsZero`.
- `Pow`, `Cbrt`, `Atanh`, and `Tanh` return an infinity, a zero, `±1`, or a finite value where 0.5.2 returned a NaN (a 0.5.2 NaN or a library NaN, as §2 labels each row), for
  the inputs listed in §2. Code that tested for `NaN` after these calls should test the value instead, for
  example with `IsInfinity` or `IsZero`.
- `Hypot` no longer returns `+inf` when `x² + y²` overflows but the result does not (§5).
- `Sin`, `Cos`, `SinCos`, and `Tan` return library NaN for finite `|x| ≥ 2^112`, and `SinPi`, `CosPi`, `SinCosPi`, and `TanPi` do so where `x · Pi` reaches 2^112. The NaN comes from 0.6.0, where commit `6e0e889` added the guard, so 0.7.1 already returns it. For the first group, 0.5.2 returned finite values, and they were wrong: `Sin(1E+34)` was `−0.9093` (the value is `−0.6861`), `Sin(1E+300)` was `+0` (the sine of a nonzero binary128 value is never zero), and `Tan(1E+34)` was `2.185` (the value is `0.9431`) (§2, §6). For the pi group, 0.5.2 returned the exact values at `1E+34` (`+0` for `SinPi`, `SinCosPi`, and `TanPi`, and `1` for `CosPi`). That is a regression from 0.5.2. Those values were not computed by an exact reduction: 0.5.2's remainder was inexact. Exact reduction modulo 2 for the pi group is planned as the first change in the follow-up PR that adds the CoRDiC kernel, and it restores these exact values.
- An explicit conversion from `BigInteger` to `UInt256` returns the exact value for `2^128 ≤ n < 2^256`,
  where it threw `OverflowException` (§2).
- `Parse` of a number whose exponent is beyond the range of the format returns `±inf` or `±0` (§5).
  An exponent beyond the `int` range is no longer ignored (§5).
- A `checked` conversion from `Float128` to an integer type throws `OverflowException` when the
  value is out of range (§5).
- Code that caught `DivideByZeroException` from dividing a subnormal by zero, or from `%` or
  `Ieee754Remainder` with a nonzero subnormal dividend and a zero divisor, now gets an infinity or a NaN instead (§2).
- `x % ±inf` and `Ieee754Remainder(x, ±inf)` return `x` for finite `x`; 0.7.1 returned NaN. Code that
  checked the remainder for NaN to detect an infinite divisor should test the divisor with `IsInfinity` instead (§5).
- `GetSignificandByteCount()` returns 16, not 14. Buffers sized from the old value are too small; `TryWriteSignificand*` now returns `true` for a destination of 16 bytes or more (§2, byte layout). For a 14- or 15-byte significand destination, `bytesWritten` is now `0` instead of `14`.
- `TryWriteExponent*` and `TryWriteSignificand*` return `false`, with `bytesWritten` set to `0`, for a destination shorter than the byte count. They used to throw `ArgumentOutOfRangeException` for a destination shorter than 2 bytes (exponent) or 14 bytes (significand), so code that caught that exception now gets `false` for those sizes (§2, byte layout).
- `CompareTo` sorts NaN below every value. Code that sorted NaN last must change (§2).
- `IsInteger` returns `false` for `±inf` and NaN (§3).
- `Round` keeps the sign of its input and rounds ties to even (§5).
- `ToString` counts `NumberDecimalDigits` after the decimal point, rounds half to even, and trims trailing zeros in E notation (§5).
- `IsNaN`, `IsInfinity`, `IsZero`, and `IsFinite` keep their contracts. Only the values produced
  by the operations above differ.

## References

- IEEE 754-2019, §6 (infinity, NaNs, and the sign bit) and §9.2 (recommended operations).
- ISO/IEC 9899 (C99), Annex F.9 (special cases of `pow`, `atan2`, and the hyperbolic functions).
