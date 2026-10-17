# Capacity strategy and single-input fast paths: results (2026-10-17)

This is a fresh, paired comparison of the previous optimization against the follow-up. All seven cases and 26 micro scenarios are retained, including the eight-entry disjoint allocation regression, single-input paths, tiny/overlapping merges and empty inputs. Numerical effects and their uncertainty are shown together; this is not a claim of a universal speedup.

Both primary targets improve in this run: **8+8 disjoint allocations fall 1,248 → 608 B/call (−51.28%)**, with paired time −22.98%; the four-entry shared-only path falls about **4.97 → 3.64 ns**, paired time −26.62%. Both timing intervals exclude zero. All eight two-nonempty-input cases allocate less and have timing intervals below zero. The remaining tradeoff is non-null empty-input paths: global medians rise about 0.2–0.7 ns while staying zero-allocation. Equivalent-output tables allocate less, but their timing improvements are inconclusive.

| Arm | Production commit |
| --- | --- |
| A: previous optimization | `af1f0210fb3a15c16f58f5c0349ba415d3a05dd3` |
| B: capacity/null-path follow-up | `2b1cafd50978e1622b68c45aba7ca572506aa5d4` |

Measured harness and prospective protocol commit: `ab4afd14b45fa03292af923b0d8c0de6bb9f7c29`.
Protocol: [capacity-optimization.md](protocols/capacity-optimization.md), [machine-readable definition](protocols/capacity-optimization.json).

## Implementation and correctness

- Return the other input immediately if either input is null, before any `Count` read. Keep this entry method short and move non-null merging into a separate private method.
- For two nonempty inputs, estimate capacity as the sum of counts minus legacy keys found in the shared input. This avoids using the larger input count as a reservation for a disjoint union, while accounting for overlapping keys without constructing a temporary set.
- Use concrete `Dictionary<string, object>` struct enumerators for the estimate and copies, avoiding the interface-enumerator allocations on this common path. Arbitrary `IReadOnlyDictionary` inputs keep the general fallback.
- Output remains ordinal case-insensitive, legacy values win, inputs are not mutated and later input mutations are observed on the next call. No merge result is cached.

The estimate uses the source's comparer; case variants or a different/custom comparer can make it differ from the actual output count. It affects reservation only; normal dictionary growth preserves correctness. The microbenchmark uses concrete default-comparer dictionaries, so its timing results do not establish performance for all dictionary implementations or comparers.

One internal reference-identity edge changes relative to A: empty shared input plus null legacy input now returns the empty shared input rather than null, restoring the behavior before the first optimization. Both render no attributes. Tests explicitly cover the null-first ordering. Four added regression cases check eight-entry disjoint/overlapping inputs, differing comparers and concrete/read-only wrappers.

