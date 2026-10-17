# Dictionary merger optimization protocol

This is a prospective follow-up to [the original experiment](../REPORT.md), with no changes to the original raw data.

## Revisions and design

- A / baseline: PR before optimization, `67486a17c31bab00665b617919b7ac99ad218468`.
- B / candidate: optimized PR, `af1f0210fb3a15c16f58f5c0349ba415d3a05dd3`.
- Same machine, Release, .NET 10, warmups (3), samples per process (4), paired blocks (12), seed, order balancing,
  headless renderer, forced-GC schedule, allocation counters, table datasets, and unchanged analysis method.
- No simultaneous timing processes. Measurements against the old PR are repeated now: we do not compare unrelated absolute times across runs.
- Retain all three no-extra-attribute sizes and operations; retain the legacy-only table case and candidate/legacy vs candidate/both integration case.
- Add a **both-1000x10** case: before vs after optimization with both dictionaries enabled and identical output attributes.
  This direct comparison is the main integration test for the optimization itself.
- Retain all original 18 microbenchmark combinations, loop counts, and core measurement loop.
  Add eight combinations: one input is an empty, non-null dictionary and the other has 1/4/8/16 entries.
  Both inputs are preallocated; new empty-input cases use 100,000 calls/sample in both arms.
- Total: 7 cases, **168 processes / 4,224 samples** (6 table cases × 24 processes × 12 samples,
  plus 24 microbenchmark processes × 26 combinations × 4 samples).
- Compare rendering work for all cross-version table pairs, not only the no-attribute cases.

## Implementation under test

The optimized helper reuses an input when the other is null or empty. It does not cache results or mutate inputs.
For a real merge, it preallocates `max(shared.Count, legacy.Count)` rather than the sum: overlap is common, and summing can
reserve excessive capacity for overlapping dictionaries. Disjoint dictionaries may still grow; the experiment retains them
so this tradeoff remains visible. Existing case-insensitive, legacy-wins precedence is retained.

Validation before timing: 3054 .NET 10 tests pass; netstandard2.1 component build succeeds.

## Reproduce

Run dotnet-invoking commands in an elevated shell on the original Windows machine.

```powershell
./scripts/prepare.ps1 -DefinitionFile protocols/merge-optimization.json
./scripts/run.ps1 -DefinitionFile protocols/merge-optimization.json -OutputName windows-net10-optimization-run2
python scripts/verify.py results/windows-net10-optimization-run2
python scripts/analyze.py results/windows-net10-optimization-run2
```

`analyze.py` is unchanged. Results and conclusions are published after all scheduled cases complete.
Small-loop timings include the shared delegate/loop overhead; null/one-input cases are checked for regressions as well as gains.
The previous headless/single-machine/.NET-version limitations continue to apply. No issue or PR comment is posted.
