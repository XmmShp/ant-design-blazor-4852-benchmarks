param(
    [Parameter(Mandatory)][string]$BaselineBinary,
    [Parameter(Mandatory)][string]$CandidateBinary,
    [Parameter(Mandatory)][string]$OutputPrefix
)
$ErrorActionPreference = 'Stop'
$env:DOTNET_TieredCompilation = '0'
$env:DOTNET_ReadyToRun = '0'
foreach ($pair in 0..2) {
    $order = if ($pair % 2) { @('new', 'old') } else { @('old', 'new') }
    foreach ($arm in $order) {
        $binary = if ($arm -eq 'old') { $BaselineBinary } else { $CandidateBinary }
        $output = "$OutputPrefix-$pair-$arm.jsonl"
        if (Test-Path -LiteralPath $output) { throw "Refusing to overwrite $output" }
        dotnet $binary --mode merge --version $arm --samples 4 --warmups 3 --empty-cases true --output $output
        if ($LASTEXITCODE) { throw "Pilot failed: $output" }
    }
}
