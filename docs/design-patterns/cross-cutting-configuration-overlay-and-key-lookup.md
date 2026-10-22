# Configuration Overlay And Key Lookup

**Pattern group:** Cross-Cutting

**Status:** Recommended

**Last reviewed:** 2026-06-01

**Owner:** EAPI Technical Leads

## Intent

Use EAPI configuration helpers and environment overlays to resolve settings consistently across local development, Function runtime, shared infrastructure, and service-specific code.

This pattern prevents hardcoded environment values and gives services a common way to read keys, secrets references, endpoints, feature flags, and operational settings.

## Use When

- Reading service endpoints, app IDs, scopes, cache policies, feature flags, or connection references.
- Supporting local execution with environment configuration.
- Adding a new configurable value to a service.

## Do Not Use When

- A value is a compile-time constant with no environment variance.
- The value is a secret that should be retrieved through Key Vault or a managed secret provider.
- The setting belongs to another service boundary.

## Pattern Shape

1. Define the configuration key in a constants class.
2. Add environment-specific values to the appropriate deployment/configuration mechanism.
3. Use `ConfigReader.Get` or `ConfigReader.Try` at the edge of the component that needs it.
4. Use environment configuration overlay for local development where applicable.

## Implementation Guidance

- Centralise key names in `Constants/Environment.cs` or the established constants file.
- Do not hardcode endpoints, tokens, connection strings, or environment-specific values.
- Use typed `ConfigReader` access when the expected type is known.
- Keep configuration reads close to infrastructure or startup code where possible.

## Code References

- [EnvironmentConfigurationExtensions.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Infrastructure/Configuration/EnvironmentConfigurationExtensions.cs:9): local environment configuration overlay.
- [EnvironmentConfigurationProvider.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Infrastructure/Configuration/EnvironmentConfigurationProvider.cs:18): provider backing local configuration load.
- [ConfigReader.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Common/Helpers/ConfigReader.cs:21): shared configuration access helper.

## Related Principles And Standards

- [Infrastructure Principles](../principles/infrastructure-principles.md)
- [Security Principles](../principles/security-principles.md)
- [Coding Standards](../standards/coding-standards.md)

## AI Agent Checklist

- New setting has a central key constant.
- No environment-specific value is hardcoded.
- Secrets are not committed.
- Deployment/configuration files are updated when runtime values change.

## Change History

| Date | Change | Author |
| --- | --- | --- |
| 2026-06-01 | Initial draft. | Seb Harvey |
| 2026-06-01 | Approved. | EAPI Technical Leads |
