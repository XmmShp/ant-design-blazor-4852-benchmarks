# Performance report: Ant Design Blazor #4852 / PR #4855

Measured 2026-10-17. This reports the committed PR as it stands, without optimizing the implementation first.

## Conclusions

1. **No statistically resolved default-path timing regression was observed in the two large, no-extra-attribute fixtures.**
   Paired point estimates range from −0.78% to +1.13%; all six 95% intervals include no change.
   This is **not proof of zero overhead or equivalence**: the largest upper bound is +5.39%.
   Mount allocation rises by about 78 KiB for 10,000 cells and 313 KiB for 40,000 cells (about 0.05% of total mount allocation).
   Update allocation is essentially unchanged. Thus “every cell allocates a new dictionary even when unused” is not supported by these measurements.
2. **Dictionary merging has a measurable allocation cost when both inputs are non-null.**
   Four entries in each input cost about 342–370 ns and 576–1,104 allocated bytes per call, depending on key overlap.
   Either input being null takes a zero-allocation fast path. Two empty dictionaries still allocate 80 bytes/call.
3. **Using both dictionaries throughout a 10,000-cell table has a measurable integration cost.**
   Relative to candidate/legacy-only, candidate/both takes +13% to +18% more time and allocates about 16.5 MiB more on mount
   and 22 MiB more on each update. This includes larger parameter/attribute processing and render trees, not just merging.

## Revisions, machine and protocol

