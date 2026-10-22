# Warning Baseline (VAA.EAPI.Automation)

Established from a clean `dotnet build VAA.EAPI.Automation.sln -c Release` on 2026-05-18,
and updated as backlog codes are burned down. This file is the reference point for the
**analyzer ratchet** (Thread 1b in `HARNESS_ENGINEERING.md`).

## How the ratchet uses this file

`Directory.Build.props` sets `TreatWarningsAsErrors=true`. Of the 43 codes catalogued
below, **41** are re-exempted via `WarningsNotAsErrors`, so the legacy backlog keeps
building. Any warning whose code is **not** on that list — i.e. a regression an agent or
contributor just introduced — becomes a hard build error.

The other two sit outside `WarningsNotAsErrors`: `NETSDK1206` and `MSB3245` are
MSBuild/SDK warnings that the compiler's `TreatWarningsAsErrors` cannot turn into errors.

Burning the backlog down: when a code's count reaches zero, remove it from both this
file and the `WarningsNotAsErrors` list. From then on it is gated.

**NU1903 is never exempted.** The package-vulnerability warning is deliberately kept out
of `WarningsNotAsErrors`, so a newly introduced vulnerable package always fails the
build. The two advisories present at the 2026-05-18 baseline —
`System.Security.Cryptography.Xml`, CVE-2026-33116 and CVE-2026-26171 — were resolved by
upgrading the `Microsoft.Extensions.*` / `System.*` runtime band. `System.Security.Cryptography.Xml`
is currently pinned to 10.0.10, so `NU1903`
now fires zero times and is not listed below.

## Backlog — 43 codes, 23,882 warnings total

### Nullable reference types (~18,900) — the dominant backlog

| Code | Count | Meaning |
|---|---|---|
| CS8602 | 10,907 | Dereference of a possibly-null reference |
| CS8604 | 4,159 | Possible null reference argument |
| CS8600 | 2,628 | Converting null/possible-null to non-nullable |
| CS8618 | 787 | Non-nullable field/property uninitialised |
| CS8632 | 333 | Nullable annotation in a `#nullable disable` context |
| CS8601 | 41 | Possible null reference assignment |
| CS8629 / CS8603 / CS8625 / CS8620 / CS8619 / CS8605 | 9 / 9 / 6 / 2 / 2 / 1 | Other nullable-flow warnings |
| CS8509 | 1 | Switch expression not exhaustive |

Driven by the projects already on `Nullable=enable`. Reducing this is entangled with the
`Nullable` migration noted in `HARNESS_ENGINEERING.md`.

### xUnit analyzer (~4,300)

| Code | Count | Meaning |
|---|---|---|
| xUnit1031 | 3,960 | Test method uses a blocking task call (deadlock risk) |
| xUnit1026 | 217 | Theory method parameter never used |
| xUnit2010 | 52 | Do not use `Assert.Equal()` for string containment |
| xUnit2002 | 36 | Do not use `Assert.NotNull` on a value type |
| xUnit1048 / xUnit1025 / xUnit1013 / xUnit2020 / xUnit2009 / xUnit2017 / xUnit1008 | 13 / 10 / 10 / 6 / 3 / 1 / 1 | Other xUnit usage/assertion warnings |

### Other compiler warnings (~550)

| Code | Count | Meaning |
|---|---|---|
| CS0618 | 396 | Use of `[Obsolete]` member |
| CS0168 | 65 | Variable declared but never used |
| CS0219 | 37 | Variable assigned but never used |
| CS1998 | 12 | `async` method lacks `await` |
| CS0472 / CS0169 / CS0414 / CS4014 / CS0649 / CS1717 | 9 / 9 / 5 / 4 / 4 / 3 | Misc |
| CS1718 / CS1030 / CS0436 / CS0252 / CS0162 | 1 each | Misc |

### NuGet, SDK and analyzer (~140)

| Code | Count | Meaning |
|---|---|---|
| NU1701 | 108 | Package restored using a different target framework |
| NETSDK1206 | 25 | Version-specific runtime identifier (`win7-x64`) |
| CA1416 | 4 | Platform-compatibility (Windows-only API on a portable target) |
| MSB3245 | 2 | Could not resolve a reference (`xunit.runner.reporters.netcoreapp10`) |

## Notable items beyond the ratchet

- **MSB3245** — an unresolved `xunit.runner.reporters.netcoreapp10` reference in
  `Core` / `TestRunner.Service`. A genuine project-config bug hiding in the noise.
