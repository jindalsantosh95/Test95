#!/usr/bin/env python3
"""Scaffold a VAA EAPI Azure Functions microservice.

This script is intentionally deterministic. Given the same repository state and
the same domain/name input, it writes the same project, test project,
deployment project, and pipeline workflow paths.
"""

from __future__ import annotations

import argparse
import json
import os
import re
import shutil
import subprocess
import sys
import uuid
from dataclasses import dataclass
from pathlib import Path
from string import Template
from textwrap import dedent


PREFIX = "VAA.EAPI.Microservices"
DEPLOYMENTS_ROOT = "VAA.EAPI.Deployments"
PIPELINES_ROOT = "VAA.EAPI.Pipelines"
DEFAULT_ENVIRONMENTS = ("dev", "sit", "uat", "prod")


@dataclass(frozen=True)
class ServiceName:
    domain_segments: tuple[str, ...]
    name_segments: tuple[str, ...]

    @property
    def suffix(self) -> str:
        return ".".join((*self.domain_segments, *self.name_segments))

    @property
    def project_name(self) -> str:
        return f"{PREFIX}.{self.suffix}"

    @property
    def namespace(self) -> str:
        return self.project_name

    @property
    def test_project_name(self) -> str:
        return f"{self.project_name}.Tests"

    @property
    def deployment_project_name(self) -> str:
        return f"{self.project_name}.Deployment"

    @property
    def application_name(self) -> str:
        return self.suffix

    @property
    def domain_name(self) -> str:
        return ".".join(self.domain_segments)

    @property
    def display_name(self) -> str:
        return " ".join((*self.domain_segments, *self.name_segments))

    @property
    def tag_name(self) -> str:
        return " ".join((*self.domain_segments, *self.name_segments))

    @property
    def handler_feature(self) -> str:
        return "HelloWorld"

    @property
    def slug(self) -> str:
        return "".join(segment.lower() for segment in (*self.domain_segments, *self.name_segments))

    @property
    def short_slug(self) -> str:
        slug = re.sub(r"[^a-z0-9]", "", self.slug)
        return slug[:18] or "microservice"

    @property
    def apim_url_name(self) -> str:
        if not self.name_segments:
            return self.slug
        first = self.name_segments[0][0].lower() + self.name_segments[0][1:]
        rest = "".join(self.name_segments[1:])
        return f"{first}{rest}"


def main() -> int:
    args = parse_args()
    repo_root = find_repo_root(Path(args.repo).resolve())
    if repo_root is None:
        print("Could not locate the EAPI repo root. Pass --repo /path/to/eapi.", file=sys.stderr)
        return 2

    known_domains = discover_known_domains(repo_root)
    try:
        service = parse_service_name(args, known_domains)
    except ValueError as exc:
        print(str(exc), file=sys.stderr)
        return 2

    plan = build_plan(repo_root, service, args)
    if args.dry_run:
        print_plan(plan, dry_run=True)
        return 0

    existing = [path for path in plan.files if path.exists()]
    if existing and not args.force:
        print("Refusing to overwrite existing files. Re-run with --force if this is intentional.", file=sys.stderr)
        for path in existing:
            print(f"  {path}", file=sys.stderr)
        return 1

    for path, content in plan.files.items():
        write_text(path, content, force=args.force)

    ensure_directories(plan.directories)

    sln_messages: list[str] = []
    if not args.skip_sln:
        sln_messages.extend(add_to_solutions(repo_root, service, plan))

    print_plan(plan, dry_run=False)
    for message in sln_messages:
        print(message)

    return 0


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Create a VAA EAPI bare-bones Azure Functions microservice scaffold.",
    )
    parser.add_argument(
        "service",
        nargs="*",
        help="Domain and service name, for example: Retail Pricing, FlightOperations Crew, or Retail.Holidays Store Appointment.",
    )
    parser.add_argument("--domain", help="Explicit domain, for example Retail or Retail.Holidays.")
    parser.add_argument("--name", help="Explicit microservice name, for example Order Search.")
    parser.add_argument("--repo", default=os.getcwd(), help="Path inside the EAPI repo. Defaults to the current directory.")
    parser.add_argument("--force", action="store_true", help="Overwrite generated files if they already exist.")
    parser.add_argument("--dry-run", action="store_true", help="Print the generated paths without writing files.")
    parser.add_argument("--skip-sln", action="store_true", help="Do not run dotnet sln add.")
    parser.add_argument("--skip-deployment", action="store_true", help="Do not generate the deployment project.")
    parser.add_argument("--skip-pipeline", action="store_true", help="Do not generate the Azure DevOps pipeline YAML.")
    return parser.parse_args()


def find_repo_root(path: Path) -> Path | None:
    for candidate in (path, *path.parents):
        if (candidate / PREFIX).is_dir() and (candidate / DEPLOYMENTS_ROOT).is_dir():
            return candidate
    return None


def discover_known_domains(repo_root: Path) -> list[tuple[str, ...]]:
    solution_root = repo_root / PREFIX
    domains: set[tuple[str, ...]] = set()
    for sln in solution_root.glob(f"{PREFIX}.*.sln"):
        suffix = sln.stem.removeprefix(f"{PREFIX}.")
        if suffix and suffix.lower() not in {"sln"}:
            domains.add(tuple(split_to_segments(suffix)))

    for project_dir in solution_root.glob(f"{PREFIX}.*"):
        if not project_dir.is_dir():
            continue
        suffix = project_dir.name.removeprefix(f"{PREFIX}.")
        segments = tuple(split_to_segments(suffix))
        if segments:
            domains.add((segments[0],))

    return sorted(domains, key=lambda item: (-len("".join(item)), item))


def parse_service_name(args: argparse.Namespace, known_domains: list[tuple[str, ...]]) -> ServiceName:
    if args.domain and args.name:
        domain_segments = tuple(split_to_segments(args.domain))
        name_segments = tuple(split_to_segments(args.name))
        return validate_service(ServiceName(domain_segments, name_segments))

    raw = " ".join(args.service).strip()
    if not raw:
        raise ValueError("Pass a domain and microservice name, for example: /new-microservice Retail Pricing.")

    separated = split_explicit_domain_name(raw)
    if separated is not None:
        domain_raw, name_raw = separated
        return validate_service(ServiceName(tuple(split_to_segments(domain_raw)), tuple(split_to_segments(name_raw))))

    tokens = re.findall(r"[A-Za-z0-9]+", raw)
    if len(tokens) < 2:
        raise ValueError("Pass both a domain and a microservice name, for example: /new-microservice Retail Pricing.")

    normalized_tokens = [normalize_for_match(token) for token in tokens]
    for domain in known_domains:
        flattened = [normalize_for_match(part) for part in domain]
        candidate = "".join(flattened)
        for end in range(1, min(len(tokens), len(domain) + 2) + 1):
            if "".join(normalized_tokens[:end]) == candidate and end < len(tokens):
                return validate_service(ServiceName(domain, tuple(pascal_segment(token) for token in tokens[end:])))

    return validate_service(ServiceName((pascal_segment(tokens[0]),), tuple(pascal_segment(token) for token in tokens[1:])))


def split_explicit_domain_name(raw: str) -> tuple[str, str] | None:
    for separator in (" | ", " : ", " / ", " - ", ","):
        if separator in raw:
            left, right = raw.split(separator, 1)
            if left.strip() and right.strip():
                return left.strip(), right.strip()
    return None


def split_to_segments(raw: str) -> list[str]:
    parts = re.split(r"[./\\:_\-\s]+", raw.strip())
    return [pascal_segment(part) for part in parts if part.strip()]


def pascal_segment(raw: str) -> str:
    pieces = re.findall(r"[A-Za-z0-9]+", raw)
    if not pieces:
        raise ValueError(f"Invalid name segment: {raw!r}")

    words: list[str] = []
    for piece in pieces:
        if piece.isupper() or piece.isdigit():
            words.append(piece)
        elif len(piece) > 1 and any(char.isupper() for char in piece[1:]):
            words.append(piece[0].upper() + piece[1:])
        else:
            words.append(piece[:1].upper() + piece[1:].lower())
    return "".join(words)


def normalize_for_match(value: str) -> str:
    return re.sub(r"[^a-z0-9]", "", value.lower())


