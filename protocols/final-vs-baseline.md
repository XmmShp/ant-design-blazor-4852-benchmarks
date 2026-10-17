# Final PR vs pre-PR baseline: prospective direct comparison

This is a new direct experiment, not an accumulation of the three earlier studies' ratios. Production source is unchanged.

- A / pre-PR baseline: `943f6af77201ba1ebfd4756c054ff838d8bef0d0`, the verified parent of initial PR commit `67486a17`.
- B / final PR: `2b1cafd50978e1622b68c45aba7ca572506aa5d4`.
- [Machine-readable definition](final-vs-baseline.json).

## Same fixtures, runner and statistics

The benchmark program, build/runner/validation/analysis scripts, same .NET 10 configuration, full table rendering, three operations, allocation counters, GC schedule, seed 4852, order balancing and process isolation are unchanged.
Twelve independent paired blocks, three warmups and four samples per process; **168 sequential fresh processes / 4,224 samples**. There are six table cases (24 processes × 12 samples each) and the existing 26-case microbenchmark (24 processes × 104 samples). No concurrent timing processes, sample exclusions, optional stopping or combining data across studies.
Per-process medians feed the geometric mean of paired B/A ratios and the unchanged 20,000-resample paired bootstrap. Descriptive 95% intervals are not multiplicity-adjusted; there is no prespecified equivalence margin. An interval including zero change does not establish zero overhead.

## Comparisons and interpretation fixed before sampling

1. **none-100x10, none-1000x10, none-2000x20:** baseline/none vs final/none. No `AdditionalAttributes` parameter is passed, including an explicit null. The 10,000- and 40,000-cell cases are the primary default-path comparisons. Each operation must validate its live cell/header counts and changed text, with batches, updated components and edit counts identical sample by sample across versions.
2. **legacy-1000x10:** baseline/legacy vs final/legacy. Retained sensitivity control. It is not strictly equivalent output: the existing fixture contains `title`, whose collision with managed column title has different precedence in the PR. Do not treat it as proof of equivalent-work performance.
3. **enabled-vs-baseline-1000x10:** baseline/legacy vs final/both. This replaces the preceding optimization study's both-enabled before/after case: the pre-PR baseline cannot accept `AdditionalAttributes`. Baseline has the existing preallocated four-entry `OnCell` dictionary; final adds a preallocated four-entry shared dictionary, with two overlapping keys. This is a direct version comparison of an enabled-feature workload, with additional attributes/parameters and different output; it is not an equivalent-functionality slowdown or isolated merger cost.
4. **merge-1000x10:** final/legacy vs final/both, unchanged same-version enabled-feature control. Keep it separate from item 3; it cannot answer the version-comparison question by itself.
5. **micro:** same 26 fixtures and iteration counts. Baseline has no shared-property/merge helper and uses the existing non-inlined return-existing-dictionary delegate. Final calls the actual internal helper through a cached delegate. In shared/merge scenarios baseline ignores the shared fixture, so their timing ratios are not regressions of equivalent functionality. Report final absolute ns/call and allocated bytes/call for those scenarios, not large B/A percentages as if both arms performed merging.

Input dictionaries are preconstructed outside timing. Thus the enabled fixtures do not measure constructing Blazor catch-all dictionaries from individually supplied unmatched Razor parameters. The unchanged test harness measures actual component lifecycle and render-tree diffing; it does not measure browser DOM/layout/paint, WASM or SignalR. One desktop/runtime and forced GC between operations do not represent every application or production GC schedule.

## Public evidence and draft

Publish all scheduled raw samples, manifest, complete statistics, validation output, run/build evidence and SHA256 checksums. Preserve all previous reports and raw data. Prepare a concise reply draft using this direct comparison only, explicitly distinguishing default-path and enabled-feature results. Do not post to the issue or PR.

No production changes or new pilot selection occur in this study. Previously recorded local validation of the final commit: 3,058 .NET 10 tests pass and netstandard2.1 builds successfully; those existing checks are not represented as rerun in this experiment.

## Reproduce

On the original Windows machine, all dotnet-invoking commands must run in an **elevated shell**:

```powershell
./scripts/prepare.ps1 -DefinitionFile protocols/final-vs-baseline.json
./scripts/run.ps1 -DefinitionFile protocols/final-vs-baseline.json -Pairs 12 -Samples 4 -Warmups 3 -OutputName windows-net10-final-vs-baseline-run4
python scripts/verify.py results/windows-net10-final-vs-baseline-run4
python scripts/analyze.py results/windows-net10-final-vs-baseline-run4
```

`prepare.ps1` extracts exact commits into revision-isolated source directories and builds with `--no-incremental` to prevent stale compiler/Razor output reuse.