**3,058 .NET 10 tests pass**; netstandard2.1 builds with zero errors and 198 existing warnings. Production commit `2b1cafd` is pushed to `feat/4852-additional-attributes` in [PR #4855](https://github.com/ant-design-blazor/ant-design-blazor/pull/4855). No issue or PR comment was posted.

The [GitHub checks snapshot](results/windows-net10-capacity-run3/github-checks.json) at 07:10 UTC records successful CLA and preview builds, a main build still in progress, and a failed `start preview info` job with annotation `Repository access blocked`. These remote statuses are separate from the completed local tests/build; this report does not claim all remote checks passed.

## Primary helper results and remaining tradeoffs

The following list highlights the requested cases and empty-input controls; all 26 combinations appear below. `shared-only` means a nonempty shared input and null legacy input. `legacy-only` is the reverse. `shared-empty` means shared is an empty, non-null dictionary and legacy has N entries; `legacy-empty` is the reverse. Overlap means all keys overlap. Displayed ns figures are global medians; paired effect estimates are computed separately.

- `both-disjoint`, 8: 581.37 → 441.26 ns/call; 1,248 → 608 B/call; paired time -22.98% [-25.88%, -19.88%].
- `both-overlap`, 8: 572.93 → 493.29 ns/call; 552 → 440 B/call; paired time -12.93% [-16.38%, -8.95%].
- `both-disjoint`, 1: 121.34 → 95.59 ns/call; 328 → 216 B/call; paired time -21.10% [-24.47%, -17.76%].
- `both-overlap`, 1: 127.09 → 100.13 ns/call; 328 → 216 B/call; paired time -20.22% [-23.31%, -16.22%].
- `shared-only`, 4: 4.97 → 3.64 ns/call; 0 → 0 B/call; paired time -26.62% [-27.95%, -25.44%].
- `legacy-only`, 4: 3.78 → 3.67 ns/call; 0 → 0 B/call; paired time -3.74% [-7.04%, -0.28%].
- `both-empty`, 0: 5.03 → 5.57 ns/call; 0 → 0 B/call; paired time +11.84% [+8.37%, +15.40%].
- `shared-empty`, 4: 4.96 → 5.51 ns/call; 0 → 0 B/call; paired time +8.70% [+3.28%, +14.02%].
- `legacy-empty`, 4: 6.67 → 6.91 ns/call; 0 → 0 B/call; paired time +5.61% [+3.66%, +7.72%].

The measured change combines capacity estimation, traversal changes and null-path separation. The experiment does not isolate the contribution of each edit. Loop, delegate and sink overhead are included; sub-nanosecond differences should be assessed as absolute costs as well as percentages. Empty-input paths remain zero-allocation; any small timing increases are retained in the full table rather than hidden.

## Equivalent-output 10,000-cell table: both dictionaries enabled

Both versions render the same attributes using two preallocated four-entry dictionaries with two overlapping keys. All render-work counters match sample by sample. This is the direct before/after test of the combined optimization.

| Case | Operation | Paired time change (95% CI) | Median allocated bytes A → B |
| --- | --- | --- | ---: |
| both-1000x10 | changed-data-update | -3.93% [-14.17%, +3.84%] | 72,465,172 → 67,985,200 |
| both-1000x10 | mount | -7.07% [-17.59%, +0.80%] | 225,414,042 → 222,053,482 |
| both-1000x10 | same-data-update | -5.50% [-15.86%, +3.05%] | 72,540,944 → 68,060,872 |

| Operation | Median reduction, MiB | Reduction relative to A |
| --- | ---: | ---: |
| mount | 3.205 | 1.49% |
| same-data-update | 4.273 | 6.18% |
| changed-data-update | 4.272 | 6.18% |

A logical render can call the helper multiple times. These are total process-allocation measurements for each operation, not a single merge. A timing interval that spans zero does not establish a table speedup even if the point estimate improves.

Mount saves **3.205 MiB (1.49%)**, and updates save about **4.27 MiB (6.18%)** each. Paired time estimates improve 3.93–7.07%, but all three intervals include zero change.

## No extra attributes, and legacy-only control

The no-attribute cases omit `AdditionalAttributes` entirely, including an explicit null. All cells are rendered.

| Case | Operation | Paired time change (95% CI) | Median allocated bytes A → B |
| --- | --- | --- | ---: |
| none-1000x10 | changed-data-update | +0.96% [-2.95%, +4.91%] | 29,567,768 → 29,567,840 |
| none-1000x10 | mount | +1.79% [-1.23%, +4.45%] | 178,855,942 → 178,856,128 |
| none-1000x10 | same-data-update | -0.68% [-4.13%, +2.36%] | 29,163,512 → 29,163,512 |
| none-100x10 | changed-data-update | -2.88% [-15.57%, +13.04%] | 2,977,840 → 2,977,840 |
| none-100x10 | mount | -3.36% [-12.10%, +6.67%] | 17,077,452 → 17,077,288 |
| none-100x10 | same-data-update | -4.26% [-16.92%, +11.75%] | 2,937,152 → 2,937,088 |
| none-2000x20 | changed-data-update | -0.54% [-3.45%, +2.56%] | 116,750,424 → 116,750,424 |
| none-2000x20 | mount | +0.07% [-2.48%, +3.19%] | 675,812,670 → 675,812,418 |
| none-2000x20 | same-data-update | +1.53% [-2.43%, +5.83%] | 114,989,544 → 114,989,544 |

| Case | Operation | Paired time change (95% CI) | Median allocated bytes A → B |
| --- | --- | --- | ---: |
| legacy-1000x10 | changed-data-update | -1.03% [-4.38%, +2.20%] | 54,863,648 → 54,863,648 |
| legacy-1000x10 | mount | +3.23% [-1.82%, +9.13%] | 212,192,684 → 212,193,256 |
| legacy-1000x10 | same-data-update | +1.40% [-2.47%, +5.27%] | 54,939,320 → 54,939,320 |

For the 10,000- and 40,000-cell no-attribute fixtures, paired estimates range from −0.68% to +1.79%; all six intervals contain zero change, with the largest upper bound +5.83%. Allocation differences are negligible. No consistent default-path regression was observed in these fixtures, which does not establish zero overhead.

This comparison is **A af1f021 vs B 2b1cafd in the same run**, not a direct comparison against original master or the initial PR. Descriptive intervals do not prove equivalence or zero cost; no prespecified equivalence margin or multiple-comparison correction is introduced. Small-fixture intervals may be wide.

## All 26 helper scenarios

| Inputs | Entries per nonempty input | Median ns/call A → B | Paired time change (95% CI) | Bytes/call A → B |
| --- | ---: | ---: | --- | ---: |
| both-disjoint | 1 | 121.34 → 95.59 | -21.10% [-24.47%, -17.76%] | 328 → 216 |
| both-disjoint | 4 | 347.02 → 226.37 | -33.79% [-36.06%, -30.93%] | 968 → 440 |
| both-disjoint | 8 | 581.37 → 441.26 | -22.98% [-25.88%, -19.88%] | 1,248 → 608 |
| both-disjoint | 16 | 1104.04 → 831.95 | -27.39% [-33.53%, -23.21%] | 1,808 → 1,168 |
| both-empty | 0 | 5.03 → 5.57 | +11.84% [+8.37%, +15.40%] | 0 → 0 |
| both-null | 0 | 3.05 → 2.93 | -3.09% [-7.90%, +2.02%] | 0 → 0 |
| both-overlap | 1 | 127.09 → 100.13 | -20.22% [-23.31%, -16.22%] | 328 → 216 |
| both-overlap | 4 | 307.08 → 276.08 | -9.91% [-14.28%, -5.47%] | 440 → 328 |
| both-overlap | 8 | 572.93 → 493.29 | -12.93% [-16.38%, -8.95%] | 552 → 440 |
| both-overlap | 16 | 1157.15 → 981.04 | -9.73% [-14.59%, -4.29%] | 720 → 608 |
| legacy-empty | 1 | 6.58 → 6.91 | +6.27% [+4.47%, +8.10%] | 0 → 0 |
| legacy-empty | 4 | 6.67 → 6.91 | +5.61% [+3.66%, +7.72%] | 0 → 0 |
| legacy-empty | 8 | 6.66 → 6.95 | +5.01% [+1.19%, +8.65%] | 0 → 0 |
| legacy-empty | 16 | 6.72 → 7.02 | +1.14% [-8.09%, +10.67%] | 0 → 0 |
| legacy-only | 1 | 3.90 → 3.62 | -5.42% [-8.21%, -2.47%] | 0 → 0 |
| legacy-only | 4 | 3.78 → 3.67 | -3.74% [-7.04%, -0.28%] | 0 → 0 |
| legacy-only | 8 | 3.83 → 3.64 | -5.92% [-9.20%, -2.42%] | 0 → 0 |
| legacy-only | 16 | 3.91 → 3.64 | -7.03% [-12.32%, -2.55%] | 0 → 0 |
| shared-empty | 1 | 4.90 → 5.59 | +12.22% [+8.15%, +16.45%] | 0 → 0 |
| shared-empty | 4 | 4.96 → 5.51 | +8.70% [+3.28%, +14.02%] | 0 → 0 |
| shared-empty | 8 | 4.85 → 5.58 | +14.37% [+11.89%, +16.85%] | 0 → 0 |
| shared-empty | 16 | 5.14 → 5.56 | +14.12% [+8.10%, +21.32%] | 0 → 0 |
| shared-only | 1 | 4.96 → 3.64 | -27.37% [-28.80%, -26.00%] | 0 → 0 |
| shared-only | 4 | 4.97 → 3.64 | -26.62% [-27.95%, -25.44%] | 0 → 0 |
| shared-only | 8 | 5.01 → 3.62 | -26.69% [-28.30%, -25.06%] | 0 → 0 |
| shared-only | 16 | 5.04 → 3.61 | -28.75% [-30.96%, -26.79%] | 0 → 0 |

## Remaining cost of enabling shared attributes

This case compares **candidate/legacy against candidate/both**, not the optimization before/after. It includes additional parameters/attributes and render-tree work as well as merging, and is retained to show the residual feature cost.

| Case | Operation | Paired time change (95% CI) | Median allocated bytes A → B |
| --- | --- | --- | ---: |
| merge-1000x10 | changed-data-update | +15.98% [+13.01%, +19.47%] | 54,863,652 → 67,985,200 |
| merge-1000x10 | mount | +6.51% [+3.46%, +9.63%] | 212,192,794 → 222,053,544 |
| merge-1000x10 | same-data-update | +13.31% [+10.66%, +16.14%] | 54,939,376 → 68,060,872 |

## Method, selection disclosure and limitations

The formal run contains **168 sequential independent processes / 4,224 measured samples**, twelve paired blocks, three warmups and four samples per process, alternating AB/BA and seed 4852. All scheduled samples are retained. The benchmark program, `run.ps1`, `prepare.ps1`, `verify.py` and `analyze.py` are unchanged from the prospective harness commit. All five equivalent-work table cases matched batches, component updates and edits sample by sample. Cell/header counts, changed text and shared-attribute counts are validated outside timing. Both library builds have stable, distinct SHA256s across the run, with matching runtime/OS/architecture/GC settings.

Two three-pair exploratory micro pilots preceded the prospective protocol and selected the implementation. All twelve pilot process files (1,248 samples) are included under `pilots/`, with the [first implementation patch and design notes](experiments/capacity-pilots.md). They are not pooled into the formal analysis. Pilot binaries came from the working tree; formal binaries were built afresh from revision-isolated archived commits with `--no-incremental`. Their assembly hashes can differ due to build metadata and source paths.

Analysis is unchanged: process medians, geometric mean of paired B/A ratios, 20,000 paired bootstrap resamples, no removed outliers and no treating within-process samples as independent replicates. The effect estimate is not the ratio of the displayed global medians. Confidence intervals are descriptive and not multiplicity-adjusted.

Same machine: i7-12700F (12 cores/20 logical), 63.81 GiB RAM, Windows build 26200, High performance power plan, default affinity; SDK 10.0.103, .NET 10.0.13, Release x64 workstation GC, tiered compilation and ReadyToRun disabled. Same actual AntDesign Table/Column and headless Blazor renderer; 100×10, 1,000×10 and 2,000×20 fully rendered cells, no virtualization/paging, `RerenderStrategy.Always`. Operations mount, resubmit the same data reference, then change values using the same row keys. Dictionary fixtures and delegate/reflection binding are outside timing. The actual helper is measured, not a reimplementation.

This is one desktop's .NET rendering/diffing work, excluding browser DOM/layout/paint, WASM and SignalR. Forced GC between operations differs from a live app. Do not extrapolate universal user-visible latency or compare absolute timings across experiment rounds.

## Public evidence and reproduction

[Raw data and summaries](results/windows-net10-capacity-run3/), [full statistics](results/windows-net10-capacity-run3/summary.md), [manifest](results/windows-net10-capacity-run3/manifest-all.json), [validation](results/windows-net10-capacity-run3/verification.json), [checksums](results/windows-net10-capacity-run3/SHA256SUMS) and path-redacted validation logs are public. Raw formal/pilot JSONL bytes are unchanged. The previous [optimization report](OPTIMIZATION.md) and [original PR report](REPORT.md), including their raw evidence, remain unchanged.

On the original Windows machine, dotnet-invoking commands must run in an elevated shell:

```powershell
./scripts/prepare.ps1 -DefinitionFile protocols/capacity-optimization.json
./scripts/run.ps1 -DefinitionFile protocols/capacity-optimization.json -Pairs 12 -Samples 4 -Warmups 3 -OutputName my-run
python scripts/verify.py results/my-run
python scripts/analyze.py results/my-run
```