def validate_service(service: ServiceName) -> ServiceName:
    if not service.domain_segments:
        raise ValueError("The namespace must include a domain segment.")
    if not service.name_segments:
        raise ValueError("The namespace must include a microservice name segment.")
    for segment in (*service.domain_segments, *service.name_segments):
        if not re.fullmatch(r"[A-Za-z][A-Za-z0-9]*", segment):
            raise ValueError(f"Invalid namespace segment: {segment!r}")
    return service


@dataclass
class GenerationPlan:
    repo_root: Path
    service: ServiceName
    files: dict[Path, str]
    directories: tuple[Path, ...]
    project_dir: Path
    test_project_dir: Path
    deployment_dir: Path | None
    pipeline_file: Path | None
    microservice_solution: Path
    deployment_solution: Path | None


def build_plan(repo_root: Path, service: ServiceName, args: argparse.Namespace) -> GenerationPlan:
    project_dir = repo_root / PREFIX / service.project_name
    test_project_dir = repo_root / PREFIX / service.test_project_name
    deployment_dir = None if args.skip_deployment else repo_root / DEPLOYMENTS_ROOT / service.deployment_project_name
    pipeline_file = None if args.skip_pipeline else repo_root / PIPELINES_ROOT / f"{service.application_name}.Microservice.CICD.Pipeline.yaml"
    solution = pick_microservice_solution(repo_root, service)
    deployment_solution = None if args.skip_deployment else repo_root / DEPLOYMENTS_ROOT / "VAA.EAPI.Deployments.sln"

    files: dict[Path, str] = {}
    add_project_files(files, project_dir, service)
    add_test_files(files, test_project_dir, service)
    if deployment_dir is not None:
        add_deployment_files(files, deployment_dir, service)
    if pipeline_file is not None:
        files[pipeline_file] = render_pipeline_yaml(service)

    directories = (
        project_dir / "Triggers" / "Timer",
        project_dir / "Triggers" / "ServiceBus",
        test_project_dir / ".mocks",
    )

    return GenerationPlan(
        repo_root=repo_root,
        service=service,
        files=files,
        directories=directories,
        project_dir=project_dir,
        test_project_dir=test_project_dir,
        deployment_dir=deployment_dir,
        pipeline_file=pipeline_file,
        microservice_solution=solution,
        deployment_solution=deployment_solution,
    )


def pick_microservice_solution(repo_root: Path, service: ServiceName) -> Path:
    candidates = []
    for index in range(len(service.domain_segments), 0, -1):
        suffix = ".".join(service.domain_segments[:index])
        candidates.append(repo_root / PREFIX / f"{PREFIX}.{suffix}.sln")
    candidates.append(repo_root / PREFIX / f"{PREFIX}.sln")
    for candidate in candidates:
        if candidate.exists():
            return candidate
    return candidates[-1]


def add_project_files(files: dict[Path, str], project_dir: Path, service: ServiceName) -> None:
    namespace = service.namespace
    substitutions = {
        "namespace": namespace,
        "project_name": service.project_name,
        "display_name": service.display_name,
        "deployment_project_name": service.deployment_project_name,
        "tag_name": service.tag_name,
        "domain_name": service.domain_name,
        "application_name": service.application_name,
        "short_slug": service.short_slug,
        "openapi_description": f"This API provides starter endpoints for the {service.display_name} microservice.",
    }

    files[project_dir / f"{service.project_name}.csproj"] = substitute(PROJECT_CSPROJ, substitutions)
    files[project_dir / "Program.cs"] = substitute(PROGRAM_CS, substitutions)
    files[project_dir / "host.json"] = HOST_JSON
    files[project_dir / "local.settings.json"] = substitute(LOCAL_SETTINGS_JSON, substitutions)
    files[project_dir / "README.md"] = substitute(PROJECT_README, substitutions)
    files[project_dir / "Constants" / "Application.cs"] = substitute(APPLICATION_CONSTANTS_CS, substitutions)
    files[project_dir / "Constants" / "Environment.cs"] = substitute(ENVIRONMENT_CONSTANTS_CS, substitutions)
    files[project_dir / "Domain" / "HelloWorld" / "HelloWorldRequest.cs"] = substitute(HELLO_WORLD_REQUEST_CS, substitutions)
    files[project_dir / "Domain" / "HelloWorld" / "HelloWorldResponse.cs"] = substitute(HELLO_WORLD_RESPONSE_CS, substitutions)
    files[project_dir / "Application" / "HelloWorld" / "GetHelloWorldQuery.cs"] = substitute(GET_HELLO_WORLD_QUERY_CS, substitutions)
    files[project_dir / "Application" / "HelloWorld" / "GetHelloWorldQueryHandler.cs"] = substitute(GET_HELLO_WORLD_QUERY_HANDLER_CS, substitutions)
    files[project_dir / "Interfaces" / "IGetHelloWorldQueryHandler.cs"] = substitute(I_GET_HELLO_WORLD_QUERY_HANDLER_CS, substitutions)
    files[project_dir / "Interfaces" / "IHelloWorldMessageService.cs"] = substitute(I_HELLO_WORLD_MESSAGE_SERVICE_CS, substitutions)
    files[project_dir / "Services" / "HelloWorldMessageService.cs"] = substitute(HELLO_WORLD_MESSAGE_SERVICE_CS, substitutions)
    files[project_dir / "Triggers" / "Http" / "HelloWorldController.cs"] = substitute(HELLO_WORLD_CONTROLLER_CS, substitutions)
    files[project_dir / "Triggers" / "Http" / "HealthController.cs"] = substitute(HEALTH_CONTROLLER_CS, substitutions)
    files[project_dir / "Triggers" / "Timer" / ".gitkeep"] = "Timer trigger implementations belong here.\n"
    files[project_dir / "Triggers" / "ServiceBus" / ".gitkeep"] = "Service Bus trigger implementations belong here.\n"
    files[project_dir / "ErrorCodes" / "ErrorCodes.json"] = ERROR_CODES_JSON


def add_test_files(files: dict[Path, str], test_project_dir: Path, service: ServiceName) -> None:
    substitutions = {
        "namespace": service.namespace,
        "test_namespace": service.test_project_name,
        "project_name": service.project_name,
        "test_project_name": service.test_project_name,
    }

    files[test_project_dir / f"{service.test_project_name}.csproj"] = substitute(TEST_CSPROJ, substitutions)
    files[test_project_dir / "Globals.cs"] = GLOBALS_CS
    files[test_project_dir / "GetHelloWorldQueryHandlerTests.cs"] = substitute(HANDLER_TESTS_CS, substitutions)
    files[test_project_dir / ".mocks" / ".gitkeep"] = "Mock HTTP payloads can be stored here when the service adds downstream integrations.\n"


def add_deployment_files(files: dict[Path, str], deployment_dir: Path, service: ServiceName) -> None:
    substitutions = {
        "deployment_project_name": service.deployment_project_name,
        "project_guid": str(uuid.uuid5(uuid.NAMESPACE_DNS, service.deployment_project_name)),
    }

    files[deployment_dir / f"{service.deployment_project_name}.deployproj"] = substitute(DEPLOYPROJ, substitutions)
    files[deployment_dir / "Deployment.targets"] = DEPLOYMENT_TARGETS
    files[deployment_dir / "Deploy-AzureResourceGroup.ps1"] = DEPLOY_AZURE_RESOURCE_GROUP_PS1
    files[deployment_dir / "Template" / "azuredeploy.json"] = json_dumps(arm_template(include_slot=False))
    files[deployment_dir / "Template" / "azuredeploy_slot.json"] = json_dumps(arm_template(include_slot=True))
    files[deployment_dir / "Template" / "azuredeploy_apim.json"] = json_dumps(apim_template())

    for environment in DEFAULT_ENVIRONMENTS:
        files[deployment_dir / "Parameters" / f"{environment}-azuredeploy.parameters.json"] = json_dumps(
            parameter_file(service, environment)
        )


def substitute(template: str, values: dict[str, str]) -> str:
    return Template(dedent(template).lstrip()).substitute(values)


def json_dumps(value: object) -> str:
    return json.dumps(value, indent=2) + "\n"


