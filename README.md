# QuadrupleLib

[![.NET](https://github.com/IsaMorphic/QuadrupleLib/actions/workflows/dotnet.yml/badge.svg)](https://github.com/IsaMorphic/QuadrupleLib/actions/workflows/dotnet.yml)
[![NuGet](https://github.com/IsaMorphic/QuadrupleLib/actions/workflows/nuget.yml/badge.svg)](https://github.com/IsaMorphic/QuadrupleLib/actions/workflows/nuget.yml)

QuadrupleLib is a modern implementation of the IEEE 754 `binary128` floating point number type for .NET 8 and above based on the `UInt128` built-in. The goal of this project is to create a fully fleshed out 128-bit floating point arithmetic library that includes all of the bells and whistles one could possibly want. 

# Main Features

- [x] Adheres to recommended requirements of IEEE 754 specification
- [x] Implements .NET 8 `IBinaryFloatingPointIeee754` generic arithmetic interface
- [x] Implements all basic arithmetic operations (`+`, `-`, `*`, `/`, `%`, `++`, `--`)
- [x] Implements all standard rounding functions (`Round`, `Floor`, `Ceiling`)
- [x] Supports all recommended rounding modes for arithmetic
- [x] Implements basic `ToString` and `Parse`/`TryParse` methods
- [x] Supports .NET Core formatting features for `ToString` and `Parse`
- [x] Implements conversion methods to & from all standard number types (except `decimal`)
- [x] Implements `IEEERemainder` as suggested in IEEE 754
- [x] Implements typical library functions (`Pow`, `Atan2`, `Log`)
- [x] Unit tests to check for specification coverage & overall correctness

# Using QuadrupleLib

QuadrupleLib is available as a regularly updated [NuGet package](https://www.nuget.org/packages/QuadrupleLib), published via my GitHub Actions workflow. Alternatively, see the [Releases](https://github.com/IsaMorphic/QuadrupleLib/releases) page for a downloadable version you may use in your local or private feeds. 

## Basic Usage

To use QuadrupleLib in your project, simply add the `PackageReference` to your `.csproj` file and add the following `using` statement to the top of any single file in your project:

```csharp
global using Float128 = QuadrupleLib.Float128<QuadrupleLib.Accelerators.DefaultAccelerator>;
```

The `Float128` type as defined above includes a full implementation of the .NET 8 `IBinaryFloatingPointIeee754<T>` generic arithmetic interface. You may use it as thus either generically or concretely. Generally, anywhere you're using `double` or `float` arithmetic, you can use `Float128` as a drop-in replacement and everything will work as expected.

## Defining `Float128` Constants

For defining high-precision constants, it is highly recommended that `Float128.Parse` is used with a given `const string`. While literal `double` and `long` values can be implicitly converted to `Float128`, you will not be able to specify the maximum amount of significant digits that you would otherwise be able to using the `Parse` method. See the example below for reference:

```csharp
private const string PI = "3.1415926535897932384626433832795028";

private static readonly Float128 _pi = Float128.Parse(PI);
public static Float128 Pi => _pi;
```

## Notes on Hardware Acceleration

`Float128<TAccelerator>` is a generic type offered by the library which can accept one of two "accelerators" that are built-in: `DefaultAccelerator` and `SoftwareAccelerator`. The former uses typical hardware accelerated intrinsics for 128-bit multiplication and division operations, while the latter provides a custom, software defined version of the same thing. In almost all cases, you'll want to use `DefaultAccelerator`, unless you are running your code on a platform that does not include intrinsics for 128-bit arithmetic. In those cases, `SoftwareAccelerator` provides a faster alternative that is widely compatible. The most notable example of such a platform is when using QuadrupleLib in conjunction with [ILGPU](https://ilgpu.net), a JIT compiler for running .NET code on the GPU. 

# Development Guide

Interested in tinkering with the source code? Feel free to fork this repo and/or clone it as a submodule in your project:

```bash
git submodule add "https://github.com/IsaMorphic/QuadrupleLib.git" "external/QuadrupleLib"
git commit -m "Feature: add QuadrupleLib as submodule"
cd external/QuadrupleLib
```

Then, to build the source code, make sure the .NET 10 SDK is installed on your machine, and run:

```bash
dotnet build --no-incremental
```

Next, to run unit tests, use the following command:

```bash
dotnet test --no-build
```

To build the NuGet package, use the following command:

```bash
dotnet pack --no-build --output build/
```

# Contributing

Before contributing any changes to the project, make sure that all the standard unit tests are passing. If your changes are a work-in-progress, please mark those PRs as drafts. I will not accept any changes which do not pass all existing unit tests; any new features should include new tests to cover them. Finally, do NOT contribute code written by Copilot or other LLMs. QuadrupleLib is written by humans, for humans. 
