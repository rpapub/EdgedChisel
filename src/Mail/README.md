# Mail

This folder contains mail-related data structures and transformation logic for use in UiPath-based automation projects.

## Components

### `GenericMailMessage.cs`
Defines a reusable, protocol-agnostic mail message model with the following features:

- Strongly typed properties (`Subject`, `Body`, `From`, `To`, `Attachments`, etc.)
- Methods for populating from:
  - `Office365Message` objects (via Microsoft 365 activities)
  - `System.Net.Mail.MailMessage` (e.g. IMAP workflows)
- Attachment extraction and caching
- Checksum generation for duplicate detection
- JSON serialization helpers

## Purpose

This module abstracts raw mail protocol details into a uniform format (`GenericMailMessage`) for reliable downstream processing, routing, and storage. It is designed to support multiple mail sources while maintaining consistent data quality and traceability.

## Usage

This file is intended to be copied into UiPath projects under the `Code/` folder.
It integrates with Studio's C# support (available since v23.10) and requires the appropriate activity packages:

- `UiPath.MicrosoftOffice365` (for Office365Message support)
- `Newtonsoft.Json`

## Related

- See `src/UiPath/MailWrapperDemo/` for a reference implementation.
