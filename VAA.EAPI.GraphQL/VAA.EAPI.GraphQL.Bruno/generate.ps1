param(
    [string]$SchemaEndpoint = 'https://testresources.virginatlantic.com/dev/graphql/full/public'
)

$ErrorActionPreference = 'Stop'

function ConvertTo-GraphQlTypeReference {
    param($Type)

    switch ($Type.kind) {
        'NON_NULL' { return "$(ConvertTo-GraphQlTypeReference $Type.ofType)!" }
        'LIST' { return "[$(ConvertTo-GraphQlTypeReference $Type.ofType)]" }
        default { return $Type.name }
    }
}

function Get-NamedType {
    param($Type)

    while ($Type.kind -in @('NON_NULL', 'LIST')) {
        $Type = $Type.ofType
    }
    return $Type
}

function Get-PlaceholderValue {
    param(
        $Type,
        [hashtable]$TypesByName,
        [System.Collections.Generic.HashSet[string]]$Ancestors
    )

    if ($Type.kind -eq 'NON_NULL') {
        if ($Type.ofType.kind -eq 'LIST') {
            return @()
        }
        return Get-PlaceholderValue -Type $Type.ofType -TypesByName $TypesByName -Ancestors $Ancestors
    }

    if ($Type.kind -eq 'LIST') {
        return $null
    }

    $namedType = $TypesByName[$Type.name]
    if ($null -eq $namedType) {
        return ''
    }

    if ($namedType.kind -eq 'ENUM') {
        return @($namedType.enumValues)[0].name
    }

    if ($namedType.kind -eq 'INPUT_OBJECT') {
        if ($Ancestors.Contains($namedType.name)) {
            return @{}
        }

        $nextAncestors = [System.Collections.Generic.HashSet[string]]::new($Ancestors, [System.StringComparer]::Ordinal)
        [void]$nextAncestors.Add($namedType.name)
        $value = [ordered]@{}
        foreach ($field in @($namedType.inputFields)) {
            if ($field.type.kind -eq 'NON_NULL' -and [string]::IsNullOrWhiteSpace($field.defaultValue)) {
                $value[$field.name] = Get-PlaceholderValue -Type $field.type -TypesByName $TypesByName -Ancestors $nextAncestors
            }
        }
        return $value
    }

    switch ($namedType.name) {
        'Boolean' { return $false }
        { $_ -in @('Int', 'Long', 'Short', 'Byte', 'Float', 'Decimal') } { return 0 }
        'Any' { return @{} }
        default { return '' }
    }
}

function ConvertTo-SafeFileName {
    param([string]$Value)

    return (($Value -replace '[<>:"/\\|?*]', ' ') -replace '\s+', ' ').Trim().TrimEnd('.')
}

$introspectionQuery = @'
query BrunoCollectionSchema {
  __schema {
    queryType { name }
    mutationType { name }
    subscriptionType { name }
    types {
      kind
      name
      enumValues(includeDeprecated: true) { name }
      inputFields {
        name
        defaultValue
        type {
          kind name
          ofType { kind name ofType { kind name ofType { kind name ofType { kind name ofType { kind name } } } } }
        }
      }
      fields(includeDeprecated: true) {
        name
        isDeprecated
        deprecationReason
        args {
          name
          defaultValue
          type {
            kind name
            ofType { kind name ofType { kind name ofType { kind name ofType { kind name ofType { kind name } } } } }
          }
        }
        type {
          kind name
          ofType { kind name ofType { kind name ofType { kind name ofType { kind name ofType { kind name } } } } }
        }
      }
    }
  }
}
'@

$payload = @{ query = $introspectionQuery; operationName = 'BrunoCollectionSchema' } | ConvertTo-Json -Compress
$response = Invoke-RestMethod -Uri $SchemaEndpoint -Method Post -ContentType 'application/json' -Headers @{ Accept = 'application/json' } -Body $payload -TimeoutSec 60
if ($response.errors) {
    throw "GraphQL introspection failed: $($response.errors | ConvertTo-Json -Depth 10 -Compress)"
}

$schema = $response.data.__schema
$typesByName = @{}
foreach ($type in @($schema.types)) {
    if (-not [string]::IsNullOrWhiteSpace($type.name)) {
        $typesByName[$type.name] = $type
    }
}

$collection = @'
meta {
  name: VAA EAPI Full Public GraphQL
}

headers {
  Accept: application/json
  Content-Type: application/json
  api-channel: Bruno
  x-correlation-id: Bruno{{$guid}}
  Ocp-Apim-Subscription-Key: {{ocp_apim_subscription_key}}
}

auth {
  mode: none
}
'@
Set-Content -Path (Join-Path $PSScriptRoot 'collection.bru') -Value $collection -Encoding utf8NoBOM

$brunoJson = [ordered]@{
    version = '1'
    name = 'VAA EAPI Full Public GraphQL'
    type = 'collection'
    ignore = @('node_modules', '.git', 'generate.ps1', 'manifest.json', 'README.md')
} | ConvertTo-Json -Depth 5
Set-Content -Path (Join-Path $PSScriptRoot 'bruno.json') -Value $brunoJson -Encoding utf8NoBOM

