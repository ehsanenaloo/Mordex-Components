[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    [switch]$Rebuild
)

$ErrorActionPreference = 'Stop'
$solution = Join-Path $PSScriptRoot 'Mordex.sln'
$buildArguments = @('build', $solution, '--configuration', $Configuration, '--nologo', '--disable-build-servers', '-m:1')
if ($Rebuild) { $buildArguments += '--no-incremental' }

& dotnet @buildArguments
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$destination = Join-Path $PSScriptRoot 'artifacts/bin'
New-Item -ItemType Directory -Force -Path $destination | Out-Null
foreach ($project in Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'src') -Directory) {
    foreach ($extension in @('dll', 'pdb', 'xml')) {
        $file = Join-Path $project.FullName "bin/$Configuration/net10.0-windows/$((Get-ChildItem -LiteralPath $project.FullName -Filter *.csproj).BaseName).$extension"
        if (Test-Path -LiteralPath $file -PathType Leaf) {
            Copy-Item -LiteralPath $file -Destination $destination -Force
        }
    }
}
Write-Output "Built assemblies: $destination"
