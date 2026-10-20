"""Audit archive contents before build; change exactly one attribute Boolean in baseline."""
import hashlib,json,sys
from pathlib import Path
root,arm,out=Path(sys.argv[1]),sys.argv[2],Path(sys.argv[3])
files=[p for p in root.rglob('*') if p.is_file() and not {'bin','obj'}.intersection(p.relative_to(root).parts)]
before={p.relative_to(root).as_posix():hashlib.sha256(p.read_bytes()).hexdigest() for p in files}
p=root/'components/core/Base/AntDomComponentBase.cs'
s=p.read_bytes();old=b'[Parameter(CaptureUnmatchedValues = true)]';new=b'[Parameter(CaptureUnmatchedValues = false)]'
assert s.count(old)==1
if arm=='baseline': p.write_bytes(s.replace(old,new))
after={p.relative_to(root).as_posix():hashlib.sha256(p.read_bytes()).hexdigest() for p in files}
changed=[k for k in before if before[k]!=after[k]]
assert changed==(['components/core/Base/AntDomComponentBase.cs'] if arm=='baseline' else [])
out.write_text(json.dumps(dict(arm=arm,captureUnmatchedValues=arm=='candidate',changed=changed,before=before,after=after),indent=2)+'\n',encoding='utf-8')