$environmentRoot = Join-Path $PSScriptRoot 'environments'
[void](New-Item -ItemType Directory -Path $environmentRoot -Force)
$environments = [ordered]@{
    Dev = 'https://testresources.virginatlantic.com/dev/graphql/full/public'
    SIT = 'https://testresources.virginatlantic.com/SIT/graphql/full/public'
    UAT = 'https://testresources.virginatlantic.com/UAT/graphql/full/public'
    Production = 'https://resources.virginatlantic.com/graphql/full/public'
}
foreach ($environment in $environments.GetEnumerator()) {
    $content = @"
vars {
  graphql_url: $($environment.Value)
}

vars:secret [
  ocp_apim_subscription_key
]
"@
    Set-Content -Path (Join-Path $environmentRoot "$($environment.Key).bru") -Value $content -Encoding utf8NoBOM
}

$operationGroups = @(
    [pscustomobject]@{ Folder = 'Queries'; OperationType = 'query'; TypeName = $schema.queryType.name; Sequence = 1 },
    [pscustomobject]@{ Folder = 'Mutations'; OperationType = 'mutation'; TypeName = $schema.mutationType.name; Sequence = 2 }
)

$manifestOperations = [System.Collections.Generic.List[object]]::new()
foreach ($group in $operationGroups) {
    if ([string]::IsNullOrWhiteSpace($group.TypeName)) { continue }

    $folderPath = Join-Path $PSScriptRoot $group.Folder
    [void](New-Item -ItemType Directory -Path $folderPath -Force)
    Get-ChildItem $folderPath -Filter '*.bru' -File | Remove-Item -Force

    $folderContent = @"
meta {
  name: $($group.Folder)
  seq: $($group.Sequence)
}
"@
    Set-Content -Path (Join-Path $folderPath 'folder.bru') -Value $folderContent -Encoding utf8NoBOM

    $fields = @($typesByName[$group.TypeName].fields | Sort-Object name)
    $sequence = 1
    foreach ($field in $fields) {
        $operationName = $field.name -replace '[^_0-9A-Za-z]', '_'
        if ($operationName -match '^[0-9]') {
            $operationName = "Operation_$operationName"
        }
        $definitions = [System.Collections.Generic.List[string]]::new()
        $arguments = [System.Collections.Generic.List[string]]::new()
        $variables = [ordered]@{}
        foreach ($argument in @($field.args)) {
            $definitions.Add("`$$($argument.name): $(ConvertTo-GraphQlTypeReference $argument.type)")
            $arguments.Add("$($argument.name): `$$($argument.name)")
            $variables[$argument.name] = Get-PlaceholderValue -Type $argument.type -TypesByName $typesByName -Ancestors ([System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal))
        }

        $definitionText = if ($definitions.Count) { "(" + ($definitions -join ', ') + ")" } else { '' }
        $argumentText = if ($arguments.Count) { "(" + ($arguments -join ', ') + ")" } else { '' }
        $returnType = Get-NamedType $field.type
        $selection = if ($returnType.kind -in @('OBJECT', 'INTERFACE', 'UNION')) { " {`n      __typename`n    }" } else { '' }
        $variablesBlock = if ($variables.Count) {
            $variablesJson = $variables | ConvertTo-Json -Depth 100
            $variablesJsonIndented = (($variablesJson -split "`r?`n") | ForEach-Object { "  $_" }) -join "`n"
            "`nbody:graphql:vars {`n$variablesJsonIndented`n}`n"
        }
        else { '' }

        $requestContent = @"
meta {
  name: $($field.name)
  type: graphql
  seq: $sequence
}

post {
  url: {{graphql_url}}
  body: graphql
  auth: inherit
}

body:graphql {
  # Replace generated placeholder values with scenario data before execution.
  $($group.OperationType) $operationName$definitionText {
    $($field.name)$argumentText$selection
  }
}
$variablesBlock
settings {
  encodeUrl: true
  timeout: 0
}
"@
        $fileName = ConvertTo-SafeFileName $field.name
        Set-Content -Path (Join-Path $folderPath "$fileName.bru") -Value $requestContent -Encoding utf8NoBOM

        $manifestOperations.Add([pscustomobject]@{
            type = $group.OperationType
            name = $field.name
            deprecated = $field.isDeprecated
            deprecationReason = $field.deprecationReason
            arguments = @($field.args | ForEach-Object { [pscustomobject]@{ name = $_.name; type = ConvertTo-GraphQlTypeReference $_.type; defaultValue = $_.defaultValue } })
            returnType = ConvertTo-GraphQlTypeReference $field.type
        })
        $sequence++
    }
}

$manifest = [ordered]@{
    sourceEndpoint = $SchemaEndpoint
    queryCount = @($manifestOperations | Where-Object type -eq 'query').Count
    mutationCount = @($manifestOperations | Where-Object type -eq 'mutation').Count
    subscriptionCount = 0
    operations = $manifestOperations
}
$manifest | ConvertTo-Json -Depth 20 | Set-Content -Path (Join-Path $PSScriptRoot 'manifest.json') -Encoding utf8NoBOM

Write-Host "Generated $($manifest.queryCount) queries and $($manifest.mutationCount) mutations from $SchemaEndpoint."