- Baseline: [`943f6af77201ba1ebfd4756c054ff838d8bef0d0`](https://github.com/ant-design-blazor/ant-design-blazor/commit/943f6af77201ba1ebfd4756c054ff838d8bef0d0).
- Candidate: [`67486a17c31bab00665b617919b7ac99ad218468`](https://github.com/ant-design-blazor/ant-design-blazor/commit/67486a17c31bab00665b617919b7ac99ad218468).
- Measured harness source: commit `89b61901893f156ce18a0cccfd2048f3b5fe5c61`; later report/validation files do not alter the measured binaries.
- Intel Core i7-12700F, 12 cores / 20 logical processors, approximately 63.81 GiB usable RAM, Windows build 26200, High performance power plan.
- SDK 10.0.103, runtime .NET 10.0.13, x64, Release, workstation GC; tiered compilation and ReadyToRun disabled; no affinity pinning.
- 12 independent paired blocks; each arm is a new sequential process; three warmup iterations and four measured samples/process.
- Six cases, 144 processes, **3,168 measured samples**, all retained. No outlier removal.
- Within-process medians; geometric mean of paired B/A ratios; descriptive 95% paired-bootstrap intervals, 20,000 resamples, fixed seed 4852.
- All no-attribute pairs have identical batch, updated-component and edit counts, sample by sample.
- Actual `Table`/`Column` lifecycle and renderer diffing execute. The display sink and JS are no-ops. Validation confirms all cells/headers exist
  and changed text is rendered. No pagination, virtualization, scrolling, selection, filtering or sorting is enabled.

Absolute times vary during a desktop run. **The paired effect estimate is not the ratio of the two unpaired/global medians** displayed
in the detailed summary. Confidence intervals describe this experiment's variability; they do not establish behavior on other machines.
Small 1,000-cell controls have wider intervals and are included in the full results rather than used to support equivalence.

## 1. Large tables with no extra attributes

A = baseline, B = candidate. No `AdditionalAttributes` parameter is passed, not even null.

| Rows × columns | Operation | Paired time change B/A − 1 (95% CI) | Median allocated bytes A → B |
| --- | --- | --- | ---: |
| 1,000 × 10 | mount | +0.19% [-2.25%, +2.65%] | 178,776,298 → 178,856,144 |
| 1,000 × 10 | same-data-update | +0.11% [-3.66%, +4.11%] | 29,163,584 → 29,163,584 |
| 1,000 × 10 | changed-data-update | +1.13% [-0.88%, +3.20%] | 29,567,912 → 29,567,912 |
| 2,000 × 20 | mount | -0.78% [-6.24%, +3.54%] | 675,492,052 → 675,812,680 |
| 2,000 × 20 | same-data-update | +0.07% [-5.84%, +5.39%] | 114,989,416 → 114,989,544 |
| 2,000 × 20 | changed-data-update | -0.29% [-4.50%, +3.57%] | 116,750,280 → 116,750,424 |

The observed small mount-allocation increase is consistent with component object layout changes; allocation-stack profiling was not performed,
so this is an interpretation rather than a proven allocation breakdown. Existing `TableRow` already had catch-all support before this PR,
and table cells already used attribute splatting for `OnCell`/`OnHeaderCell`. This experiment measures the **incremental PR change**.

## 2. The actual merger method

The candidate's internal `GetAdditionalAttributes` is bound once to an open delegate and invoked directly.
Fixtures/input dictionaries are preconstructed. Timings include the shared loop/delegate/sink overhead; no reflection is inside the loop.
Bytes/call measure allocations, not retained heap size or peak process memory.

| Inputs | Entries per input | Candidate ns/call | Candidate bytes/call |
| --- | ---: | ---: | ---: |
| Both null | 0 | 2.90 | 0 |
| Legacy only | 4 | 3.69 | 0 |
| Shared only | 4 | 3.68 | 0 |
| Both non-null but empty | 0 | 27.65 | 80 |
| Two dictionaries, all keys overlap | 4 | 342.34 | 576 |
| Two dictionaries, disjoint keys | 4 | 369.59 | 1,104 |
| Two dictionaries, all keys overlap | 16 | 1181.71 | 1,104 |
| Two dictionaries, disjoint keys | 16 | 1167.16 | 2,192 |

For illustration, 10,000 four-entry merges allocate about **5.49 MiB** with fully overlapping keys or **10.53 MiB** with disjoint keys.
This is per 10,000 **calls**, not a promise of one call per cell per logical table update.

Baseline has no equivalent shared-attribute feature. Its microbenchmark is simply the old return-existing-dictionary path;
large microbenchmark B/A ratios in the detailed summary must not be described as regressions of equivalent functionality.
The absolute candidate ns/call and bytes/call above are the useful quantities.

## 3. Integration: explicitly using both dictionaries

Both arms use the candidate binary and 1,000 rows × 10 columns.
A uses a preallocated four-entry `OnCell` dictionary. B additionally supplies a preallocated four-entry `AdditionalAttributes` dictionary;
two keys overlap. The callback and input-dictionary construction do not allocate inside measured operations.

| Operation | Candidate both vs candidate legacy time change (95% CI) | Extra median allocation per operation |
| --- | --- | ---: |
| mount | +18.32% [+14.77%, +22.32%] | 17,301,592 B (16.50 MiB) |
| same-data-update | +13.36% [+10.51%, +16.89%] | 23,041,528 B (21.97 MiB) |
| changed-data-update | +14.36% [+11.66%, +17.36%] | 23,041,540 B (21.97 MiB) |

This comparison intentionally exercises additional functionality and a larger attribute set. It is an **enabled-feature cost**, not a default-path regression.
Blazor's lifecycle can render cells more than once per update; consequently the isolated per-call number must not be multiplied by cell count
and treated as the complete integration cost.

The supplemental `legacy-1000x10` baseline/candidate case is also published. Its dictionary includes `title`, whose collision with the managed
column title changes precedence in the PR. Therefore its output is not strictly equivalent across revisions; it is a sensitivity check, not
an additional proof of default-path equivalence. The no-attribute primary comparisons have no such dictionary/collision difference.

## Interpretation and next discussion

The [Microsoft guidance](https://learn.microsoft.com/en-us/aspnet/core/blazor/performance/rendering?view=aspnetcore-10.0#avoid-attribute-splatting-with-captureunmatchedvalues)
identifies a real scaling risk. These results do not justify saying that catch-all support is free, but also do not show a large default-path
slowdown in this PR on this runtime.

The clearest next optimization target is the actual merging path: avoid merging empty inputs, investigate sizing the destination dictionary,
and remeasure against a new pinned commit. Caching only by dictionary reference is unsafe because input dictionaries can mutate in place.
Do not remove the shared API solely on the basis of these results; consider real application/browser/WASM workloads before broader claims.

**No issue/PR comment was posted and the production PR was not modified as part of this experiment.**

## Evidence and reproduction

- [Methods and commands](README.md)
- [Raw per-process files and environment manifest](results/windows-net10-run1/)
- [Full effect/allocation table](results/windows-net10-run1/summary.md)
- [All flattened samples](results/windows-net10-run1/samples.csv)
- [Machine-readable statistics](results/windows-net10-run1/summary.json)
- [Validation result](results/windows-net10-run1/verification.json)
- [Evidence checksums](results/windows-net10-run1/SHA256SUMS)

```powershell
./scripts/prepare.ps1
./scripts/run.ps1 -Pairs 12 -Samples 4 -Warmups 3 -OutputName my-run
python scripts/verify.py results/my-run
python scripts/analyze.py results/my-run
```

On the original Windows host, all dotnet-invoking commands run elevated. The benchmark runs on one desktop and one .NET version;
it does not measure browser DOM/layout/paint, WASM execution, SignalR serialization/network, GC settings for other hosts, or application-specific interactions.
Explicit full GC between table operations differs from a production GC schedule. Intervals are not corrected for multiple comparisons.
