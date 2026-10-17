# Direct comparison: final PR vs pre-PR baseline (2026-10-17)

This study directly pairs the final implementation against the version before the PR. It does not multiply or add the effects from the three previous studies. Production code remains unchanged at `2b1cafd` in [PR #4855](https://github.com/ant-design-blazor/ant-design-blazor/pull/4855).

With no extra attributes, the six large-table timing estimates range from **−4.15% to −0.88%**. No timing regression was resolved in these fixtures; four intervals include zero change and two are below zero (one only narrowly). This is not proof of zero overhead or a universal speedup: the largest upper confidence bound is **+1.58%**. Mount allocations rise about **0.05%**, while update allocations are essentially unchanged.

In the enabled-attribute fixture, final/both versus pre-PR/legacy costs **+7.51% mount, +15.89% same-data update and +17.06% changed-data update**, all with intervals above zero. Allocations rise **9.480 MiB on mount and 12.514 MiB per update**. These are costs of different feature/output workloads, not equivalent-functionality regression percentages or pure merger overhead.

| Arm | Production commit |
| --- | --- |
| A: pre-PR baseline (parent of initial PR commit) | `943f6af77201ba1ebfd4756c054ff838d8bef0d0` |
| B: final PR | `2b1cafd50978e1622b68c45aba7ca572506aa5d4` |

Measured harness/prospective protocol commit: `4c57fef4e61f68d8718574321196a32a5d86971d`.
Protocol: [final-vs-baseline.md](protocols/final-vs-baseline.md), [machine-readable definition](protocols/final-vs-baseline.json).

## No additional attributes: direct default-path comparison

No `AdditionalAttributes` parameter is passed, including an explicit null. Every requested cell/header is rendered, with no paging or virtualization. The primary fixtures contain 10,000 and 40,000 cells, each measured for mount, same-reference update and changed-value/same-key update.

| Case | Operation | Paired time change B/A − 1 (95% CI) | Median allocated bytes A → B |
| --- | --- | --- | ---: |
| none-1000x10 | changed-data-update | -4.15% [-7.08%, -0.88%] | 29,567,912 → 29,567,912 |
| none-1000x10 | mount | -3.68% [-8.47%, +0.79%] | 178,775,602 → 178,855,958 |
| none-1000x10 | same-data-update | -4.05% [-9.33%, +1.58%] | 29,163,584 → 29,163,584 |
| none-2000x20 | changed-data-update | -1.86% [-4.03%, -0.0031%] | 116,750,424 → 116,750,356 |
| none-2000x20 | mount | -0.88% [-2.54%, +0.77%] | 675,492,206 → 675,812,854 |
| none-2000x20 | same-data-update | -2.08% [-4.41%, +0.26%] | 114,989,472 → 114,989,544 |

All these comparisons validate the actual live render trees and match batches, updated-component counts and edits sample by sample. Descriptive intervals do not prove equivalence or zero overhead, and no equivalence margin was prespecified. Allocations are total managed allocations during an operation, not retained heap or peak memory.

The 10,000-cell mount adds 80,356 bytes (78.47 KiB, 0.04495%) and the 40,000-cell mount adds 320,648 bytes (313.13 KiB, 0.04747%). Update differences are zero or tens of bytes against totals of tens/hundreds of MiB. The changed-data upper bound for 40,000 cells is shown at extra precision because ordinary two-decimal formatting would display negative zero. No multiple-comparison correction was applied, so the narrow interval below zero is not a basis for a general speedup claim.

The smaller 1,000-cell control is also retained:

| Case | Operation | Paired time change B/A − 1 (95% CI) | Median allocated bytes A → B |
| --- | --- | --- | ---: |
| none-100x10 | changed-data-update | -1.62% [-9.27%, +7.32%] | 2,977,840 → 2,977,840 |
| none-100x10 | mount | +0.21% [-7.33%, +8.84%] | 17,069,140 → 17,077,486 |
| none-100x10 | same-data-update | -0.87% [-10.89%, +10.76%] | 2,937,152 → 2,937,152 |

## Additional attributes enabled: direct version comparison

The baseline has no shared `AdditionalAttributes` parameter for these columns, so it cannot perform exactly the same feature workload. In this direct comparison, A uses the existing preallocated four-entry `OnCell` dictionary; B uses that dictionary plus a preallocated four-entry `AdditionalAttributes` dictionary. Two keys overlap.
This measures the final enabled-feature workload against the pre-PR existing-attribute workload. Output and parameter/attribute processing differ, including the documented title-precedence change. It is not an equivalent-functionality regression or a measurement of dictionary merging alone.

| Case | Operation | Paired time change B/A − 1 (95% CI) | Median allocated bytes A → B |
| --- | --- | --- | ---: |
| enabled-vs-baseline-1000x10 | changed-data-update | +17.06% [+11.61%, +24.17%] | 54,863,648 → 67,985,272 |
| enabled-vs-baseline-1000x10 | mount | +7.51% [+3.31%, +11.48%] | 212,112,836 → 222,053,718 |
| enabled-vs-baseline-1000x10 | same-data-update | +15.89% [+12.06%, +19.20%] | 54,939,308 → 68,060,932 |

| Operation | Median allocation change B − A, MiB | Change relative to A |
| --- | ---: | ---: |
| changed-data-update | +12.514 | +23.917% |
| mount | +9.480 | +4.687% |
| same-data-update | +12.514 | +23.884% |

The benefit is consistent forwarding of accessibility, test and native HTML attributes to component targets; those are functionality benefits, not a performance gain. Using the feature still does additional work. The observed total version difference should not be attributed solely to either capture, merging or splatting.

## Actual final merge-helper costs

These are the final implementation's absolute costs, measured through a cached delegate to the real internal method. Dictionary construction, fixtures and reflection binding are outside timing; loop/delegate/sink overhead is included. One operation can call the merger multiple times, so per-call costs are not one-call-per-cell integration estimates.

| Final helper inputs | Entries per nonempty input | Final median ns/call | Final B/call |
| --- | ---: | ---: | ---: |
| both-disjoint | 1 | 96.99 | 216 |
| both-disjoint | 4 | 228.77 | 440 |
| both-disjoint | 8 | 432.28 | 608 |
| both-disjoint | 16 | 838.19 | 1,168 |
| both-empty | 0 | 5.57 | 0 |
| both-null | 0 | 2.90 | 0 |
| both-overlap | 1 | 100.63 | 216 |
| both-overlap | 4 | 281.02 | 328 |
| both-overlap | 8 | 502.77 | 440 |
| both-overlap | 16 | 993.06 | 608 |
| legacy-empty | 1 | 7.09 | 0 |
| legacy-empty | 4 | 6.93 | 0 |
| legacy-empty | 8 | 6.96 | 0 |
| legacy-empty | 16 | 7.00 | 0 |
| legacy-only | 1 | 3.73 | 0 |
| legacy-only | 4 | 3.72 | 0 |
| legacy-only | 8 | 3.66 | 0 |
| legacy-only | 16 | 3.67 | 0 |
| shared-empty | 1 | 5.58 | 0 |
| shared-empty | 4 | 5.63 | 0 |
| shared-empty | 8 | 5.59 | 0 |
| shared-empty | 16 | 5.74 | 0 |
| shared-only | 1 | 3.68 | 0 |
| shared-only | 4 | 3.67 | 0 |
| shared-only | 8 | 3.70 | 0 |
| shared-only | 16 | 3.67 | 0 |

`shared-only` means nonempty shared input/null legacy input; `legacy-only` is the reverse. `shared-empty` means shared is a non-null empty dictionary and legacy contains N entries; `legacy-empty` is the reverse. Overlap means all keys overlap.

The pre-PR arm has no equivalent shared-property/merge helper. It uses the existing non-inlined return-existing-dictionary delegate and ignores shared fixtures. Therefore the full raw summary's B/A micro ratios in shared/merge scenarios must not be interpreted as regressions of equivalent functionality. Final absolute ns/B per call are the useful quantities for those scenarios. Ordinary default-comparer dictionaries are timed; arbitrary dictionary/comparer performance is not established.

The return-path control is slightly cheaper than the final helper even when no merging is required: both-null is about **2.77 → 2.90 ns**, and four-entry legacy-only about **3.47 → 3.72 ns**; both remain zero-allocation. These include delegate/loop overhead and are not measurements of Blazor's capture machinery or application latency. Avoiding a merged dictionary does not mean literally zero CPU cost.

The final implementation returns the existing input for null/empty cases, estimates union capacity for nonempty merges, and uses struct enumerators on ordinary dictionary paths. It retains case-insensitive output and legacy-value precedence, does not mutate inputs, and does not cache by reference. For tradeoffs measured against the intermediate implementation (including small increases on non-null empty-input timings), see the historical [capacity study](CAPACITY.md); those intermediate-vs-final percentages are not combined with this study.

## Supplemental controls, kept separate

### Legacy-only across versions

| Case | Operation | Paired time change B/A − 1 (95% CI) | Median allocated bytes A → B |
| --- | --- | --- | ---: |
| legacy-1000x10 | changed-data-update | +0.40% [-4.34%, +4.80%] | 54,863,648 → 54,863,504 |
| legacy-1000x10 | mount | +3.53% [-2.26%, +9.20%] | 212,112,714 → 212,192,818 |
| legacy-1000x10 | same-data-update | +1.72% [-2.95%, +6.45%] | 54,939,248 → 54,939,176 |

This fixture contains `title`, whose collision with managed column title changes precedence in the PR. It is not strictly equivalent output and is not additional proof of default-path equivalence.

### Final implementation only: legacy vs both

| Case | Operation | Paired time change B/A − 1 (95% CI) | Median allocated bytes A → B |
| --- | --- | --- | ---: |
| merge-1000x10 | changed-data-update | +14.66% [+10.61%, +19.61%] | 54,863,648 → 67,985,272 |
| merge-1000x10 | mount | +7.13% [+3.11%, +11.39%] | 212,193,040 → 222,053,784 |
| merge-1000x10 | same-data-update | +10.52% [+7.64%, +14.54%] | 54,939,320 → 68,060,944 |

Both arms here are the final binary. This is the within-final enabled-feature control, not the direct version comparison above. Its percentages cannot substitute for final-vs-pre-PR results.

## Methods, validation and limits

**168 independent sequential processes / 4,224 measured samples**; twelve paired blocks, three warmups and four measured samples per process, alternating AB/BA, shuffled case order and seed 4852. All scheduled samples are retained; no outliers are removed and no data are pooled with the earlier studies. Process medians feed the geometric mean of paired B/A ratios and the unchanged 20,000-resample paired bootstrap. Within-process samples are not independent replicates, and the paired estimate is not the ratio of displayed global medians. Confidence intervals are descriptive and not adjusted for multiple comparisons.

Actual AntDesign Table/Column and Blazor renderer lifecycle/diffing execute. Cell/header counts, updated text and shared-attribute counts are validated outside timing. All three equivalent-output no-attribute cases match render-work counters sample by sample; each binary has a stable, distinct library SHA256, and runtime/OS/architecture/GC settings match throughout. The prospective benchmark program and build/runner/validation/analysis scripts are unchanged.

Both libraries were built afresh from exact revision-isolated source archives with `--no-incremental`. Existing final-commit validation (3,058 passing .NET 10 tests and a successful netstandard2.1 build) is documented in the [previous report](CAPACITY.md); it was not rerun or relabeled as a new regression-test run here, since production code did not change.

Same machine/configuration: i7-12700F (12 cores/20 logical), 63.81 GiB RAM, Windows build 26200, High performance power plan and OS-default affinity; SDK 10.0.103, runtime .NET 10.0.13, Release x64 workstation GC, tiered compilation and ReadyToRun disabled. `RerenderStrategy.Always`, fully rendered 100×10, 1,000×10 and 2,000×20 cells, no virtualization/paging. Explicit GC is outside timing; automatic GC occurring within an operation is included.

This is one desktop/runtime's headless .NET rendering/diffing work. Display-batch application and JS are no-ops; browser DOM/layout/paint, WASM and SignalR are excluded. Forced GC between operations differs from production behavior. Enabled fixtures reuse preconstructed explicit dictionaries, so they do not measure creating catch-all dictionaries from individually supplied unmatched Razor parameters. Do not infer zero cost or universal user-visible latency, or extrapolate absolute timings between studies.

## Public evidence and reproduction

[All raw data and summaries](results/windows-net10-final-vs-baseline-run4/), [full statistics](results/windows-net10-final-vs-baseline-run4/summary.md), [manifest](results/windows-net10-final-vs-baseline-run4/manifest-all.json), [validation](results/windows-net10-final-vs-baseline-run4/verification.json) and [checksums](results/windows-net10-final-vs-baseline-run4/SHA256SUMS) are public. JSONL bytes remain unchanged; build logs only replace local home/workspace paths. The previous three reports and their evidence remain unchanged. No issue or PR comment was posted.

On the original Windows machine, dotnet-invoking commands must run in an elevated shell:

```powershell
./scripts/prepare.ps1 -DefinitionFile protocols/final-vs-baseline.json
./scripts/run.ps1 -DefinitionFile protocols/final-vs-baseline.json -Pairs 12 -Samples 4 -Warmups 3 -OutputName my-run
python scripts/verify.py results/my-run
python scripts/analyze.py results/my-run
```
