# Dictionary merge optimization: measured results (2026-10-17)

Empty-input returns eliminate allocations, and capacity reservation reduces allocations for common overlapping dictionaries. This is **not a universal speedup**: disjoint 8+8 dictionaries allocate more, and tiny merges and some single-input paths are slower.
The equivalent-output 10,000-cell table allocates less; its timing improvement is inconclusive. No consistent no-attribute regression was observed in these fixtures, which does not prove zero overhead.

| Arm | Production commit |
| --- | --- |
| A: PR before optimization | `67486a17c31bab00665b617919b7ac99ad218468` |
| B: optimized PR | `af1f0210fb3a15c16f58f5c0349ba415d3a05dd3` |

Measured harness: `203ee86a7874a432e039e47174f4ea5a5536926d`. Protocol: [merge-optimization.md](protocols/merge-optimization.md), [machine-readable definition](protocols/merge-optimization.json).

## Change and validation

The helper returns the other input when one input is null or empty. For two nonempty inputs it reserves `max(shared.Count, legacy.Count)` entries, uses ordinal case-insensitive keys and retains legacy-wins precedence. It neither caches results nor mutates inputs. Reusing an input changes reference identity for empty-sided merges; later input mutations are read on the next invocation. Tests cover these behaviors.
The larger input count was chosen to avoid over-reserving when keys overlap. It still permits growth for disjoint keys; actual dictionary capacity growth can increase total allocation in some sizes, as measured below.

