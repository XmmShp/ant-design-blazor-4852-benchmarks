# CaptureUnmatchedValues isolation (prospective)

Both arms archive production commit 2b1cafd50978e1622b68c45aba7ca572506aa5d4. A/baseline changes only AntDomComponentBase's `[Parameter(CaptureUnmatchedValues = true)]` to `false`. B/candidate keeps `true`. No production checkout or PR changes. Source audits hash every archived file before compilation, and identify the one changed file. The helper, Razor output, inputs, and harness are otherwise identical. Other capture parameters are untouched.

12 paired fresh sequential processes per case/arm, balanced alternating AB/BA, case shuffle seed 4852, 3 warmups and 4 samples. Same .NET10 headless renderer, measurements, runtime settings and paired bootstrap analysis as previous studies. Six table cases: none at 100x10, 1000x10, 2000x20; legacy-only, shared-only explicit AdditionalAttributes, and both dictionaries at 1000x10. The same 26 helper microcases serve as a negative control: they do not dispatch parameters. 168 processes / 4224 samples. No unmatched parameters are passed in either arm; this isolates dispatch overhead, not allocation for capturing unknown parameters.

Outside timing, compare native render output SHA256 and render batch/component/edit counts for every paired sample. Fingerprint contains native element structure, attribute names/scalar values, text/markup; skips component parameter frames, framework IDs/reference captures; delegate attribute values use their type. Actual loaded parameter metadata is recorded and checked. Preflight results are kept separately and never pooled. If scalar generated IDs differ, investigate before formal sampling rather than silently discarding differences.

Report B/A (capture enabled / disabled), absolute milliseconds, allocations and paired 95% bootstrap intervals (12 process medians, 20000 resamples). No outlier removal or invented acceptance threshold. Include all cases, uncertain estimates and headless/single machine limitations; no general browser/WASM claims. Publish immutable source, raw records, manifest, source audits, validation and SHA256 checksums. Preserve earlier studies unchanged.

Reproduce (PowerShell):
```
./scripts/prepare.ps1 -DefinitionFile protocols/capture-isolation.json
./scripts/run.ps1 -DefinitionFile protocols/capture-isolation.json -OutputName windows-net10-capture-isolation-run5
python scripts/verify.py results/windows-net10-capture-isolation-run5
python scripts/analyze.py results/windows-net10-capture-isolation-run5
```
