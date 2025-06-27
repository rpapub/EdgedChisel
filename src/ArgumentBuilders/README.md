# ArgumentBuilders

This folder contains helper classes and utilities for initializing, populating, and standardizing common input/output arguments used in UiPath workflows.

## Purpose

In UiPath automation, invoking workflows with consistent and well-structured arguments is critical. This module provides code-based scaffolding for:

- Initializing **input arguments** such as request payloads (`Request`)
- Constructing **output arguments** such as processing results (`Result`) and execution status (`Status`)
- Embedding **error handling context** (e.g. retryable vs non-retryable) using predefined structures

## Components

### `Status.cs`
Provides a reusable API for creating `Dictionary<string, object>` outputs representing the status of a workflow step.

Supports:
- Success responses
- Retryable failure (e.g. transient errors)
- Non-retryable failure (e.g. business rule violations)
- Exception wrapping for status generation

All output dictionaries follow a consistent schema, including keys such as:
- `isSuccess`
- `errorMessage`
- `errorType`
- `code`
- `timestamp`
- `durationMs`

## Usage

These builder classes are designed to be copied into the `Code/` folder of any UiPath project.
They work seamlessly with the `InvokeWorkflow` pattern where arguments are passed as dictionaries and interpreted consistently.

## Future Additions

- `RequestBuilder`: for initializing expected input payloads
- `ResultBuilder`: for preparing downstream result dictionaries in a structured format