def write_text(path: Path, content: str, force: bool) -> None:
    if path.exists() and not force:
        raise FileExistsError(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(content, encoding="utf-8")


def ensure_directories(paths: tuple[Path, ...]) -> None:
    for path in paths:
        path.mkdir(parents=True, exist_ok=True)


def add_to_solutions(repo_root: Path, service: ServiceName, plan: GenerationPlan) -> list[str]:
    messages: list[str] = []
    dotnet = shutil.which("dotnet")
    if dotnet is None:
        return ["dotnet was not found; generated files were not added to solution files."]

    projects = [
        plan.project_dir / f"{service.project_name}.csproj",
        plan.test_project_dir / f"{service.test_project_name}.csproj",
    ]
    for project in projects:
        messages.append(run_sln_add(dotnet, plan.microservice_solution, project, repo_root))

    if plan.deployment_solution and plan.deployment_dir:
        deployproj = plan.deployment_dir / f"{service.deployment_project_name}.deployproj"
        messages.append(run_sln_add(dotnet, plan.deployment_solution, deployproj, repo_root))

    return messages


def run_sln_add(dotnet: str, solution: Path, project: Path, cwd: Path) -> str:
    if not solution.exists():
        return f"Skipped solution add because {solution} does not exist."
    command = [dotnet, "sln", str(solution), "add", str(project)]
    completed = subprocess.run(command, cwd=cwd, text=True, capture_output=True, check=False)
    output = (completed.stdout or completed.stderr).strip()
    if completed.returncode == 0:
        return output or f"Added {project.name} to {solution.name}."
    return f"Could not add {project.name} to {solution.name}: {output}"


def print_plan(plan: GenerationPlan, dry_run: bool) -> None:
    action = "Dry run" if dry_run else "Created"
    print(f"{action} scaffold for {plan.service.project_name}")
    print(f"Namespace: {plan.service.namespace}")
    print(f"Microservice project: {plan.project_dir}")
    print(f"Test project: {plan.test_project_dir}")
    if plan.deployment_dir:
        print(f"Deployment project: {plan.deployment_dir}")
    if plan.pipeline_file:
        print(f"Pipeline workflow: {plan.pipeline_file}")
    print(f"Solution: {plan.microservice_solution}")
    print("Next verification commands:")
    print(f"  dotnet build {plan.microservice_solution} -c Release")
    print(f"  dotnet test {plan.microservice_solution} -c Release")
    print(f"  scripts/verify.ps1 -ProjectPath {plan.project_dir / (plan.service.project_name + '.csproj')} -SkipBuild")


PROJECT_CSPROJ = """
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <AzureFunctionsVersion>v4</AzureFunctionsVersion>
    <OutputType>Exe</OutputType>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <Content Include="local.settings.json" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.ApplicationInsights.WorkerService" Version="2.23.0" />
    <PackageReference Include="Microsoft.Azure.Functions.Worker" Version="2.50.0" />
    <PackageReference Include="Microsoft.Azure.Functions.Worker.ApplicationInsights" Version="2.0.0" />
    <PackageReference Include="Microsoft.Azure.Functions.Worker.Extensions.Http" Version="3.3.0" />
    <PackageReference Include="Microsoft.Azure.Functions.Worker.Extensions.OpenApi" Version="1.6.0" />
    <PackageReference Include="Microsoft.Azure.Functions.Worker.Extensions.Warmup" Version="4.0.2" />
    <PackageReference Include="Microsoft.Azure.Functions.Worker.Sdk" Version="2.0.5" OutputItemType="Analyzer" />
    <PackageReference Include="Microsoft.Extensions.Diagnostics.HealthChecks" Version="10.0.12" />
    <PackageReference Include="Microsoft.Extensions.Http" Version="10.0.12" />
  </ItemGroup>

  <ItemGroup>
    <None Update="ErrorCodes\\ErrorCodes.json">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </None>
    <None Update="local.settings.json">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
      <CopyToPublishDirectory>Never</CopyToPublishDirectory>
    </None>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\\VAA.EAPI.Infrastructure\\VAA.EAPI.Infrastructure.csproj" />
  </ItemGroup>
</Project>
"""


PROGRAM_CS = """
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using $namespace.Application.HelloWorld;
using $namespace.Interfaces;
using $namespace.Services;

namespace $namespace;

public sealed class Program
{
    public static Task Main(string[] args)
    {
        var host = new HostBuilder()
            .ConfigureAppConfiguration(builder =>
            {
                builder.AddCommandLine(args);
                builder.AddEnvironmentConfiguration();
            })
            .ConfigureFunctionsWorkerDefaults(worker =>
            {
                worker.AddDefaultMiddleware();
            })
            .ConfigureOpenApi()
            .ConfigureServices(services =>
            {
                services.AddDefaultServices();
                services.AddHttpClient();

                services.AddSingleton<IHttpConnector, HttpConnector>();
                services.AddSingleton<IHelloWorldMessageService, HelloWorldMessageService>();
                services.AddSingleton<IGetHelloWorldQueryHandler, GetHelloWorldQueryHandler>();
            })
            .Build();

        return host.RunAsync();
    }
}
"""


HOST_JSON = """{
  "version": "2.0",
  "extensions": {
    "http": {
      "routePrefix": "api"
    }
  },
  "logging": {
    "applicationInsights": {
      "samplingSettings": {
        "isEnabled": true,
        "excludedTypes": "Request"
      },
      "enableLiveMetricsFilters": true
    }
  }
}
"""


LOCAL_SETTINGS_JSON = """
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "OpenApi__DocTitle": "Enterprise API - $display_name Microservice",
    "OpenApi__Description": "$openapi_description",
    "OpenApi__DocVersion": "1.0.0",
    "OpenApi__Version": "v3",
    "OpenApi__ForceHttps": "true",
    "OpenApi__ForceHttp": "false",
    "OpenApi__HostNames": "http://localhost:7071",
    "OpenApi__Scope": "TODO-OPENAPI-SCOPE",
    "OpenApi__TokenUrl": "TODO-OPENAPI-TOKEN-URL"
  }
}
"""


PROJECT_README = """
# $project_name

Bare-bones Azure Functions isolated worker microservice for $display_name.

## Structure

- `Application/HelloWorld`: request model and handler.
- `Domain/HelloWorld`: request and response DTOs documented for OpenAPI generation.
- `Interfaces`: handler and service contracts.
- `Services`: service implementations.
- `Triggers/Http`: HTTP trigger endpoints with OpenAPI attributes.
- `Triggers/Timer`: reserved for timer triggers.
- `Triggers/ServiceBus`: reserved for Service Bus triggers.
- `ErrorCodes`: JSON error payloads copied to output.

## Endpoints

- `GET /api/hello?name=Seb`
- `POST /api/hello`
- `GET /api/heartbeat`
- `GET /api/swagger.json`
- `GET /api/swagger/ui`

## Startup Baseline

`Program.cs` registers `AddDefaultServices()`, `AddHttpClient()`, `IHttpConnector`, health checks through the shared baseline, Application Insights worker telemetry through the shared baseline, response compression through the shared baseline, and the hello-world handler/service contracts.

## Local Run

```bash
dotnet restore $project_name.csproj
dotnet build $project_name.csproj -c Release
func start
```

Review deployment parameter placeholders under `../$deployment_project_name/Parameters` before deploying. They intentionally contain no secrets.
"""


APPLICATION_CONSTANTS_CS = """
namespace $namespace.Constants;

public static class Application
{
    public const string ServiceName = "$display_name";
    public const string OpenApiTag = "$tag_name";
}
"""


ENVIRONMENT_CONSTANTS_CS = """
namespace $namespace.Constants;

public static class Environment
{
    public const string OpenApiScope = "OpenApi__Scope";
    public const string OpenApiTokenUrl = "OpenApi__TokenUrl";
    public const string OpenApiDocTitle = "OpenApi__DocTitle";
    public const string OpenApiDescription = "OpenApi__Description";
    public const string OpenApiDocVersion = "OpenApi__DocVersion";
    public const string OpenApiVersion = "OpenApi__Version";
    public const string OpenApiForceHttps = "OpenApi__ForceHttps";
    public const string OpenApiForceHttp = "OpenApi__ForceHttp";
    public const string OpenApiHostNames = "OpenApi__HostNames";
}
"""


HELLO_WORLD_REQUEST_CS = """
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;

namespace $namespace.Domain.HelloWorld;

public sealed class HelloWorldRequest
{
    [OpenApiProperty(Description = "Optional name to include in the greeting.")]
    public string? Name { get; init; }
}
"""


HELLO_WORLD_RESPONSE_CS = """
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;

namespace $namespace.Domain.HelloWorld;

public sealed class HelloWorldResponse
{
    [OpenApiProperty(Description = "Name used to create the greeting.")]
    public string Name { get; init; } = string.Empty;

    [OpenApiProperty(Description = "Generated hello-world message.")]
    public string Message { get; init; } = string.Empty;

    [OpenApiProperty(Description = "UTC timestamp for the generated response.")]
    public DateTimeOffset GeneratedAtUtc { get; init; }
}
"""


GET_HELLO_WORLD_QUERY_CS = """
namespace $namespace.Application.HelloWorld;

public sealed record GetHelloWorldQuery(string? Name);
"""


GET_HELLO_WORLD_QUERY_HANDLER_CS = """
using $namespace.Domain.HelloWorld;
using $namespace.Interfaces;

namespace $namespace.Application.HelloWorld;

public sealed class GetHelloWorldQueryHandler : IGetHelloWorldQueryHandler
{
    private readonly IHelloWorldMessageService _messageService;

    public GetHelloWorldQueryHandler(IHelloWorldMessageService messageService)
    {
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
    }

    public Task<HelloWorldResponse> HandleAsync(GetHelloWorldQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        cancellationToken.ThrowIfCancellationRequested();

        var name = _messageService.ResolveName(query.Name);
        var response = new HelloWorldResponse
        {
            Name = name,
            Message = _messageService.BuildMessage(name),
            GeneratedAtUtc = DateTimeOffset.UtcNow
        };

        return Task.FromResult(response);
    }
}
"""


I_GET_HELLO_WORLD_QUERY_HANDLER_CS = """
using $namespace.Application.HelloWorld;
using $namespace.Domain.HelloWorld;

namespace $namespace.Interfaces;

public interface IGetHelloWorldQueryHandler
{
    Task<HelloWorldResponse> HandleAsync(GetHelloWorldQuery query, CancellationToken cancellationToken);
}
"""


I_HELLO_WORLD_MESSAGE_SERVICE_CS = """
namespace $namespace.Interfaces;

public interface IHelloWorldMessageService
{
    string ResolveName(string? name);

    string BuildMessage(string name);
}
"""


HELLO_WORLD_MESSAGE_SERVICE_CS = """
using $namespace.Interfaces;

namespace $namespace.Services;

public sealed class HelloWorldMessageService : IHelloWorldMessageService
{
    public string ResolveName(string? name)
    {
        return string.IsNullOrWhiteSpace(name) ? "World" : name.Trim();
    }

    public string BuildMessage(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return $$"Hello, {name}!";
    }
}
"""


HELLO_WORLD_CONTROLLER_CS = """
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System.Net;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using $namespace.Application.HelloWorld;
using $namespace.Domain.HelloWorld;
using $namespace.Interfaces;

namespace $namespace.Triggers.Http;

public sealed class HelloWorldController
{
    private readonly IGetHelloWorldQueryHandler _handler;

    public HelloWorldController(IGetHelloWorldQueryHandler handler)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    [OpenApiOperation(operationId: "GetHelloWorld", tags: new[] { Constants.Application.OpenApiTag }, Summary = "Returns a hello-world message.", Description = "Returns a hello-world message for the supplied name.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
    [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "Optional name to include in the greeting.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(HelloWorldResponse), Summary = "OK", Description = "OK")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized, Summary = "Unauthorized", Description = "Unauthorized")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Forbidden, Summary = "Forbidden", Description = "Forbidden")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.InternalServerError, Summary = "Internal Server Error", Description = "Internal Server Error")]
    [Function("GetHelloWorld")]
    public async Task<HttpResponseData> GetHelloWorld(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "hello")] HttpRequestData req,
        CancellationToken cancellationToken)
    {
        var name = req.Query["name"];
        var responsePayload = await _handler.HandleAsync(new GetHelloWorldQuery(name), cancellationToken);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(responsePayload, cancellationToken);

        return response;
    }

    [OpenApiOperation(operationId: "PostHelloWorld", tags: new[] { Constants.Application.OpenApiTag }, Summary = "Returns a hello-world message from a JSON request.", Description = "Accepts a JSON body and returns a hello-world response.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
    [OpenApiRequestBody("application/json", typeof(HelloWorldRequest), Required = false, Description = "Optional hello-world request body.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(HelloWorldResponse), Summary = "OK", Description = "OK")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized, Summary = "Unauthorized", Description = "Unauthorized")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Forbidden, Summary = "Forbidden", Description = "Forbidden")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.InternalServerError, Summary = "Internal Server Error", Description = "Internal Server Error")]
    [Function("PostHelloWorld")]
    public async Task<HttpResponseData> PostHelloWorld(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "hello")] HttpRequestData req,
        CancellationToken cancellationToken)
    {
        var request = await req.ReadFromJsonAsync<HelloWorldRequest>(cancellationToken) ?? new HelloWorldRequest();
        var responsePayload = await _handler.HandleAsync(new GetHelloWorldQuery(request.Name), cancellationToken);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(responsePayload, cancellationToken);

        return response;
    }
}
"""


HEALTH_CONTROLLER_CS = """
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Net;

namespace $namespace.Triggers.Http;

public sealed class HealthController
{
    private readonly HealthCheckService _healthCheckService;

    public HealthController(HealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService ?? throw new ArgumentNullException(nameof(healthCheckService));
    }

    [OpenApiOperation(operationId: "Heartbeat", tags: new[] { "HealthCheck" }, Summary = "Runs health checks for the function app.", Description = "Returns the current health report for the microservice.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(HealthReport), Summary = "OK", Description = "OK")]
    [Function("Heartbeat")]
    public async Task<HttpResponseData> Heartbeat(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "heartbeat")] HttpRequestData req,
        CancellationToken cancellationToken)
    {
        var health = await _healthCheckService.CheckHealthAsync(cancellationToken);
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(health, cancellationToken);

        return response;
    }

    [Function(nameof(Warmup))]
    public static void Warmup([WarmupTrigger] object warmupContext, FunctionContext context)
    {
        var logger = context.GetLogger(nameof(Warmup));
        logger.LogInformation("Function App instance is now warm.");
    }
}
"""


ERROR_CODES_JSON = """{
  "errors": []
}
"""


TEST_CSPROJ = """
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="coverlet.collector" Version="6.0.4">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="10.0.12" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.3.0" />
    <PackageReference Include="xunit.v3" Version="3.2.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.5">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\\$project_name\\$project_name.csproj" />
  </ItemGroup>

  <ItemGroup>
    <None Update=".mocks\\**\\*">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>
</Project>
"""


GLOBALS_CS = """
global using Xunit;
"""


HANDLER_TESTS_CS = """
using Microsoft.Extensions.DependencyInjection;
using $namespace.Application.HelloWorld;
using $namespace.Interfaces;
using $namespace.Services;

namespace $test_namespace;

public sealed class GetHelloWorldQueryHandlerTests
{
    [Fact]
    public async Task HandleAsyncReturnsPersonalizedMessage()
    {
        var serviceProvider = CreateServiceProvider();
        var handler = serviceProvider.GetRequiredService<IGetHelloWorldQueryHandler>();

        var response = await handler.HandleAsync(new GetHelloWorldQuery("Ada"), CancellationToken.None);

        Assert.Equal("Ada", response.Name);
        Assert.Equal("Hello, Ada!", response.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsyncUsesWorldWhenNameIsMissing(string? name)
    {
        var serviceProvider = CreateServiceProvider();
        var handler = serviceProvider.GetRequiredService<IGetHelloWorldQueryHandler>();

        var response = await handler.HandleAsync(new GetHelloWorldQuery(name), CancellationToken.None);

        Assert.Equal("World", response.Name);
        Assert.Equal("Hello, World!", response.Message);
    }

    private static ServiceProvider CreateServiceProvider()
    {
        return new ServiceCollection()
            .AddLogging()
            .AddSingleton<IHelloWorldMessageService, HelloWorldMessageService>()
            .AddSingleton<IGetHelloWorldQueryHandler, GetHelloWorldQueryHandler>()
            .BuildServiceProvider();
    }
}
"""


DEPLOYPROJ = """
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="4.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <ItemGroup Label="ProjectConfigurations">
    <ProjectConfiguration Include="Debug|AnyCPU">
      <Configuration>Debug</Configuration>
      <Platform>AnyCPU</Platform>
    </ProjectConfiguration>
    <ProjectConfiguration Include="Release|AnyCPU">
      <Configuration>Release</Configuration>
      <Platform>AnyCPU</Platform>
    </ProjectConfiguration>
  </ItemGroup>
  <PropertyGroup Label="Globals">
    <ProjectGuid>$project_guid</ProjectGuid>
  </PropertyGroup>
  <PropertyGroup>
    <PrepareForBuildDependsOn>
    </PrepareForBuildDependsOn>
  </PropertyGroup>
  <Import Condition=" Exists('Deployment.targets') " Project="Deployment.targets" />
  <Import Project="$$(MSBuildToolsPath)\\Microsoft.Common.targets" />
  <Import Condition=" Exists('$$(MSBuildExtensionsPath)\\Microsoft\\VisualStudio\\v$$(VisualStudioVersion)\\Deployment\\1.1\\DeploymentProject.targets') " Project="$$(MSBuildExtensionsPath)\\Microsoft\\VisualStudio\\v$$(VisualStudioVersion)\\Deployment\\1.1\\DeploymentProject.targets" />
  <ItemGroup>
    <None Include="Deployment.targets">
      <Visible>False</Visible>
    </None>
    <None Include="Template\\azuredeploy_apim.json" />
    <None Include="Template\\azuredeploy_slot.json" />
  </ItemGroup>
  <ItemGroup>
    <Content Include="Deploy-AzureResourceGroup.ps1" />
    <Content Include="Parameters\\dev-azuredeploy.parameters.json" />
    <Content Include="Parameters\\sit-azuredeploy.parameters.json" />
    <Content Include="Parameters\\uat-azuredeploy.parameters.json" />
    <Content Include="Parameters\\prod-azuredeploy.parameters.json" />
    <Content Include="Template\\azuredeploy.json" />
  </ItemGroup>
  <Target Name="GetReferenceAssemblyPaths" />
</Project>
"""


DEPLOYMENT_TARGETS = """<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="4.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>
    <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>
    <OutputPath>bin\\$(Configuration)\\</OutputPath>
    <DebugSymbols>false</DebugSymbols>
    <SkipCopyBuildProduct>true</SkipCopyBuildProduct>
    <AddAdditionalExplicitAssemblyReferences>false</AddAdditionalExplicitAssemblyReferences>
    <TargetRuntime>None</TargetRuntime>
    <BaseIntermediateOutputPath Condition="'$(BaseIntermediateOutputPath)'=='' ">obj\\</BaseIntermediateOutputPath>
    <BaseIntermediateOutputPath Condition=" !HasTrailingSlash('$(BaseIntermediateOutputPath)') ">$(BaseIntermediateOutputPath)\\</BaseIntermediateOutputPath>
    <IntermediateOutputPath>$(BaseIntermediateOutputPath)$(Configuration)\\</IntermediateOutputPath>
    <ProjectReferencesOutputPath Condition=" '$(ProjectReferencesOutputPath)' == '' ">$(IntermediateOutputPath)ProjectReferences</ProjectReferencesOutputPath>
    <ProjectReferencesOutputPath Condition=" !HasTrailingSlash('$(ProjectReferencesOutputPath)') ">$(ProjectReferencesOutputPath)\\</ProjectReferencesOutputPath>
    <StageArtifacts Condition=" '$(StageArtifacts)' == '' ">true</StageArtifacts>
  </PropertyGroup>

  <PropertyGroup>
    <DefineCommonItemSchemas>false</DefineCommonItemSchemas>
    <DefineCommonCapabilities>false</DefineCommonCapabilities>
  </PropertyGroup>

  <ProjectExtensions>
    <ProjectCapabilities>
      <DeploymentProject />
    </ProjectCapabilities>
  </ProjectExtensions>

  <ItemDefinitionGroup>
    <Content>
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </Content>
    <None>
      <CopyToOutputDirectory>Never</CopyToOutputDirectory>
    </None>
    <ProjectReference>
      <Private>false</Private>
      <Targets>Build</Targets>
    </ProjectReference>
  </ItemDefinitionGroup>

  <Target Name="CreateManifestResourceNames" />
  <PropertyGroup>
    <StageArtifactsDependsOn>
      _GetDeploymentProjectContent;
      _CalculateContentOutputRelativePaths;
      _GetReferencedProjectsOutput;
      _CalculateArtifactStagingDirectory;
      _CopyOutputToArtifactStagingDirectory;
    </StageArtifactsDependsOn>
  </PropertyGroup>

  <Target Name="_CopyOutputToArtifactStagingDirectory">
    <Copy SourceFiles="@(DeploymentProjectContentOutput)" DestinationFiles="$(ArtifactStagingDirectory)\\$(MSBuildProjectName)%(RelativePath)" />
    <Copy SourceFiles="@(BuildProjectReferencesOutput)" DestinationFiles="$(ArtifactStagingDirectory)\\$(MSBuildProjectName)\\%(ProjectName)\\%(RecursiveDir)%(FileName)%(Extension)" />
  </Target>

  <Target Name="_GetDeploymentProjectContent">
    <MSBuild Projects="$(MSBuildProjectFile)" Targets="ContentFilesProjectOutputGroup">
      <Output TaskParameter="TargetOutputs" ItemName="DeploymentProjectContentOutput" />
    </MSBuild>
  </Target>

  <Target Name="_GetReferencedProjectsOutput">
    <PropertyGroup>
      <MsBuildProperties>Configuration=$(Configuration);Platform=$(Platform)</MsBuildProperties>
    </PropertyGroup>
    <MSBuild Projects="@(ProjectReference)" BuildInParallel="$(BuildInParallel)" Properties="$(MsBuildProperties)" Targets="%(ProjectReference.Targets)" />
    <ItemGroup>
      <BuildProjectReferencesOutput Include="%(ProjectReference.IncludeFilePath)">
        <ProjectName>$([System.IO.Path]::GetFileNameWithoutExtension('%(ProjectReference.Identity)'))</ProjectName>
      </BuildProjectReferencesOutput>
    </ItemGroup>
  </Target>

  <Target Name="_CalculateArtifactStagingDirectory" Condition=" '$(ArtifactStagingDirectory)'=='' ">
    <PropertyGroup>
      <ArtifactStagingDirectory Condition=" '$(OutDir)'!='' ">$(OutDir)</ArtifactStagingDirectory>
      <ArtifactStagingDirectory Condition=" '$(ArtifactStagingDirectory)'=='' ">$(OutputPath)</ArtifactStagingDirectory>
      <ArtifactStagingDirectory Condition=" !HasTrailingSlash('$(ArtifactStagingDirectory)') ">$(ArtifactStagingDirectory)\\</ArtifactStagingDirectory>
      <ArtifactStagingDirectory>$(ArtifactStagingDirectory)staging\\</ArtifactStagingDirectory>
      <ArtifactStagingDirectory Condition=" '$(Build_StagingDirectory)'!='' AND '$(TF_Build)'=='True' ">$(Build_StagingDirectory)</ArtifactStagingDirectory>
    </PropertyGroup>
  </Target>

  <Target Name="_CalculateContentOutputRelativePaths" Outputs="%(DeploymentProjectContentOutput.Identity)">
    <PropertyGroup>
      <_OriginalIdentity>%(DeploymentProjectContentOutput.Identity)</_OriginalIdentity>
      <_RelativePath>$(_OriginalIdentity.Replace('$(MSBuildProjectDirectory)', ''))</_RelativePath>
    </PropertyGroup>
    <ItemGroup>
      <DeploymentProjectContentOutput>
        <RelativePath>$(_RelativePath)</RelativePath>
      </DeploymentProjectContentOutput>
    </ItemGroup>
  </Target>

  <Target Name="CoreCompile" />
  <PropertyGroup>
    <StageArtifactsAfterTargets Condition=" '$(StageArtifacts)' == 'true' ">PrepareForRun</StageArtifactsAfterTargets>
  </PropertyGroup>
  <Target Name="StageArtifacts" DependsOnTargets="$(StageArtifactsDependsOn)" AfterTargets="$(StageArtifactsAfterTargets)" />
  <Target Name="DeleteBinObjFolders" BeforeTargets="Clean">
    <RemoveDir Directories="$(OutputPath)" />
    <RemoveDir Directories="$(BaseIntermediateOutputPath)" />
  </Target>
</Project>
"""


DEPLOY_AZURE_RESOURCE_GROUP_PS1 = """#Requires -Version 3.0

Param(
    [string] [Parameter(Mandatory=$true)] $ResourceGroupLocation,
    [string] $ResourceGroupName = 'VAA.EAPI',
    [switch] $UploadArtifacts,
    [string] $StorageAccountName,
    [string] $StorageContainerName = $ResourceGroupName.ToLowerInvariant() + '-stageartifacts',
    [string] $TemplateFile = 'Template/azuredeploy.json',
    [string] $TemplateParametersFile = 'Parameters/dev-azuredeploy.parameters.json',
    [string] $ArtifactStagingDirectory = '.',
    [string] $DSCSourceFolder = 'DSC',
    [switch] $ValidateOnly
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 3

function Format-ValidationOutput {
    param ($ValidationOutput, [int] $Depth = 0)
    Set-StrictMode -Off
    return @($ValidationOutput | Where-Object { $_ -ne $null } | ForEach-Object { @('  ' * $Depth + ': ' + $_.Message) + @(Format-ValidationOutput @($_.Details) ($Depth + 1)) })
}

$OptionalParameters = New-Object -TypeName Hashtable
$TemplateFile = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($PSScriptRoot, $TemplateFile))
$TemplateParametersFile = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($PSScriptRoot, $TemplateParametersFile))

if ($UploadArtifacts) {
    $ArtifactStagingDirectory = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($PSScriptRoot, $ArtifactStagingDirectory))
    $DSCSourceFolder = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($PSScriptRoot, $DSCSourceFolder))

    $JsonParameters = Get-Content $TemplateParametersFile -Raw | ConvertFrom-Json
    if (($JsonParameters | Get-Member -Type NoteProperty 'parameters') -ne $null) {
        $JsonParameters = $JsonParameters.parameters
    }
    $ArtifactsLocationName = '_artifactsLocation'
    $ArtifactsLocationSasTokenName = '_artifactsLocationSasToken'
    $OptionalParameters[$ArtifactsLocationName] = $JsonParameters | Select -Expand $ArtifactsLocationName -ErrorAction Ignore | Select -Expand 'value' -ErrorAction Ignore
    $OptionalParameters[$ArtifactsLocationSasTokenName] = $JsonParameters | Select -Expand $ArtifactsLocationSasTokenName -ErrorAction Ignore | Select -Expand 'value' -ErrorAction Ignore

    if (Test-Path $DSCSourceFolder) {
        $DSCSourceFilePaths = @(Get-ChildItem $DSCSourceFolder -File -Filter '*.ps1' | ForEach-Object -Process {$_.FullName})
        foreach ($DSCSourceFilePath in $DSCSourceFilePaths) {
            $DSCArchiveFilePath = $DSCSourceFilePath.Substring(0, $DSCSourceFilePath.Length - 4) + '.zip'
            Publish-AzureRmVMDscConfiguration $DSCSourceFilePath -OutputArchivePath $DSCArchiveFilePath -Force -Verbose
        }
    }

    if ($StorageAccountName -eq '') {
        $StorageAccountName = 'stage' + ((Get-AzureRmContext).Subscription.SubscriptionId).Replace('-', '').substring(0, 19)
    }

    $StorageAccount = (Get-AzureRmStorageAccount | Where-Object{$_.StorageAccountName -eq $StorageAccountName})
    if ($StorageAccount -eq $null) {
        $StorageResourceGroupName = 'ARM_Deploy_Staging'
        New-AzureRmResourceGroup -Location "$ResourceGroupLocation" -Name $StorageResourceGroupName -Force
        $StorageAccount = New-AzureRmStorageAccount -StorageAccountName $StorageAccountName -Type 'Standard_LRS' -ResourceGroupName $StorageResourceGroupName -Location "$ResourceGroupLocation"
    }

    if ($OptionalParameters[$ArtifactsLocationName] -eq $null) {
        $OptionalParameters[$ArtifactsLocationName] = $StorageAccount.Context.BlobEndPoint + $StorageContainerName + '/'
    }

    New-AzureStorageContainer -Name $StorageContainerName -Context $StorageAccount.Context -ErrorAction SilentlyContinue *>&1
    $ArtifactFilePaths = Get-ChildItem $ArtifactStagingDirectory -Recurse -File | ForEach-Object -Process {$_.FullName}
    foreach ($SourcePath in $ArtifactFilePaths) {
        Set-AzureStorageBlobContent -File $SourcePath -Blob $SourcePath.Substring($ArtifactStagingDirectory.length + 1) -Container $StorageContainerName -Context $StorageAccount.Context -Force
    }

    if ($OptionalParameters[$ArtifactsLocationSasTokenName] -eq $null) {
        $OptionalParameters[$ArtifactsLocationSasTokenName] = ConvertTo-SecureString -AsPlainText -Force (New-AzureStorageContainerSASToken -Container $StorageContainerName -Context $StorageAccount.Context -Permission r -ExpiryTime (Get-Date).AddHours(4))
    }
}

if ((Get-AzureRmResourceGroup -Name $ResourceGroupName -Location $ResourceGroupLocation -Verbose -ErrorAction SilentlyContinue) -eq $null) {
    New-AzureRmResourceGroup -Name $ResourceGroupName -Location $ResourceGroupLocation -Verbose -Force -ErrorAction Stop
}

if ($ValidateOnly) {
    $ErrorMessages = Format-ValidationOutput (Test-AzureRmResourceGroupDeployment -ResourceGroupName $ResourceGroupName -TemplateFile $TemplateFile -TemplateParameterFile $TemplateParametersFile @OptionalParameters)
    if ($ErrorMessages) {
        Write-Output '', 'Validation returned the following errors:', @($ErrorMessages), '', 'Template is invalid.'
    }
    else {
        Write-Output '', 'Template is valid.'
    }
}
else {
    New-AzureRmResourceGroupDeployment -Name ((Get-ChildItem $TemplateFile).BaseName + '-' + ((Get-Date).ToUniversalTime()).ToString('MMdd-HHmm')) -ResourceGroupName $ResourceGroupName -TemplateFile $TemplateFile -TemplateParameterFile $TemplateParametersFile @OptionalParameters -Force -Verbose -ErrorVariable ErrorMessages
    if ($ErrorMessages) {
        Write-Output '', 'Template deployment returned the following errors:', @(@($ErrorMessages) | ForEach-Object { $_.Exception.Message.TrimEnd("`r`n") })
    }
}
"""


def app_settings() -> list[dict[str, str]]:
    return [
        {"name": "FUNCTIONS_EXTENSION_VERSION", "value": "~4"},
        {"name": "FUNCTIONS_WORKER_RUNTIME", "value": "[parameters('FUNCTIONS_WORKER_RUNTIME')]"},
        {
            "name": "AzureWebJobsStorage",
            "value": "[concat('DefaultEndpointsProtocol=https;AccountName=', parameters('storageaccountName'), ';EndpointSuffix=', environment().suffixes.storage, ';AccountKey=', listKeys(resourceId('Microsoft.Storage/storageAccounts', parameters('storageaccountName')), '2021-09-01').keys[0].value)]",
        },
        {"name": "APPINSIGHTS_INSTRUMENTATIONKEY", "value": "[parameters('APPINSIGHTS_INSTRUMENTATIONKEY')]"},
        {"name": "OpenApi__DocTitle", "value": "[parameters('OpenApi__DocTitle')]"},
        {"name": "OpenApi__Description", "value": "[parameters('OpenApi__Description')]"},
        {"name": "OpenApi__DocVersion", "value": "[parameters('OpenApi__DocVersion')]"},
        {"name": "OpenApi__Version", "value": "[parameters('OpenApi__Version')]"},
        {"name": "OpenApi__TokenUrl", "value": "[parameters('OpenApi__TokenUrl')]"},
        {"name": "OpenApi__Scope", "value": "[parameters('OpenApi__Scope')]"},
        {"name": "OpenApi__ForceHttps", "value": "[parameters('OpenApi__ForceHttps')]"},
        {"name": "OpenApi__ForceHttp", "value": "[parameters('OpenApi__ForceHttp')]"},
        {"name": "OpenApi__HostNames", "value": "[parameters('OpenApi__HostNames')]"},
        {"name": "WEBSITE_DNS_SERVER", "value": "[parameters('WEBSITE_DNS_SERVER')]"},
        {"name": "WEBSITE_DNS_ALT_SERVER", "value": "[parameters('WEBSITE_DNS_ALT_SERVER')]"},
        {"name": "WEBSITE_LOAD_USER_PROFILE", "value": "[parameters('WEBSITE_LOAD_USER_PROFILE')]"},
        {"name": "WEBSITE_RUN_FROM_PACKAGE", "value": "[parameters('WEBSITE_RUN_FROM_PACKAGE')]"},
    ]


def arm_template(include_slot: bool) -> dict:
    parameters = {
        "environment": {"type": "string"},
        "AppServicePlanName": {"type": "string"},
        "FunctionAppName": {"type": "string"},
        "location": {"type": "string"},
        "AppInsightsName": {"type": "string"},
        "APPINSIGHTS_INSTRUMENTATIONKEY": {"type": "string"},
        "FUNCTIONS_WORKER_RUNTIME": {"type": "string"},
        "aseRg": {"type": "string"},
        "aseName": {"type": "string"},
        "keyVaultName": {"type": "string"},
        "storageaccountName": {"type": "string"},
        "IPRestriction": {"type": "string", "defaultValue": "0.0.0.0/0"},
        "SlotName": {"type": "string", "defaultValue": "DS"},
        "WEBSITE_DNS_SERVER": {"type": "string", "defaultValue": ""},
        "WEBSITE_DNS_ALT_SERVER": {"type": "string", "defaultValue": ""},
        "WEBSITE_LOAD_USER_PROFILE": {"type": "string", "defaultValue": "1"},
        "WEBSITE_RUN_FROM_PACKAGE": {"type": "string", "defaultValue": "1"},
        "OpenApi__DocTitle": {"type": "string"},
        "OpenApi__Description": {"type": "string"},
        "OpenApi__DocVersion": {"type": "string"},
        "OpenApi__Version": {"type": "string", "defaultValue": "v3"},
        "OpenApi__TokenUrl": {"type": "string"},
        "OpenApi__Scope": {"type": "string"},
        "OpenApi__ForceHttps": {"type": "string", "defaultValue": "true"},
        "OpenApi__ForceHttp": {"type": "string", "defaultValue": "false"},
        "OpenApi__HostNames": {"type": "string"},
        "tags": {"type": "object"},
    }

    site_config = {
        "appSettings": app_settings(),
        "ipSecurityRestrictions": [
            {
                "ipAddress": "[parameters('IPRestriction')]",
                "action": "Allow",
                "priority": 1,
                "name": "Allow_APIM",
                "description": "Allow APIM to reach the function app.",
            }
        ],
        "ipSecurityRestrictionsDefaultAction": "Deny",
    }

    resources: list[dict] = [
        {
            "type": "Microsoft.Storage/storageAccounts",
            "apiVersion": "2021-09-01",
            "name": "[parameters('storageaccountName')]",
            "location": "[parameters('location')]",
            "tags": "[parameters('tags')]",
            "sku": {"name": "Standard_RAGRS", "tier": "Standard"},
            "kind": "StorageV2",
            "properties": {"allowBlobPublicAccess": False},
        },
        {
            "type": "Microsoft.Insights/components",
            "apiVersion": "2020-02-02",
            "name": "[parameters('AppInsightsName')]",
            "location": "[parameters('location')]",
            "tags": "[parameters('tags')]",
            "kind": "web",
            "properties": {"Application_Type": "web"},
        },
        {
            "type": "Microsoft.Web/sites",
            "apiVersion": "2022-03-01",
            "name": "[parameters('FunctionAppName')]",
            "location": "[parameters('location')]",
            "tags": "[parameters('tags')]",
            "kind": "functionapp",
            "identity": {"type": "SystemAssigned"},
            "dependsOn": [
                "[resourceId('Microsoft.Storage/storageAccounts', parameters('storageaccountName'))]",
                "[resourceId('Microsoft.Insights/components', parameters('AppInsightsName'))]",
            ],
            "properties": {
                "enabled": True,
                "serverFarmId": "[resourceId(parameters('aseRg'), 'Microsoft.Web/serverFarms', parameters('AppServicePlanName'))]",
                "hostingEnvironmentProfile": "[resourceId(parameters('aseRg'), 'Microsoft.Web/hostingEnvironments', parameters('aseName'))]",
                "httpsOnly": True,
                "siteConfig": site_config,
            },
        },
    ]

    if include_slot:
        resources.append(
            {
                "type": "Microsoft.Web/sites/slots",
                "apiVersion": "2022-03-01",
                "name": "[concat(parameters('FunctionAppName'), '/', parameters('SlotName'))]",
                "location": "[parameters('location')]",
                "tags": "[parameters('tags')]",
                "kind": "functionapp",
                "dependsOn": ["[resourceId('Microsoft.Web/sites', parameters('FunctionAppName'))]"],
                "properties": {
                    "enabled": True,
                    "serverFarmId": "[resourceId(parameters('aseRg'), 'Microsoft.Web/serverFarms', parameters('AppServicePlanName'))]",
                    "hostingEnvironmentProfile": "[resourceId(parameters('aseRg'), 'Microsoft.Web/hostingEnvironments', parameters('aseName'))]",
                    "httpsOnly": True,
                    "siteConfig": site_config,
                },
            }
        )

    return {
        "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#",
        "contentVersion": "1.0.0.0",
        "parameters": parameters,
        "resources": resources,
    }


def apim_template() -> dict:
    return {
        "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#",
        "contentVersion": "1.0.0.0",
        "parameters": {
            "functionAppName": {"type": "string"},
            "apimEnv": {"type": "string"},
            "apiEnv": {"type": "string"},
            "apisEnv": {"type": "string"},
            "apimurlname": {"type": "string"},
            "APIMName": {"type": "string"},
            "applicationName": {"type": "string"},
            "tag": {"type": "string", "defaultValue": "Microservices"},
        },
        "variables": {
            "apiName": "[concat('enterprise-api-', parameters('apimurlname'), '-microservice-', parameters('apisEnv'))]"
        },
        "resources": [
            {
                "type": "Microsoft.ApiManagement/service/apis",
                "apiVersion": "2021-12-01-preview",
                "name": "[concat(parameters('APIMName'), '/', variables('apiName'))]",
                "properties": {
                    "displayName": "[variables('apiName')]",
                    "apiRevision": "1",
                    "description": "[variables('apiName')]",
                    "subscriptionRequired": True,
                    "serviceUrl": "[concat('https://', parameters('functionAppName'), '.azurewebsites.net/api')]",
                    "path": "[concat(parameters('apimEnv'), parameters('applicationName'))]",
                    "value": "[concat('https://', parameters('functionAppName'), '.azurewebsites.net/api/swagger.json')]",
                    "format": "openapi-link",
                    "protocols": ["https"],
                    "isCurrent": True,
                },
            },
            {
                "type": "Microsoft.ApiManagement/service/apis/operations",
                "apiVersion": "2021-08-01",
                "name": "[concat(parameters('APIMName'), '/', variables('apiName'), '/renderswaggerdocument')]",
                "dependsOn": [
                    "[resourceId('Microsoft.ApiManagement/service/apis', parameters('APIMName'), variables('apiName'))]"
                ],
                "properties": {
                    "displayName": "Render Swagger Document",
                    "method": "GET",
                    "urlTemplate": "/swagger.json",
                    "request": {"queryParameters": [], "headers": []},
                    "responses": [
                        {
                            "statusCode": 200,
                            "description": "Swagger JSON",
                            "representations": [{"contentType": "application/json"}],
                        }
                    ],
                },
            },
            {
                "type": "Microsoft.ApiManagement/service/apis/tags",
                "apiVersion": "2021-12-01-preview",
                "name": "[concat(parameters('APIMName'), '/', variables('apiName'), '/', parameters('tag'))]",
                "dependsOn": [
                    "[resourceId('Microsoft.ApiManagement/service/apis', parameters('APIMName'), variables('apiName'))]"
                ],
            },
            {
                "type": "Microsoft.ApiManagement/service/apis/tags",
                "apiVersion": "2021-12-01-preview",
                "name": "[concat(parameters('APIMName'), '/', variables('apiName'), '/', parameters('apisEnv'))]",
                "dependsOn": [
                    "[resourceId('Microsoft.ApiManagement/service/apis', parameters('APIMName'), variables('apiName'))]"
                ],
            },
        ],
    }


def parameter_file(service: ServiceName, environment: str) -> dict:
    env_upper = "PROD" if environment == "prod" else environment.upper()
    service_key = service.short_slug
    storage = storage_account_name(environment, service)
    function_name = f"todo-{environment}-{service_key}-fn"
    app_insights = f"todo-{environment}-{service_key}-ai"
    description = f"This API provides starter endpoints for the {service.display_name} microservice."

    return {
        "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentParameters.json#",
        "contentVersion": "1.0.0.0",
        "parameters": {
            "environment": {"value": env_upper},
            "AppServicePlanName": {"value": "TODO-APP-SERVICE-PLAN"},
            "FunctionAppName": {"value": function_name},
            "location": {"value": "North Europe"},
            "AppInsightsName": {"value": app_insights},
            "APPINSIGHTS_INSTRUMENTATIONKEY": {"value": "TODO-APPINSIGHTS-INSTRUMENTATIONKEY"},
            "FUNCTIONS_WORKER_RUNTIME": {"value": "dotnet-isolated"},
            "aseRg": {"value": "TODO-ASE-RESOURCE-GROUP"},
            "aseName": {"value": "TODO-ASE-NAME"},
            "keyVaultName": {"value": "TODO-KEY-VAULT-NAME"},
            "storageaccountName": {"value": storage},
            "IPRestriction": {"value": "0.0.0.0/0"},
            "SlotName": {"value": "DS"},
            "WEBSITE_DNS_SERVER": {"value": ""},
            "WEBSITE_DNS_ALT_SERVER": {"value": ""},
            "WEBSITE_LOAD_USER_PROFILE": {"value": "1"},
            "WEBSITE_RUN_FROM_PACKAGE": {"value": "1"},
            "OpenApi__Scope": {"value": "TODO-OPENAPI-SCOPE"},
            "OpenApi__TokenUrl": {"value": "TODO-OPENAPI-TOKEN-URL"},
            "OpenApi__DocTitle": {"value": f"Enterprise API - {service.display_name} Microservice"},
            "OpenApi__Description": {"value": description},
            "OpenApi__DocVersion": {"value": "1.0.0"},
            "OpenApi__HostNames": {"value": f"https://{function_name}.azurewebsites.net"},
            "OpenApi__Version": {"value": "v3"},
            "OpenApi__ForceHttps": {"value": "true"},
            "OpenApi__ForceHttp": {"value": "false"},
            "tags": {
                "value": {
                    "APPLICATION_NAME": f"{service.display_name} Microservice",
                    "DOMAIN": service.domain_name.upper(),
                    "ENVIRONMENT": env_upper,
                    "BUDGETCODE": "",
                    "PROJECTCODE": "",
                    "DEPLOYMENT_CR": "",
                }
            },
        },
    }


def storage_account_name(environment: str, service: ServiceName) -> str:
    candidate = f"todo{environment}{service.short_slug}sa"
    candidate = re.sub(r"[^a-z0-9]", "", candidate.lower())
    return candidate[:24].ljust(3, "0")


def render_pipeline_yaml(service: ServiceName) -> str:
    app = service.application_name
    deployment = f"{PREFIX}.{app}.Deployment"
    microservice_name = f"{service.apim_url_name}microservice"
    apim_url = service.apim_url_name
    short_slug = service.short_slug

    return dedent(
        f"""\
        trigger:
          branches:
            include:
              - '*'
          paths:
            include:
              - {PREFIX}/{PREFIX}.{app}*
              - {DEPLOYMENTS_ROOT}/{deployment}*

        pr:
          branches:
            include:
              - main
          paths:
            include:
              - {PREFIX}/{PREFIX}.{app}*
              - {DEPLOYMENTS_ROOT}/{deployment}*

        variables:
          - group: Common_Variables
          - group: AzureWebJobsStorage
          - group: APIM
          - group: APIM_IP
          - group: Spectral_Ops
          - name: agentPool
            value: 'Azure Pipelines'
          - name: agentSpecification
            value: 'windows-latest'
          - name: buildConfiguration
            value: 'release'
          - name: rootPath
            value: '{PREFIX}'
          - name: subRootPath
            value: '{PREFIX}'
          - name: applicationName
            value: '{app}'
          - name: microserviceName
            value: '{microservice_name}'
          - name: apimurlname
            value: '{apim_url}'
          - name: functionAppName_DEV
            value: 'todo-dev-{short_slug}-fn'
          - name: functionAppName_SIT
            value: 'todo-sit-{short_slug}-fn'
          - name: functionAppName_UAT
            value: 'todo-uat-{short_slug}-fn'
          - name: functionAppName_PRD
            value: 'todo-prod-{short_slug}-fn'
          - name: slotName
            value: 'DS'
          - name: testRepoPath_SIT
            value: 'VAA.ENTERPRISE.API.TESTS.CORE'
          - name: testRepoPath_UAT
            value: 'VAA.ENTERPRISE.API.TESTS.CORE'

        name: $(BuildDefinitionName)_1.0.$(year:yy)$(DayOfYear)$(Rev:.r)

        stages:
          - template: functions/build_apim.yaml
            parameters:
              agentPool: $(agentPool)
              agentSpecification: $(agentSpecification)
              buildConfiguration: $(buildConfiguration)
              location: $(location)
              rootPath: $(rootPath)
              subRootPath: $(subRootPath)
              applicationName: $(applicationName)
              SPECTRAL_DSN: $(SPECTRAL_DSN)
              SPECTRAL_KEY: $(SPECTRAL_KEY)
              resourceGroupName_DEV: $(resourceGroupName_DEV)
              resourceGroupName_SIT: $(resourceGroupName_SIT)
              resourceGroupName_UAT: $(resourceGroupName_UAT)
              resourceGroupName_PROD: $(resourceGroupName_PROD)
              serviceConnection_NonProd: $(serviceConnection_NonProd)
              serviceConnection_Prod: $(serviceConnection_Prod)
              subscriptionId_NonProd: $(subscriptionId_NonProd)
              subscriptionId_Prod: $(subscriptionId_Prod)

          - template: functions/release_slot_apim.yaml
            parameters:
              agentPool: $(agentPool)
              agentSpecification: $(agentSpecification)
              buildConfiguration: $(buildConfiguration)
              location: $(location)
              applicationName: $(applicationName)
              microserviceName: $(microserviceName)
              apimurlname: $(apimurlname)
              resourceGroupName_DEV: $(resourceGroupName_DEV)
              resourceGroupName_SIT: $(resourceGroupName_SIT)
              resourceGroupName_UAT: $(resourceGroupName_UAT)
              resourceGroupName_PROD: $(resourceGroupName_PROD)
              serviceConnection_NonProd: $(serviceConnection_NonProd)
              serviceConnection_Prod: $(serviceConnection_Prod)
              subscriptionId_NonProd: $(subscriptionId_NonProd)
              subscriptionId_Prod: $(subscriptionId_Prod)
              functionAppName_DEV: $(functionAppName_DEV)
              functionAppName_SIT: $(functionAppName_SIT)
              functionAppName_UAT: $(functionAppName_UAT)
              functionAppName_PRD: $(functionAppName_PRD)
              testRepoPath_SIT: $(testRepoPath_SIT)
              testRepoPath_UAT: $(testRepoPath_UAT)
              APIM_NonProd: $(APIM_NonProd)
              APIM_Prod: $(APIM_Prod)
              APIMResourceGroupName_NonProd: $(APIMResourceGroupName_NonProd)
              APIMServiceConnection_NonProd: $(APIMServiceConnection_NonProd)
              APIMSubscriptionId_NonProd: $(APIMSubscriptionId_NonProd)
              APIMResourceGroupName_Prod: $(APIMResourceGroupName_Prod)
              APIMServiceConnection_Prod: $(APIMServiceConnection_Prod)
              APIMSubscriptionId_Prod: $(APIMSubscriptionId_Prod)
              APIM_IP_DEV_SIT_UAT: $(APIM_IP_DEV_SIT_UAT)
              APIM_IP_PRD: $(APIM_IP_PRD)
              WEBSITE_DNS_ALT_SERVER: $(WEBSITE_DNS_ALT_SERVER)
              WEBSITE_DNS_SERVER: $(WEBSITE_DNS_SERVER)
        """
    )


if __name__ == "__main__":
    raise SystemExit(main())
