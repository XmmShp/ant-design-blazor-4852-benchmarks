# Ant Design Blazor #4852 performance experiments

Public, reproducible experiments for [issue #4852](https://github.com/ant-design-blazor/ant-design-blazor/issues/4852)
and [PR #4855](https://github.com/ant-design-blazor/ant-design-blazor/pull/4855).

This repository measures **the proposed PR as committed**, without optimizing it first.
It does not post results to the issue or modify the PR.

## Fixed source revisions

| Arm | Commit |
| --- | --- |
| Baseline (parent of PR) | `943f6af77201ba1ebfd4756c054ff838d8bef0d0` |
| Candidate (PR) | `67486a17c31bab00665b617919b7ac99ad218468` |

The same benchmark source is compiled separately against each revision. `prepare.ps1` extracts exact committed source;
uncommitted production changes are never included. No changes to the library are applied for measurement.

## Reproduce

Requires .NET SDK 10.0.103 (or a compatible 10.0.1xx patch), PowerShell 7, Git, tar, and Python 3.
On the original Windows machine **all commands invoking dotnet must run in an elevated shell**.

```powershell
./scripts/prepare.ps1
./scripts/run.ps1 -Pairs 12 -Samples 4 -Warmups 3 -OutputName my-run
python scripts/analyze.py results/my-run
```

Optional: `prepare.ps1 -SourceRepository /path/to/existing/ant-design-blazor` avoids a network clone.
`run.ps1 -Only none-1000x10` selects one case. Existing sample files are never overwritten.

## Table experiment

- Actual AntDesign `Table<BenchRow>` and `Column<string>` components and Blazor `Renderer`.
- Release, .NET 10, workstation GC. Tiered compilation and ReadyToRun are disabled to avoid compilation/tiering drift.
- No browser: render-tree building, parameter processing, diffing and .NET lifecycle execute normally;
  display-batch application and JS calls are synchronous no-ops. No scrolling, virtualization, pagination, sorting, filtering or selection.
- All requested cells/headers are counted by traversing the live render trees, after each operation and outside timing.
  Changed-data updates must contain `B-0-0`; the merger case must contain a `data-shared` attribute on every cell/header.
- Mount includes component instantiation/DI and full table initialization. Data/service construction, validation, serialization,
  explicit GC and disposal are excluded. Unavoidable automatic GC within an operation is included and counted.
- Each measured sample mounts a fresh table, resubmits exactly the same data reference, then supplies a preconstructed
  changed-value array with identical row keys. `RerenderStrategy.Always` is explicit.
- `none` passes no arbitrary attributes or `AdditionalAttributes` parameter, not even an explicit null.
- `legacy` supplies a shared preallocated four-entry dictionary via `OnCell`; the callback never allocates.
- `both` adds a shared preallocated four-entry `AdditionalAttributes` dictionary to `legacy`; two keys overlap.
  It is compared against candidate/legacy, because baseline columns cannot accept this new parameter.
  This integration comparison includes extra parameter/attribute processing and larger render trees, not only merging.

## Merger microbenchmark

Calls the candidate's **actual internal `GetAdditionalAttributes`** through a cached open delegate.
Baseline calls a non-inlined delegate returning the preexisting dictionary (the old path).
No reflection, dictionary construction, fixtures, JSON or GC is inside the timed loop other than allocations by the actual helper.
Tests null/empty, legacy-only, shared-only, and two dictionaries with disjoint/overlapping keys (1/4/8/16 entries each).
Single/null paths use 2,000,000 calls/sample; allocating paths use 100,000. Results are ns/call and bytes/call.
An observable sink prevents elimination. Baseline has no equivalent shared-attribute support: these ratios express added helper
cost, not equivalent functionality. Absolute ns and bytes are the useful measurements for allocating cases.

## Sampling and limitations

Twelve independent paired blocks; each arm starts a fresh sequential process. Each process warms three fixture iterations and
records four samples. Case order is shuffled with seed 4852; A/B ordering alternates each block. Timed processes never run concurrently.
All warmups for table fixtures include a render-tree validation. OS CPU affinity is unchanged.

Analysis uses the median within each process, geometric mean of paired B/A ratios, and 20,000 paired bootstrap resamples
for a descriptive 95% interval. Within-process samples are **not** treated as independent replicates. No outliers are removed.
Results can detect a regression in these fixtures; an interval spanning 1 is not proof of equivalence or zero cost.
Intervals are not adjusted for multiple comparisons.

This is a headless .NET benchmark on one desktop, not a browser, WASM, SignalR, end-user latency or universal performance claim.
It uses the Blazor renderer infrastructure API, which may change between runtime versions.
Forced full GC between table operations makes retained-state conditions more reproducible but differs from a live application.
Absolute timings include the table library's existing initialization/render overhead; the merge helper is separately isolated.

## Results

Raw per-process JSONL, environment manifests, flattened samples, analysis and conclusions are published under `results/` after the run.
No credentials, computer name, home paths, private data or build logs are published. CPU model, runtime/OS versions and power plan are recorded.

## License

Benchmark source is MIT licensed. Ant Design Blazor is separately MIT licensed; extracted dependency sources remain ignored.
