# QuadrupleLib

QuadrupleLib is a modern implementation of the IEEE 754 `binary128` floating point number type for .NET 8 and above based on the `UInt128` built-in. The goal of this project is to create a fully fleshed out 128-bit floating point arithmetic library that includes all of the bells and whistles one could possibly want. 

### Project TODOs (Completed)

- [x] Adheres to recommended requirements of IEEE 754 specification
- [x] Implements .NET 8 `IBinaryFloatingPointIeee754` generic arithmetic interface
- [x] Implements all basic arithmetic operations (`+`, `-`, `*`, `/`, `%`, `++`, `--`)
- [x] Implements all standard rounding functions (`Round`, `Floor`, `Ceiling`)
- [x] Supports all recommended rounding modes for arithmetic
- [x] Implements basic `ToString` and `Parse`/`TryParse` methods
- [x] Supports .NET Core formatting features for `ToString` and `Parse`
- [x] Implements conversion methods to & from all standard number types
- [x] Implements `IEEERemainder` as suggested in IEEE 754
- [x] Implements typical library functions (`Pow`, `Atan2`, `Log`)

### Project TODOs (WIP)

- [x] Unit tests to check for specification coverage (#11)
