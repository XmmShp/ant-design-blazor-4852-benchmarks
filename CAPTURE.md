# Isolated CaptureUnmatchedValues A/B benchmark

Both arms use final production commit `2b1cafd50978e1622b68c45aba7ca572506aa5d4`. A disables the AntDomComponentBase capture flag; B retains the original enabled flag. This single Boolean is the only library source change. Merge code, Razor, parameter inputs and native output are the same. B/A > 1 means capture enabled is slower. The production branch and PR were not changed, and no issue/PR message was posted.

For default 10k-cell tables, the three point estimates range from -2.39% to +0.41%, all intervals spanning zero. For default 40k-cell tables, mount is +1.41% and changed-data update -1.42% (intervals spanning zero), while same-data update is +5.92% [95% CI +0.11%, +13.91%]; zero regression is therefore not established. Explicit shared-only mount/same/changed estimates are +0.60% / +1.52% / +3.33%; both-dictionary estimates are +4.36% / +5.31% / +6.76%, all with intervals spanning zero. Table allocation differences are at most 696 B (difference of medians of process medians), without additional allocation proportional to cell count. All 26 helper cases have identical bytes/call.

Tradeoff: retaining true preserves direct HTML-attribute syntax. This study does not show a consistent large slowdown on the default path, and explicit single-dictionary point estimates are modest, but the intervals still allow larger costs; the default 40k same-data update flags a possible real regression. Keeping the convenience remains a reasonable choice on this evidence, but extremely small overhead is not established. Actual capture of unknown parameters was not measured, so total cost of inline HTML attributes is not promised to equal this study. The small-table control and helper cases that bypass parameter dispatch also fluctuate; every difference must not be attributed to the capture algorithm.

## All table results

| Case | Operation | A false (ms) | B true (ms) | B/A change [95% CI] | Allocated bytes A → B |
|---|---|---:|---:|---:|---:|
| both-1000x10 | changed-data-update | 407.022 | 418.843 | +6.76% [-1.50%, +17.00%] | 67,985,188 → 67,985,272 |
| both-1000x10 | mount | 567.181 | 579.962 | +4.36% [-3.67%, +13.95%] | 222,053,416 → 222,053,696 |
| both-1000x10 | same-data-update | 407.105 | 419.980 | +5.31% [-3.58%, +16.38%] | 68,060,944 → 68,060,880 |
| legacy-1000x10 | changed-data-update | 346.794 | 348.321 | -0.33% [-7.30%, +7.23%] | 54,863,648 → 54,863,648 |
| legacy-1000x10 | mount | 491.770 | 507.230 | -0.19% [-5.07%, +4.41%] | 212,192,330 → 212,193,026 |
| legacy-1000x10 | same-data-update | 351.471 | 353.553 | +4.28% [-0.22%, +9.32%] | 54,939,320 → 54,939,320 |
| none-1000x10 | changed-data-update | 172.022 | 175.482 | +0.41% [-2.70%, +3.57%] | 29,567,912 → 29,567,912 |
| none-1000x10 | mount | 418.494 | 410.686 | -2.30% [-7.92%, +3.77%] | 178,856,078 → 178,856,080 |
| none-1000x10 | same-data-update | 148.395 | 142.960 | -2.39% [-9.25%, +4.14%] | 29,163,584 → 29,163,512 |
| none-100x10 | changed-data-update | 9.454 | 8.327 | -6.84% [-13.58%, -0.18%] | 2,977,840 → 2,977,840 |
| none-100x10 | mount | 21.587 | 19.852 | -6.89% [-12.38%, -1.93%] | 17,077,308 → 17,077,448 |
| none-100x10 | same-data-update | 9.621 | 8.652 | -6.37% [-14.01%, +1.32%] | 2,937,008 → 2,937,152 |
| none-2000x20 | changed-data-update | 715.460 | 708.978 | -1.42% [-6.25%, +3.46%] | 116,750,424 → 116,750,336 |
| none-2000x20 | mount | 1507.713 | 1544.367 | +1.41% [-2.36%, +5.35%] | 675,812,400 → 675,812,394 |
| none-2000x20 | same-data-update | 604.312 | 610.749 | +5.92% [+0.11%, +13.91%] | 114,989,544 → 114,989,616 |
| shared-1000x10 | changed-data-update | 354.327 | 359.704 | +3.33% [-3.93%, +10.84%] | 54,864,768 → 54,864,768 |
| shared-1000x10 | mount | 513.675 | 524.451 | +0.60% [-4.32%, +4.99%] | 212,214,182 → 212,214,496 |
| shared-1000x10 | same-data-update | 349.791 | 366.285 | +1.52% [-4.16%, +6.34%] | 54,940,440 → 54,940,440 |

