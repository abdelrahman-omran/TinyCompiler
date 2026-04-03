# TinyCompiler

[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

A compiler implementation for the **Tiny** programming language, built in C#. Covers the foundational frontend phases of compilation: lexical analysis and syntax parsing.

## Overview

The Tiny language is a small, educational programming language commonly used in compiler design courses. This project implements:

- **Scanner (Lexical Analyzer)** — tokenizes raw Tiny source code into a stream of classified tokens
- **Parser** — applies a context-free grammar to the token stream and builds an Abstract Syntax Tree (AST)
- **Error Handling** — reports lexical and syntactic errors with line and column information

## Project Structure

```
├── Parser-Template - Studs Versions/   # Parser template and student implementations
├── LICENSE.txt
└── README.md
```

## Getting Started

### Prerequisites

- .NET SDK (C# project)
- Visual Studio or any C# compatible IDE

### Run

Open the solution in Visual Studio, build, and run. You can provide Tiny source code as input to see the token stream and generated AST.

## The Tiny Language

Tiny supports basic programming constructs including variable assignments, arithmetic expressions, conditionals (`if`/`then`/`else`/`end`), and `repeat`/`until` loops. It uses a simple, human-readable syntax designed for compiler education.

## Compiler Phases Implemented

| Phase | Description |
|-------|-------------|
| Scanner | Converts source text into tokens |
| Parser | Builds an AST from the token stream |
| Error Reporting | Locates and reports lexical/syntactic errors |

## License

Apache 2.0 — see [LICENSE.txt](LICENSE.txt) for details.
