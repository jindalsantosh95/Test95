# Configuration Reference

This document defines how solution-level configuration references should be maintained. Apply it to any project that reads configuration from `appsettings*.json`, environment variables, command-line providers, Key Vault references, or shared configuration helpers.

## Required Sections Per Project

For each changed solution or project, document:

- startup configuration providers and their order,
- source files that define configuration constants,
- required runtime keys,
- optional keys and defaults,
- trigger schedule keys or binding expressions,
- secret ownership and expected environment source,
- operational notes for missing, malformed, or legacy key names.

## Template

### `<ProjectName>` keys

Configuration is loaded through:

- `<provider or extension, for example AddCommandLine(args)>`
- `<provider or extension, for example AddEnvironmentConfiguration()>`

Sources:

- `<ProjectName>/Constants/Environment.cs`
- `<ProjectName>/Constants/Application.cs`
- `<other files that call configuration readers>`

#### Required Runtime Keys

| Key | Used by | Purpose |
|---|---|---|
| `<KeyName>` | `<class, trigger, or registration>` | `<runtime purpose>` |

#### Optional Runtime Keys

| Key | Default | Used by | Purpose |
|---|---|---|---|
| `<KeyName>` | `<default or none>` | `<class or registration>` | `<runtime purpose>` |

#### Trigger Or Binding Keys

| Key token in attribute | Resolved key | Purpose |
|---|---|---|
| `%<Section:Key>%` | `<Section:Key>` | `<timer, queue, Service Bus, or route behavior>` |

## Operational Notes

- Keep key names exactly aligned with code, including any legacy spellings that must remain compatible.
- Keep secrets out of source control; document the key name and expected source, not the secret value.
- Document breaking startup behavior when a key is missing.
- When configuration affects external endpoints, headers, queues, topics, cache, storage, or schedule timing, update the behavior harness or operational tests as appropriate.
