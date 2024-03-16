# QuadrupleLib

QuadrupleLib is a modern implementation of the IEEE 754 `binary128` floating point number type for .NET 7 and above based on the `UInt128` built-in. The goal of this project is to eventually create a fully fleshed out 128-bit floating point arithmetic library that includes all of the bells and whistles one could possibly want. 

### Project TODOs (Completed)

- [x] Adheres to minimum requirements of IEEE 754 specification
- [x] Partially implements .NET 7 `INumber` generic arithmetic interface
- [x] Implements all basic arithmetic operations (`+`, `-`, `*`, `/`, `%`, `++`, `--`)
- [x] Implements all standard rounding functions (`Round`, `Floor`, `Ceiling`)
- [x] Implements basic `ToString` and `Parse`/`TryParse` methods
- [x] Implements conversion methods to & from `binary64` type
- [x] Provides conversion pathways to all standard .NET number types via `binary64`

### Project TODOs (HELP WANTED!!)

- [ ] Unit tests to check for specification coverage
- [ ] Implements `IEEERemainder` as suggested in IEEE 754
- [ ] Implements typical library functions (`Pow`, `Atan2`, `Log`)
- [ ] Supports all rounding modes (atm only implements "ties to even" mode)
- [ ] Supports .NET Core formatting features for `ToString` and `Parse`
- [ ] Implements .NET 7 `IBinaryFloatingPointIeee754` generic arithmetic interface
