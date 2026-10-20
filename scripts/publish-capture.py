"""Package completed isolation evidence without modifying raw samples or earlier studies."""
from pathlib import Path
import datetime,hashlib,json,subprocess
r=Path(__file__).resolve().parents[1]
d=r/'results/windows-net10-capture-isolation-run5'
validation=subprocess.check_output(['python',str(r/'scripts/verify.py'),str(d)])
(d/'validation.json').write_bytes(validation)
for arm in ['baseline','candidate']:
    (d/f'{arm}-source-audit.json').write_bytes((r/f'artifacts/{arm}-source-audit.json').read_bytes())
    log=(r/f'artifacts/build-{arm}.log').read_text(encoding='utf-8-sig')
    log=log.replace(str(r.parent.parent),'[WORKSPACE]').replace(str(r.parent.parent).replace('\\','/'),'[WORKSPACE]')
    (d/f'build-{arm}.log').write_text(log,encoding='utf-8')
a=json.loads((d/'baseline-source-audit.json').read_text(encoding='utf-8'));b=json.loads((d/'candidate-source-audit.json').read_text(encoding='utf-8'))
assert a['before']==b['before']
assert [k for k in a['after'] if a['after'][k]!=b['after'][k]]==['components/core/Base/AntDomComponentBase.cs']
archive_hashes={arm:hashlib.sha256((r/f'artifacts/{arm}.tar').read_bytes()).hexdigest() for arm in ['baseline','candidate']}
assert archive_hashes['baseline']==archive_hashes['candidate']
(d/'source-equivalence.json').write_text(json.dumps(dict(archivedFiles=len(a['before']),archiveSha256=archive_hashes,identicalOriginalArchives=True,changedFiles=['components/core/Base/AntDomComponentBase.cs'],onlyReplacement='CaptureUnmatchedValues = true -> false (baseline only)',sourceCommit='2b1cafd50978e1622b68c45aba7ca572506aa5d4'),indent=2)+'\n',encoding='utf-8')
harness={}
for name in ['Program.cs','Benchmarks.csproj']:
    hashes={arm:hashlib.sha256((r/f'artifacts/harness-{arm}/{name}').read_bytes()).hexdigest() for arm in ['baseline','candidate']}
    assert len(set(hashes.values()))==1
    assert hashes['baseline']==hashlib.sha256((r/f'src/Benchmarks/{name}').read_bytes()).hexdigest()
    harness[name]=hashes
(d/'harness-source-equivalence.json').write_text(json.dumps(harness,indent=2)+'\n',encoding='utf-8')
(d/'completed.json').write_text(json.dumps(dict(completedUtc=datetime.datetime.now(datetime.timezone.utc).isoformat(),validation=json.loads(validation)),indent=2)+'\n',encoding='utf-8')
for folder in [d,r/'results/capture-preflight']:
    lines=[hashlib.sha256(f.read_bytes()).hexdigest()+'  '+f.relative_to(folder).as_posix() for f in sorted(folder.rglob('*')) if f.is_file() and f.name!='SHA256SUMS']
    (folder/'SHA256SUMS').write_text('\n'.join(lines)+'\n',encoding='utf-8')
print(validation.decode())
