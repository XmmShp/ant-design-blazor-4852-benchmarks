"""Fail closed on missing processes, partial files, duplicate samples or mismatched rendering work."""
import json
from pathlib import Path
import sys

root = Path(sys.argv[1])
manifest = json.loads((root / 'manifest-all.json').read_text(encoding='utf-8-sig'))
cases = set(manifest.get('cases', ['none-100x10', 'none-1000x10', 'none-2000x20', 'legacy-1000x10', 'merge-1000x10', 'micro']))
equivalent_work_cases = set(manifest.get('equivalentWorkCases', [c for c in cases if c.startswith('none-')]))
expected = {f'{case}-pair{pair:02d}-{arm}.jsonl' for case in cases
            for pair in range(manifest['pairs']) for arm in ['a', 'b']}
actual = {p.name for p in root.glob('*.jsonl')}
assert actual == expected, f'Missing {expected - actual}; unexpected {actual - expected}'
count = 0
hashes = {'baseline': set(), 'candidate': set()}
no_attribute_work = {}
for name in sorted(expected):
    rows = [json.loads(line) for line in (root / name).read_text(encoding='utf-8-sig').splitlines()]
    assert rows[0]['kind'] == 'environment', name
    environment = rows[0]
    hashes[environment['version']].add(environment['librarySha256'])
    assert environment['tieredCompilation'] == '0' and environment['readyToRun'] == '0', name
    samples = rows[1:]
    assert all(r['kind'] == 'sample' for r in samples), name
    assert len(samples) == manifest['samplesPerProcess'] * (manifest.get('microScenarioCount', 18) if name.startswith('micro-') else 3), name
    identities = {(r['operation'], r.get('scenario'), r.get('size'), r['sample']) for r in samples}
    assert len(identities) == len(samples), name
    for row in samples:
        assert row['elapsedNs'] > 0 and row['allocatedBytes'] >= 0, name
        if name.startswith('micro-'):
            continue
        assert row['validatedCells'] == row['rows'] * row['columns'], name
        assert row['validatedHeaders'] == row['columns'], name
        assert row['batches'] > 0 and row['updatedComponents'] > 0, name
        if row['operation'] == 'same-data-update':
            assert row['edits'] == 0, name
        if row['operation'] == 'changed-data-update':
            assert row['edits'] > 0, name
    count += len(samples)
    if name.split('-pair')[0] in equivalent_work_cases:
        no_attribute_work[name] = [tuple(r[k] for k in ['operation', 'batches', 'updatedComponents', 'edits']) for r in samples]
for name, work in no_attribute_work.items():
    if name.endswith('-a.jsonl'):
        assert work == no_attribute_work[name[:-7] + 'b.jsonl'], f'Different rendering work: {name}'
assert all(len(values) == 1 for values in hashes.values()), hashes
assert hashes['baseline'] != hashes['candidate'], 'Different library binaries must be loaded'
print(json.dumps({'verifiedProcesses': len(expected), 'verifiedSamples': count,
                  'pairs': manifest['pairs'], 'librarySha256': {k: list(v)[0] for k, v in hashes.items()}}, indent=2))
