# Exploratory capacity pilots

Before fixing the formal capacity follow-up protocol, two exploratory implementations were each compared against production commit `af1f0210fb3a15c16f58f5c0349ba415d3a05dd3`.
Each pilot used three sequential old/new paired processes, order AB/BA/AB, three warmups, four samples and the unchanged 26-case microbenchmark. No formal table samples were collected or discarded during selection.

- v1: [patch against af1f021](capacity-pilot-v1.patch). Estimate capacity by subtracting keys found in the shared input from the sum of input counts; copy concrete dictionaries with struct enumerators. Check null inputs before accessing either count.
- v2: the same algorithm, with the null checks in the short entry method and non-null merge logic in a separate private method. This is the production implementation selected for the formal follow-up. The formal protocol pins its exact production SHA.

Both pilots suggested lower allocations for the eight-entry disjoint case, without sacrificing the overlapping-key allocation savings. The v2 pilot also suggested a faster shared-only path. These small exploratory samples motivated the choice; they are **not** pooled into the formal 12-pair analysis or used as its confidence intervals. All twelve pilot process files are published in the formal evidence directory's `pilots/` subdirectory, with original bytes and per-file SHA256 checksums.

The pilot candidate binaries were built from the live working tree before the production commit. Their assembly hashes can differ from the formal archived-source build due to build metadata/source paths. The recorded hashes identify each build; no equality of pilot/formal binary hashes is claimed.

The capacity estimate uses the source dictionary's comparer for `ContainsKey`, while the output always uses `OrdinalIgnoreCase`. For different comparers, case variants or duplicates within an input, the estimate need not be the exact output count; normal dictionary growth preserves correctness. It does not cache results or mutate inputs. Concrete dictionary traversal is optimized; arbitrary `IReadOnlyDictionary` implementations retain the interface enumeration fallback. The existing 26-case microbenchmark uses concrete default-comparer dictionaries and does not establish timing results for all dictionary implementations/comparers.

To reproduce the exploratory runner, build the unchanged benchmark harness against baseline af1f021 and the chosen candidate source (apply the v1 patch for v1), then run the following **in an elevated shell on the original Windows machine**:

```powershell
./scripts/run-capacity-pilot.ps1 -BaselineBinary /path/to/baseline/Benchmarks.dll -CandidateBinary /path/to/candidate/Benchmarks.dll -OutputPrefix artifacts/capacity-pilot
```

This script is a reusable transcription of the two inline pilot commands, with an additional overwrite guard. The formal study uses the unchanged `run.ps1` and `analyze.py`, not the pilot runner.
