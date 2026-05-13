# LVK.FastCdc

[![build](https://github.com/lassevk/LasseVK.FastCdc/actions/workflows/build.yml/badge.svg)](https://github.com/lassevk/LasseVK.FastCdc/actions/workflows/build.yml)
[![codeql](https://github.com/lassevk/LasseVK.FastCdc/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/lassevk/LasseVK.FastCdc/actions/workflows/github-code-scanning/codeql)

This package implements the [FastCdc](https://www.usenix.org/conference/atc16/technical-sessions/presentation/xia)
algorithm for efficient chunking of large datasets.

***Note:*** Portions of this library were added by AI, given that I have an IDE that uses AI-style autocompletion.
There is *however*, no portion of it not vetted by me.

# Discord

You can reach me through my [Discord server](https://discord.gg/xz2N2XZCV5).

## Installation

You can install the package from the command line, using `dotnet`:

```bash
dotnet add package LasseVK.FastCdc
```

Or you can use your favorite IDE which should have a Nuget package manager built in.

## Framework support

The package supports the following .NET versions and standards:

* .NET 8.0 (until november 10, 2026)
* .NET 9.0 (until november 10, 2026)
* .NET 10.0 (until november 14, 2028)

This follows the official supported versions policies from Microsoft:

* [The official .NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy)

*Note:* After support for a .NET version ends, the package will still exist on nuget for use with
that version, but I won't guarantee that updates to that version will be made.

# Usage

You provide one of the following types of datasets to the `Chunker.Chunk` methods:

* `ReadOnlySpan<byte>`
* `byte[]`
* `Stream` (or one of its descendants)

In return, you get a collection of `Chunk` records, each specifying the
offset and length of the chunk.

Optionally you can provide a `ChunkingOptions` options object, specifying how the chunking algorithm should
operate. Specifically the options allows you to control the average, minimum and maximum chunk size.

```csharp
```
