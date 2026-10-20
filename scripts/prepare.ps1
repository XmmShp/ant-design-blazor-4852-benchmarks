param(
    [string]$SourceRepository = 'https://github.com/XmmShp/ant-design-blazor.git',
    [string]$DefinitionFile = ''
)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$versions = [ordered]@{
    baseline = '943f6af77201ba1ebfd4756c054ff838d8bef0d0'
    candidate = '67486a17c31bab00665b617919b7ac99ad218468'
}
if ($DefinitionFile) {
    $definition = Get-Content (Join-Path $root $DefinitionFile) -Raw | ConvertFrom-Json -AsHashtable
    $versions.baseline = $definition.baseline
    $versions.candidate = $definition.candidate
}
New-Item -ItemType Directory -Force "$root/artifacts", "$root/sources" | Out-Null
if (Test-Path -LiteralPath $SourceRepository) { $gitSource = (Resolve-Path $SourceRepository).Path }
else {
    $gitSource = "$root/artifacts/source-repository"
    if (!(Test-Path "$gitSource/HEAD")) {
        git clone --bare $SourceRepository $gitSource
        if ($LASTEXITCODE) { throw 'Source clone failed' }
    }
    if ($DefinitionFile) {
        git -C $gitSource fetch origin
        if ($LASTEXITCODE) { throw 'Source fetch failed' }
    }
}
foreach ($version in $versions.Keys) {
    # Distinct revisions must never reuse compiler/Razor caches: archive timestamps
    # can be older than outputs from a previous experiment.
    $suffix = if ($definition.sourceVariants) { "-capture-isolation" } else { "" }
    $target = "$root/sources/$version-$($versions[$version])$suffix"
    New-Item -ItemType Directory -Force $target | Out-Null
    git -C $gitSource archive --format=tar --output="$root/artifacts/$version.tar" $versions[$version] components Directory.Build.props README.md LICENSE
    if ($LASTEXITCODE) { throw 'Source archive failed' }
    tar -xf "$root/artifacts/$version.tar" -C $target
    if ($LASTEXITCODE) { throw 'Source extraction failed' }
    if ($definition.sourceVariants) {
        python "$root/scripts/apply-capture-variant.py" $target $version "$root/artifacts/$version-source-audit.json"
        if ($LASTEXITCODE) { throw 'Variant audit failed' }
    }
    $project = (Resolve-Path "$target/components/AntDesign.csproj").Path
    New-Item -ItemType Directory -Force "$root/artifacts/harness-$version" | Out-Null
    Copy-Item "$root/src/Benchmarks/*" "$root/artifacts/harness-$version/" -Recurse -Force
    dotnet build "$root/artifacts/harness-$version/Benchmarks.csproj" -c Release --no-incremental -p:TargetFrameworks=net10.0 "-p:AntDesignSource=$project" -o "$root/artifacts/bin-$version" --verbosity quiet *> "$root/artifacts/build-$version.log"
    if ($LASTEXITCODE) { Get-Content "$root/artifacts/build-$version.log" -Tail 20; throw "Build failed: $version" }
    Write-Output "Built $version at $($versions[$version])"
}
