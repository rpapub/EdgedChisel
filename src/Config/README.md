# Config

This folder contains utility code for accessing structured configuration data from `Dictionary<string, object>` sources, as commonly passed in UiPath workflows.

## Components

### `ConfigHelper.cs`
Provides strongly typed accessors for nested config dictionaries, following a two-level hierarchy:

```

{
"MySection": {
"MyKey": "value"
}
}

````

#### Supported accessors:
- `GetString(section, key)`
- `GetInt32(section, key)`
- `GetBool(section, key)`
- `GetTimeSpanFromSeconds(section, key, defaultSeconds)`

All methods validate type and presence, and throw meaningful exceptions on failure.

## Purpose

This helper simplifies input argument handling when using structured config dictionaries in orchestrated or modular UiPath workflows.

## Usage Example

```csharp
string timeoutName = ConfigHelper.GetString(config, "Orchestrator", "TimeoutName");
int retries = ConfigHelper.GetInt32(config, "RetryPolicy", "MaxAttempts");
````

This eliminates repetitive casting and error handling in each workflow or InvokeCode activity.
