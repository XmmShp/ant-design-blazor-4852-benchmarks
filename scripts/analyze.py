"""Analyze independent paired processes, rather than treating within-process samples as independent."""
import argparse
import collections
import csv
import json
import math
from pathlib import Path
import random
import statistics as stats

parser = argparse.ArgumentParser()
parser.add_argument('directory', type=Path)
args = parser.parse_args()
rng = random.Random(4852)
groups = collections.defaultdict(dict)
raw_rows = []
for path in sorted(args.directory.glob('*.jsonl')):
    case, rest = path.stem.split('-pair')
    pair, arm = rest.split('-')
    process_groups = collections.defaultdict(list)
    for line in path.read_text(encoding='utf-8-sig').splitlines():
        row = json.loads(line)
        if row['kind'] != 'sample':
            continue
        row.update(case=case, pair=int(pair), arm=arm, file=path.name)
        raw_rows.append(row)
        key = (case, row['operation'], row.get('scenario', ''), row.get('size', 0))
        process_groups[key].append(row)
    for key, rows in process_groups.items():
        groups[key][(int(pair), arm)] = {
            'ns': stats.median(r['elapsedNs'] for r in rows),
            'bytes': stats.median(r['allocatedBytes'] for r in rows),
            'batches': stats.median(r.get('batches', 0) for r in rows),
            'updatedComponents': stats.median(r.get('updatedComponents', 0) for r in rows),
            'edits': stats.median(r.get('edits', 0) for r in rows),
        }

summary = []
for key, processes in sorted(groups.items()):
    case, operation, scenario, size = key
    pairs = sorted({p for p, a in processes if a == 'a'} & {p for p, a in processes if a == 'b'})
    if not pairs:
        continue
    ratios = [processes[(p, 'b')]['ns'] / processes[(p, 'a')]['ns'] for p in pairs]
    # Process medians reduce single-render scheduling outliers; paired geometric
    # means and paired bootstrap preserve order blocks and between-process noise.
    logs = [math.log(r) for r in ratios]
    boots = sorted(math.exp(stats.mean(rng.choices(logs, k=len(logs)))) for _ in range(20000))
    record = dict(case=case, operation=operation, scenario=scenario, size=size, pairs=len(pairs),
        baseline_median_ns=stats.median(processes[p, 'a']['ns'] for p in pairs),
        candidate_median_ns=stats.median(processes[p, 'b']['ns'] for p in pairs),
        paired_ratio=math.exp(stats.mean(logs)), ci95_low=boots[500], ci95_high=boots[19499],
        baseline_median_bytes=stats.median(processes[p, 'a']['bytes'] for p in pairs),
        candidate_median_bytes=stats.median(processes[p, 'b']['bytes'] for p in pairs),
        min_pair_ratio=min(ratios), max_pair_ratio=max(ratios))
    for metric in ['batches', 'updatedComponents', 'edits']:
        record[metric + '_match'] = all(processes[p, 'a'][metric] == processes[p, 'b'][metric] for p in pairs)
    summary.append(record)

if not summary:
    raise SystemExit('No paired results')
(args.directory / 'summary.json').write_text(json.dumps(summary, indent=2) + '\n', encoding='utf-8')
with (args.directory / 'summary.csv').open('w', newline='', encoding='utf-8') as f:
    writer = csv.DictWriter(f, fieldnames=list(summary[0]))
    writer.writeheader()
    writer.writerows(summary)
with (args.directory / 'samples.csv').open('w', newline='', encoding='utf-8') as f:
    writer = csv.DictWriter(f, fieldnames=sorted({k for r in raw_rows for k in r}))
    writer.writeheader()
    writer.writerows(raw_rows)

lines = ['| Case | Operation / merge case | A | B | B/A (95% paired bootstrap CI) | Allocated bytes A → B |',
         '| --- | --- | ---: | ---: | --- | ---: |']
for row in summary:
    micro = row['case'] == 'micro'
    scale = 1 if micro else 1_000_000
    unit = 'ns' if micro else 'ms'
    label = row['operation'] if not micro else f"{row['scenario']} ({row['size']} entries per dictionary)"
    lines.append(f"| {row['case']} | {label} | {row['baseline_median_ns']/scale:.3f} {unit} | "
                 f"{row['candidate_median_ns']/scale:.3f} {unit} | {row['paired_ratio']:.4f} "
                 f"[{row['ci95_low']:.4f}, {row['ci95_high']:.4f}] | "
                 f"{row['baseline_median_bytes']:,.0f} → {row['candidate_median_bytes']:,.0f} |")
(args.directory / 'summary.md').write_text('\n'.join(lines) + '\n', encoding='utf-8')
print('\n'.join(lines))
