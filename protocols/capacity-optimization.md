# Capacity strategy and single-input fast paths: prospective protocol

Follow-up to [the first optimization](../OPTIMIZATION.md), with its evidence unchanged.

- A / baseline: `af1f0210fb3a15c16f58f5c0349ba415d3a05dd3` (max input-count reservation).
- B / candidate: `2b1cafd50978e1622b68c45aba7ca572506aa5d4` (overlap-aware capacity estimate, concrete-dictionary copy paths, short null-input entry method).
- [Machine-readable definition](capacity-optimization.json): exactly the same seven cases, datasets, operations, 26 micro scenarios, iteration counts, three warmups, four samples and twelve paired process blocks as the previous optimization study.
- **168 sequential fresh processes / 4,224 measured samples**, order balancing and seed 4852 unchanged. No simultaneous timing processes. No exclusions, optional stopping or replacement of slow samples.
- Same SDK/runtime, build configuration, power plan, headless renderer, forced-GC schedule, counter scope, validation and unchanged `analyze.py`.
- Five direct cross-version table cases must match batches, updated components and edits sample by sample. Live render trees must have the expected cells/headers, updated text and shared attributes. The remaining integration case compares candidate/legacy against candidate/both; it is not the optimization's before/after effect.

Primary targets are the actual helper's **8+8 disjoint** allocations/timing, single-input/null-other paths, and the identical-output **both-1000x10** table case. All other cases, including empty inputs and tiny/overlapping dictionaries, remain visible to detect transferred regressions.
The default/no-attribute 10,000- and 40,000-cell table comparisons remain the guard against regression in common use. A confidence interval containing zero change does not establish equivalence; no multiplicity adjustment or equivalence margin is introduced.

## Implementation and semantic details

The entry method returns the other dictionary immediately when either input is null, without reading `Count`; the non-null merge is in a separate private method. Non-null empty inputs still return the other input without allocating. In the edge case shared is empty and legacy is null, this returns the empty shared input (as before the first optimization), instead of null in af1f021. Both render no attributes. The reference-reuse tests explicitly cover this ordering.

For two nonempty inputs, start with the sum of counts and subtract legacy keys found by the shared input's `ContainsKey`. This estimates the union without constructing a temporary set. The source comparer may differ from the output's ordinal case-insensitive comparer; this affects reservation only, and normal dictionary growth preserves correctness. Ordinary `Dictionary<string, object>` uses its struct enumerator for the estimate and copies; arbitrary `IReadOnlyDictionary` implementations use the interface fallback. Existing precedence, case-insensitive output, input immutability and mutation visibility are retained. Results are not cached.

Four added regression cases cover disjoint/overlapping eight-entry inputs, different input comparers, and concrete/read-only dictionaries. Full .NET 10 suite: **3,058 passing tests**; netstandard2.1 build: zero errors, 198 existing warnings.

The candidate was selected using [two disclosed exploratory micro pilots](../experiments/capacity-pilots.md), three paired blocks each. All pilot raw files will be published, but are not pooled with the prospective twelve formal pairs. The published pilot source patch identifies the first explored implementation. No formal samples were used for selection.

## Reproduce

On the original machine, run dotnet-invoking commands in an **elevated shell**:

```powershell
./scripts/prepare.ps1 -DefinitionFile protocols/capacity-optimization.json
./scripts/run.ps1 -DefinitionFile protocols/capacity-optimization.json -Pairs 12 -Samples 4 -Warmups 3 -OutputName windows-net10-capacity-run3
python scripts/verify.py results/windows-net10-capacity-run3
python scripts/analyze.py results/windows-net10-capacity-run3
```

The formal harness uses revision-isolated source extraction and full `--no-incremental` builds. All original headless/single-machine/.NET-version limitations apply. No issue or PR comment will be posted.
