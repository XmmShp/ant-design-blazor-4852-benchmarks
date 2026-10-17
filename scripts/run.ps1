param(
    [int]$Pairs = 12,
    [int]$Samples = 4,
    [int]$Warmups = 3,
    [string]$OutputName = 'windows-net10-run1',
    [string]$Only = 'all',
    [string]$DefinitionFile = ''
)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$output = "$root/results/$OutputName"
New-Item -ItemType Directory -Force $output | Out-Null
$env:DOTNET_TieredCompilation = '0'
$env:DOTNET_ReadyToRun = '0'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$rng = [Random]::new(4852)

$cases = @(
    @{ id='none-100x10'; rows=100; cols=10; a='baseline'; b='candidate'; modeA='none'; modeB='none' },
    @{ id='none-1000x10'; rows=1000; cols=10; a='baseline'; b='candidate'; modeA='none'; modeB='none' },
    @{ id='none-2000x20'; rows=2000; cols=20; a='baseline'; b='candidate'; modeA='none'; modeB='none' },
    @{ id='legacy-1000x10'; rows=1000; cols=10; a='baseline'; b='candidate'; modeA='legacy'; modeB='legacy' },
    @{ id='merge-1000x10'; rows=1000; cols=10; a='candidate'; b='candidate'; modeA='legacy'; modeB='both' },
    @{ id='micro'; rows=0; cols=0; a='baseline'; b='candidate'; modeA='merge'; modeB='merge' }
)
$baselineCommit = '943f6af77201ba1ebfd4756c054ff838d8bef0d0'
$candidateCommit = '67486a17c31bab00665b617919b7ac99ad218468'
$extendedEmptyCases = $false
$equivalentWorkCases = @($cases | Where-Object { $_.id.StartsWith('none-') } | ForEach-Object { $_.id })
if ($DefinitionFile) {
    $definition = Get-Content (Join-Path $root $DefinitionFile) -Raw | ConvertFrom-Json -AsHashtable
    $baselineCommit = $definition.baseline
    $candidateCommit = $definition.candidate
    $cases = @($definition.cases)
    $extendedEmptyCases = [bool]$definition.extendedEmptyCases
    $equivalentWorkCases = @($definition.equivalentWorkCases)
}
if ($Only -ne 'all') { $cases = @($cases | Where-Object { $_.id -eq $Only }) }
if ($cases.Count -eq 0) { throw 'Unknown case' }

$cpu = if ($IsWindows) { Get-CimInstance Win32_Processor | Select-Object Name, NumberOfCores, NumberOfLogicalProcessors } else { $null }
$power = if ($IsWindows) { (powercfg /getactivescheme | Out-String).Trim() } else { $null }
$manifest = @{
    schema=1; harnessCommit=(git -C $root rev-parse HEAD); startedUtc=[DateTimeOffset]::UtcNow.ToString('o');
    baseline=$baselineCommit; candidate=$candidateCommit;
    definitionFile=$DefinitionFile; cases=@($cases | ForEach-Object { $_.id });
    microScenarioCount=$(if ($extendedEmptyCases) { 26 } else { 18 });
    equivalentWorkCases=$equivalentWorkCases;
    sdk=(dotnet --version); pairs=$Pairs; samplesPerProcess=$Samples; warmupsPerProcess=$Warmups;
    seed=4852; cpu=$cpu; powerScheme=$power; affinity='OS default; no affinity pinning';
    tieredCompilation=0; readyToRun=0; only=$Only; processIsolation='new sequential process per arm per pair'
}
$manifest | ConvertTo-Json -Depth 6 | Set-Content "$output/manifest-$Only.json" -Encoding utf8
for ($pair = 0; $pair -lt $Pairs; $pair++) {
    # Shuffle case order in each block; alternate A/B ordering in balanced
    # two-block groups so the second version is not always warmer or cooler.
    $ordered = @($cases | Sort-Object { $rng.Next() })
    foreach ($case in $ordered) {
        $arms = if (($pair % 2) -eq 0) { @('a','b') } else { @('b','a') }
        foreach ($arm in $arms) {
            $version = $case[$arm]
            $cellMode = $case["mode$($arm.ToUpperInvariant())"]
            $file = "$output/$($case.id)-pair$('{0:d2}' -f $pair)-$arm.jsonl"
            if (Test-Path -LiteralPath $file) { throw "Refusing to overwrite $file" }
            $benchmarkArguments = @("$root/artifacts/bin-$version/Benchmarks.dll", '--version', $version,
                '--samples', $Samples, '--warmups', $Warmups, '--output', $file)
            if ($case.id -eq 'micro') {
                $benchmarkArguments += @('--mode', 'merge')
                if ($extendedEmptyCases) { $benchmarkArguments += @('--empty-cases', 'true') }
            }
            else { $benchmarkArguments += @('--rows', $case.rows, '--columns', $case.cols, '--cell-mode', $cellMode) }
            Write-Output "pair=$pair case=$($case.id) arm=$arm version=$version mode=$cellMode"
            & dotnet @benchmarkArguments
            if ($LASTEXITCODE) { throw "Benchmark failed: $file" }
        }
    }
}
Write-Output "Completed: $OutputName"