The 12 paired changes for default 40k same-data update range from -7.41% to +48.27%. The +48.27% pair is retained as planned. Millisecond medians are 604.312 → 610.749 ms, whereas the paired geometric-mean estimate is +5.92%; these are different statistics and the variability is retained. See [process-pairs.csv](results/windows-net10-capture-isolation-run5/process-pairs.csv).

## Method

12 independent paired processes per case, balanced alternating AB/BA, case shuffle seed 4852; 3 warmups and 4 samples/process. 168 formal processes and 4224 sample records. Take each process median, then the geometric mean of 12 paired time ratios, with 20000 paired bootstrap resamples and 95% intervals. Milliseconds are medians of process medians, not the numerator/denominator of the paired ratio estimator or a paired absolute-delta estimate. An interval spanning 0% does not prove zero overhead. No outlier removal or multiple-comparison adjustment. Core timing/analysis are unchanged; this run adds output validation outside timing and a shared-only case, without pooling historical samples.

## Fixtures and scope

none supplies no AdditionalAttributes parameter, even null; legacy supplies only OnCell; shared explicitly passes a prebuilt four-entry AdditionalAttributes dictionary; both supplies the same four-entry AdditionalAttributes plus four-entry OnCell dictionary (two overlapping keys). Each A/B pair receives identical parameters. Fixtures are built outside timing and legacy precedence is unchanged. No unknown component parameters are passed, so this measures the flag-selected parameter dispatch path, excluding dictionary creation when actually capturing unknown HTML parameters. The 26 direct helper microcases are a negative control, bypass parameter dispatch, and their fluctuations cannot be attributed to the capture flag.

## Equivalence and provenance

All 1728 table operation records validate cell/header counts. Corresponding paired samples match native-output SHA256 and batch/component-update/edit counts. Fingerprints retain native element structure, attribute names/scalar values, text and markup; skip component parameters and framework IDs/reference captures; delegate values use their type. Delegate identity and browser behavior are not compared. Build logs redact only the local workspace path; raw samples retain their original bytes. 8 preflight processes / 24 operations are separate and excluded from statistics. Source audits compare all archived files before compilation: original archives match and only one true/false replacement differs. Actual loaded ParameterAttribute metadata is checked in every process.

## Limits

Windows / .NET 10 / one machine; real component lifecycle and render-tree diff, excluding browser DOM/layout, JS, network and WASM. Frequent small-table interactions, other parameter counts or actual unknown-attribute capture may differ. The disabled arm is an experimental control, not a compatibility-ready patch; removal of existing capture API requires separate review. Earlier final-versus-pre-PR feature costs of 7.5% / 15.9–17.1% involve different work and cannot all be attributed to this flag or decomposed by subtracting ratios between studies.

## Public evidence

- [Prospective protocol](protocols/capture-isolation.md) / [definition](protocols/capture-isolation.json) / [one-line library patch](experiments/capture-disabled.patch)
- [Raw records, manifest, source audits, build logs and checksums](results/windows-net10-capture-isolation-run5/)
- [All 44 table/helper estimates](results/windows-net10-capture-isolation-run5/summary.md) / [JSON](results/windows-net10-capture-isolation-run5/summary.json) / [samples CSV](results/windows-net10-capture-isolation-run5/samples.csv)
- [Validation](results/windows-net10-capture-isolation-run5/validation.json) / [source equivalence](results/windows-net10-capture-isolation-run5/source-equivalence.json) / [SHA256SUMS](results/windows-net10-capture-isolation-run5/SHA256SUMS)
- [Separate correctness preflight](results/capture-preflight/)
- [Earlier final vs pre-PR study](FINAL.md)
