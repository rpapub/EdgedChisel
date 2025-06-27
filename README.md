# Edged Chisel

This repository demonstrates how to build an opinionated abstraction layer over UiPath’s vendor-specific capabilities—using mail handling as a case study—to meet the practical needs of automation developers in real enterprise environments.

It combines:

* **Structured, typed data models** (e.g., `GenericMailMessage`)
* **C# source files embedded within UiPath projects** for reliable data transformation and transparent control flow
* **A monorepo layout** that enables simple reuse of `.cs` files across multiple UiPath projects, without requiring libraries or package distribution

This approach emphasizes:

* A clear boundary between vendor surface and automation logic
* Focus on the business process data

Ultimately, it empowers developers to define stable, minimal, and extensible interfaces that reflect business needs—not tool constraints.
