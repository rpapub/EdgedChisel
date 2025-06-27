# Edged Chisel

This repository demonstrates how to build an opinionated abstraction layer over UiPath’s vendor-specific capabilities—using mail handling as a case study—to meet the practical needs of automation developers in real enterprise environments.

It combines:

- **Structured, typed data models** (e.g., `GenericMailMessage`)
- **C# source files embedded within UiPath projects** for reliable data transformation, argument preparation, and output structuring
- **A monorepo layout** enabling reuse of `.cs` files across multiple UiPath projects without submodules or packaging

This approach emphasizes:

- A clear boundary between vendor-provided APIs and automation logic
- Clean handling of `Request`, `Status`, and `Result` arguments
- Focus on business-process-relevant data quality

## Structure

```

src/
├── Mail/              # GenericMailMessage, mail transformers
├── ArgumentBuilders/  # StatusBuilder, ResultBuilder, retry logic helpers
├── Config/            # Typed accessors for Dictionary-based config
├── UiPath/
│   └── MailWrapperDemo/  # Reference project importing and using shared code
tools/
└── sync-shared-code.ps1  # Copies .cs files into UiPath projects

```


This hybrid pattern brings structure and reuse to UiPath workflows—without sacrificing the low-code developer experience.