Production commit `af1f021` is pushed to branch `feat/4852-additional-attributes` of [PR #4855](https://github.com/ant-design-blazor/ant-design-blazor/pull/4855).
All **3,054 .NET 10 tests passed**; the netstandard2.1 build succeeded with 198 existing warnings and zero errors. No issue or PR comment was posted.

## Default path: no extra attributes

| Case | Operation | Paired time change (95% CI) | Median allocated bytes A → B |
| --- | --- | --- | ---: |
| none-1000x10 | changed-data-update | +1.03% [-0.93%, +3.04%] | 29,567,912 → 29,567,840 |
| none-1000x10 | mount | +0.76% [-1.83%, +3.75%] | 178,856,354 → 178,855,984 |
| none-1000x10 | same-data-update | +1.68% [-0.53%, +3.59%] | 29,163,584 → 29,163,584 |
| none-100x10 | changed-data-update | -2.70% [-14.63%, +9.40%] | 2,977,840 → 2,977,840 |
| none-100x10 | mount | +2.34% [-6.85%, +12.64%] | 17,077,406 → 17,077,390 |
| none-100x10 | same-data-update | -1.85% [-14.53%, +12.15%] | 2,937,008 → 2,937,152 |
| none-2000x20 | changed-data-update | -2.14% [-5.08%, +0.36%] | 116,750,424 → 116,750,424 |
| none-2000x20 | mount | -2.68% [-5.01%, -0.49%] | 675,813,584 → 675,812,476 |
| none-2000x20 | same-data-update | -2.86% [-4.65%, -1.19%] | 114,989,544 → 114,989,688 |

For 10,000 and 40,000 cells the paired estimates range from −2.86% to +1.68%; the largest upper confidence bound is +3.75%. The 40,000-cell mount and same-data update intervals are below zero in this run; no-attribute paths have no deliberate algorithmic improvement, so these observations do not establish a general speedup. Allocation differences are negligible relative to the totals.
The small 1,000-cell fixture has wider intervals (upper bounds up to +12.64%). These are descriptive intervals, without multiple-comparison adjustment or a prespecified equivalence margin.
This study compares **old PR vs optimized PR**, not optimized PR vs the original master revision. The original baseline comparison remains in the earlier report.

## Equivalent-output table: both dictionaries enabled

Both versions render identical attributes: two preallocated four-entry dictionaries per cell/header, with two overlapping keys.

| Case | Operation | Paired time change (95% CI) | Median allocated bytes A → B |
| --- | --- | --- | ---: |
| both-1000x10 | changed-data-update | -5.64% [-15.45%, +2.19%] | 77,905,104 → 72,465,272 |
| both-1000x10 | mount | -8.25% [-19.54%, +2.30%] | 229,494,608 → 225,414,148 |
| both-1000x10 | same-data-update | -4.47% [-17.49%, +8.46%] | 77,980,776 → 72,540,944 |

Mount saves about **3.89 MiB (1.78%)**; each update saves about **5.19 MiB (6.98%)**. Paired timing estimates improve 4.47–8.25%, but **all three intervals include zero change**. The run supports allocation savings, not a conclusive table speedup. A logical render may invoke the helper multiple times.

Legacy-only cross-version control:

| Case | Operation | Paired time change (95% CI) | Median allocated bytes A → B |
| --- | --- | --- | ---: |
| legacy-1000x10 | changed-data-update | -0.06% [-3.04%, +2.56%] | 54,863,648 → 54,863,576 |
| legacy-1000x10 | mount | -2.39% [-5.39%, +0.43%] | 212,193,116 → 212,193,116 |
| legacy-1000x10 | same-data-update | -0.12% [-3.01%, +2.63%] | 54,939,288 → 54,939,320 |

## Actual helper microbenchmark: all 26 cases

`legacy-empty` means legacy is an empty non-null dictionary and shared has N entries; `shared-empty` is the reverse. `*-only` means the other input is null. Overlap means all keys overlap. Timings include common delegate/loop/sink overhead; nanosecond differences and large percentages on very small baselines should be interpreted together.

| Inputs | Entries per nonempty input | Median ns/call A → B | Paired time change (95% CI) | Bytes/call A → B |
| --- | ---: | ---: | --- | ---: |
| both-disjoint | 1 | 108.25 → 118.78 | +10.39% [+7.85%, +12.92%] | 328 → 328 |
| both-disjoint | 4 | 374.28 → 347.96 | -5.48% [-8.23%, -2.67%] | 1,104 → 968 |
| both-disjoint | 8 | 586.65 → 577.48 | -0.76% [-2.34%, +0.75%] | 1,104 → 1,248 |
| both-disjoint | 16 | 1156.39 → 1039.30 | -9.54% [-10.65%, -8.55%] | 2,192 → 1,808 |
| both-empty | 0 | 27.23 → 5.07 | -81.67% [-82.56%, -80.78%] | 80 → 0 |
| both-null | 0 | 2.97 → 3.19 | +4.98% [+1.39%, +8.30%] | 0 → 0 |
| both-overlap | 1 | 118.84 → 125.20 | +5.07% [+2.84%, +7.34%] | 328 → 328 |
| both-overlap | 4 | 337.29 → 310.94 | -7.40% [-8.51%, -6.30%] | 576 → 440 |
| both-overlap | 8 | 644.20 → 567.26 | -12.17% [-13.97%, -10.67%] | 1,104 → 552 |
| both-overlap | 16 | 1130.30 → 1069.12 | -7.65% [-9.98%, -5.72%] | 1,104 → 720 |
| legacy-empty | 1 | 75.77 → 6.56 | -91.26% [-91.43%, -91.06%] | 272 → 0 |
| legacy-empty | 4 | 190.35 → 6.52 | -96.51% [-96.67%, -96.33%] | 520 → 0 |
| legacy-empty | 8 | 355.22 → 6.47 | -98.26% [-98.42%, -98.14%] | 1,048 → 0 |
| legacy-empty | 16 | 559.47 → 6.67 | -98.83% [-98.89%, -98.77%] | 1,048 → 0 |
| legacy-only | 1 | 3.68 → 3.91 | +4.18% [+2.20%, +6.04%] | 0 → 0 |
| legacy-only | 4 | 3.73 → 3.91 | +2.78% [+0.67%, +4.69%] | 0 → 0 |
| legacy-only | 8 | 3.73 → 3.91 | +4.33% [+1.97%, +7.00%] | 0 → 0 |
| legacy-only | 16 | 3.71 → 3.90 | -1.40% [-7.83%, +3.84%] | 0 → 0 |
| shared-empty | 1 | 75.95 → 5.15 | -93.29% [-93.56%, -93.00%] | 272 → 0 |
| shared-empty | 4 | 188.20 → 5.13 | -97.34% [-97.42%, -97.25%] | 520 → 0 |
| shared-empty | 8 | 361.25 → 5.02 | -98.71% [-98.83%, -98.61%] | 1,048 → 0 |
| shared-empty | 16 | 560.53 → 5.11 | -99.12% [-99.18%, -99.07%] | 1,048 → 0 |
| shared-only | 1 | 3.72 → 4.92 | +31.34% [+28.56%, +34.25%] | 0 → 0 |
| shared-only | 4 | 3.69 → 4.88 | +32.48% [+30.57%, +34.52%] | 0 → 0 |
| shared-only | 8 | 3.71 → 4.98 | +33.25% [+31.22%, +35.49%] | 0 → 0 |
| shared-only | 16 | 3.70 → 4.87 | +20.61% [+0.25%, +34.79%] | 0 → 0 |

- Both empty: **80 → 0 B/call**, about 27.24 → 5.07 ns. One empty input: **272–1,048 → 0 B/call**, about 5–7 ns after optimization.
- Four entries per input: overlapping **576 → 440 B (−23.61%)**, paired time −7.40%; disjoint **1,104 → 968 B (−12.32%)**, paired time −5.48%. Both timing intervals exclude zero.
- Eight disjoint entries per input: **1,104 → 1,248 B (+13.04%)**; timing interval includes zero. Reserving eight initially produces a different growth sequence from the previous default constructor. Reserving the larger input count is not optimal for all union sizes.
- One entry per input: allocation remains 328 B; paired time worsens **+5.07% overlap / +10.39% disjoint**, with intervals above zero.
- Shared-only/null-legacy paths remain zero-allocation but rise from about **3.7 to 4.9 ns** in global medians; reading `Count` before returning adds work. Both-null and most legacy-only cases also show small sub-nanosecond differences. These counterexamples remain in the report and raw evidence.

## Remaining cost when enabling shared attributes

This is optimized/legacy vs optimized/both, **not before vs after optimization**, and includes additional parameter and attribute processing as well as merging.

| Case | Operation | Paired time change (95% CI) | Median allocated bytes A → B |
| --- | --- | --- | ---: |
| merge-1000x10 | changed-data-update | +12.07% [+6.61%, +17.10%] | 54,863,648 → 72,465,272 |
| merge-1000x10 | mount | +5.91% [+0.83%, +10.87%] | 212,193,090 → 225,414,094 |
| merge-1000x10 | same-data-update | +11.52% [+6.56%, +16.65%] | 54,939,176 → 72,540,944 |

Enabling shared attributes still costs +5.91% mount, +11.52% same-data update, and +12.07% changed-data update in paired estimates; all intervals are above zero. Added allocations are about 12.61 MiB on mount and 16.79 MiB per update. Optimization reduces the merge overhead without eliminating the cost of enabling the feature.

## Method and limits

**168 sequential fresh processes / 4,224 measured samples**, 12 paired blocks, three warmups and four samples per process, alternating AB/BA and seed 4852. All scheduled cases are retained. All five equivalent-work table cases match batches, updated components and edits sample by sample; actual live cell/header counts, changed text and shared-attribute counts are validated outside timing. Each arm consistently loaded one distinct library SHA256; runtime/OS/architecture/GC settings matched throughout.

Same real AntDesign Table/Column and headless Blazor renderer, fixtures, forced-GC schedule, operations, counters and analysis as the original experiment. Added only a direct both-enabled comparison and eight empty-sided micro cases. Per-process medians feed geometric means of paired B/A ratios and 20,000 paired bootstrap resamples; these ratios are not ratios of the displayed global medians. Within-process samples are not independent replicates. No outliers were removed. `analyze.py` is unchanged.

Machine: i7-12700F (12 cores/20 logical), 63.81 GiB RAM, Windows build 26200, High performance power plan, OS-default affinity; SDK 10.0.103, runtime .NET 10.0.13, Release x64 workstation GC, tiered compilation and ReadyToRun disabled. Inputs are preallocated; actual helper calls use a cached open delegate with reflection outside timing. Tables fully render 100×10, 1,000×10 and 2,000×20 cells without virtualization/paging; operations are mount, same-reference update and changed-value/same-key update with `RerenderStrategy.Always`.

This measures .NET rendering and diffing on one desktop. It excludes browser DOM/layout/paint, WASM and SignalR. Forced GC between operations differs from a live app. Do not infer universal user-visible latency or extrapolate absolute timings across the two experiments.

## Evidence and reproduction

[All raw samples and summaries](results/windows-net10-optimization-run2/), [full statistics](results/windows-net10-optimization-run2/summary.md), [environment/source manifest](results/windows-net10-optimization-run2/manifest-all.json), [validation](results/windows-net10-optimization-run2/verification.json), and [SHA256SUMS](results/windows-net10-optimization-run2/SHA256SUMS) are public.
Raw JSONL files are unchanged. Build/test logs only replace local home/workspace paths; the checksum file covers the published bytes.
The original experiment and its raw evidence remain unchanged: [English](REPORT.md), [中文](REPORT.zh-CN.md).

Run dotnet-invoking commands in an elevated shell on the original Windows machine:

```powershell
./scripts/prepare.ps1 -DefinitionFile protocols/merge-optimization.json
./scripts/run.ps1 -DefinitionFile protocols/merge-optimization.json -Pairs 12 -Samples 4 -Warmups 3 -OutputName my-run
python scripts/verify.py results/my-run
python scripts/analyze.py results/my-run
```

Use harness commit `203ee86` for the exact measured runner; the publication commit additionally strengthens environment validation without changing the runner, benchmark loop or analysis. Source extraction is isolated by revision and compiled with `--no-incremental` to prevent timestamp/cache reuse.
