# 容量策略与单字典快速路径：继续优化复测（2026-10-17）

本轮在同一次实验中重新配对比较上一版与新版本，保留相同的 7 组用例和 26 种微基准场景：8+8 不重叠键、单字典路径、小字典、重叠键和空字典都在结果中。结论需结合置信区间，不能概括为所有路径都加速。

两个重点均改善：**8+8 不重叠键分配从 1248 降到 608 字节/次（−51.28%）**，配对耗时 −22.98%；每份四项的仅共享字典路径约 **4.97 → 3.64 ns**，配对耗时 −26.62%，两者耗时区间均低于零。八种双方非空合并的分配都减少、耗时区间也都低于零。仍有取舍：双方非 null 但有空输入时，全局中位数增加约 0.2–0.7 ns，仍为零分配。相同输出的整表分配减少，但耗时改善尚不能确认。

| Arm | Production commit |
| --- | --- |
| A: previous optimization | `af1f0210fb3a15c16f58f5c0349ba415d3a05dd3` |
| B: capacity/null-path follow-up | `2b1cafd50978e1622b68c45aba7ca572506aa5d4` |

Measured harness and prospective protocol commit: `ab4afd14b45fa03292af923b0d8c0de6bb9f7c29`.
Protocol: [capacity-optimization.md](protocols/capacity-optimization.md), [machine-readable definition](protocols/capacity-optimization.json).

## 实现与正确性

- 任一输入为 null 时，立即返回另一份输入，不读取 `Count`。入口方法保持简短，非 null 的合并逻辑拆到私有方法。
- 两份非空输入先用条目总数减去在共享字典中找到的旧键，估算并集容量，避免不重叠时按较大输入长度预留仍需扩容，也避免直接按总长度预留导致重叠时浪费。不构造临时集合。
- 常见 `Dictionary<string, object>` 的容量估算和复制采用结构体枚举器，避免接口枚举器分配；其他 `IReadOnlyDictionary` 保留通用路径。
- 输出仍大小写不敏感、旧属性值优先；不改输入、不缓存合并结果，后续调用能观察到输入修改。

容量估算使用输入字典的比较器。大小写变体、同一输入内的变体键或自定义比较器可能使估算与实际输出长度不同；这只影响预留容量，必要时字典仍会扩容，结果正确性不依赖估算完全准确。现有微基准使用普通默认比较器的 `Dictionary`，不代表所有字典实现和比较器的性能。

一个内部引用身份边界与上一版不同：共享字典为空、旧字典为 null 时，现在返回空共享字典，而非上一版的 null，恢复首次优化前的行为。两者均不渲染属性；测试已明确覆盖 null 优先返回的顺序。另加四组回归检查，覆盖八条目重叠/不重叠、不同输入比较器、普通字典和只读包装。

**3058 项 .NET 10 测试通过**；netstandard2.1 构建 0 错误、198 个已有警告。代码 `2b1cafd` 已推送到 `feat/4852-additional-attributes`，更新 [PR #4855](https://github.com/ant-design-blazor/ant-design-blazor/pull/4855)。没有向 issue 或 PR 发送评论。

[远端检查快照](results/windows-net10-capacity-run3/github-checks.json)截至 07:10 UTC：CLA 和预览构建成功，主构建仍在运行；`start preview info` 任务失败，注释为 `Repository access blocked`。这些远端状态与已完成的本地测试/构建分开记录，不声称远端检查已全部通过。

## 目标场景与空字典对照

以下列出目标场景和空输入对照，完整 26 种组合见后表。`shared-only` 表示只有共享字典非空、旧字典为 null；`legacy-only` 相反。`shared-empty` 表示共享字典是非 null 的空字典、旧字典有 N 项；`legacy-empty` 相反。`overlap` 表示全部键重叠。绝对纳秒是全局中位数，配对效果另行计算。

- `both-disjoint`, 8: 581.37 → 441.26 ns/call; 1,248 → 608 B/call; paired time -22.98% [-25.88%, -19.88%].
- `both-overlap`, 8: 572.93 → 493.29 ns/call; 552 → 440 B/call; paired time -12.93% [-16.38%, -8.95%].
- `both-disjoint`, 1: 121.34 → 95.59 ns/call; 328 → 216 B/call; paired time -21.10% [-24.47%, -17.76%].
- `both-overlap`, 1: 127.09 → 100.13 ns/call; 328 → 216 B/call; paired time -20.22% [-23.31%, -16.22%].
- `shared-only`, 4: 4.97 → 3.64 ns/call; 0 → 0 B/call; paired time -26.62% [-27.95%, -25.44%].
- `legacy-only`, 4: 3.78 → 3.67 ns/call; 0 → 0 B/call; paired time -3.74% [-7.04%, -0.28%].
- `both-empty`, 0: 5.03 → 5.57 ns/call; 0 → 0 B/call; paired time +11.84% [+8.37%, +15.40%].
- `shared-empty`, 4: 4.96 → 5.51 ns/call; 0 → 0 B/call; paired time +8.70% [+3.28%, +14.02%].
- `legacy-empty`, 4: 6.67 → 6.91 ns/call; 0 → 0 B/call; paired time +5.61% [+3.66%, +7.72%].

此次测量的是容量估算、遍历方式和快速路径拆分的组合效果，不能分别归因到某一行代码。微基准包含循环、委托和结果存储成本；亚纳秒变化应同时看绝对耗时和百分比。空输入仍为零分配，若有小幅耗时反例，也完整保留。

## 两份字典都启用：相同输出的 1 万单元格表格

两侧均使用两份预分配的四条目字典、两个键重叠，输出属性相同，渲染工作量计数逐样本一致。这才是优化前后的直接整表比较。

| Case | Operation | Paired time change (95% CI) | Median allocated bytes A → B |
| --- | --- | --- | ---: |
| both-1000x10 | changed-data-update | -3.93% [-14.17%, +3.84%] | 72,465,172 → 67,985,200 |
| both-1000x10 | mount | -7.07% [-17.59%, +0.80%] | 225,414,042 → 222,053,482 |
| both-1000x10 | same-data-update | -5.50% [-15.86%, +3.05%] | 72,540,944 → 68,060,872 |

| Operation | Median reduction, MiB | Reduction relative to A |
| --- | ---: | ---: |
| mount | 3.205 | 1.49% |
| same-data-update | 4.273 | 6.18% |
| changed-data-update | 4.272 | 6.18% |

一次逻辑渲染可能调用多次合并，以上分配是整次操作的进程总分配。耗时点估计即使改善，只要区间跨过零变化，也不能确认整表加速。

首次渲染少分配 **3.205 MiB（1.49%）**，每次更新少约 **4.27 MiB（6.18%）**。配对耗时点估计下降 3.93%–7.07%，但三个区间均包含零变化。

## 不传额外属性及仅旧属性接口

不传属性的用例完全省略 `AdditionalAttributes` 参数，连显式 null 都不传，表格全量渲染。

| Case | Operation | Paired time change (95% CI) | Median allocated bytes A → B |
| --- | --- | --- | ---: |
| none-1000x10 | changed-data-update | +0.96% [-2.95%, +4.91%] | 29,567,768 → 29,567,840 |
| none-1000x10 | mount | +1.79% [-1.23%, +4.45%] | 178,855,942 → 178,856,128 |
| none-1000x10 | same-data-update | -0.68% [-4.13%, +2.36%] | 29,163,512 → 29,163,512 |
| none-100x10 | changed-data-update | -2.88% [-15.57%, +13.04%] | 2,977,840 → 2,977,840 |
| none-100x10 | mount | -3.36% [-12.10%, +6.67%] | 17,077,452 → 17,077,288 |
| none-100x10 | same-data-update | -4.26% [-16.92%, +11.75%] | 2,937,152 → 2,937,088 |
| none-2000x20 | changed-data-update | -0.54% [-3.45%, +2.56%] | 116,750,424 → 116,750,424 |
| none-2000x20 | mount | +0.07% [-2.48%, +3.19%] | 675,812,670 → 675,812,418 |
| none-2000x20 | same-data-update | +1.53% [-2.43%, +5.83%] | 114,989,544 → 114,989,544 |

| Case | Operation | Paired time change (95% CI) | Median allocated bytes A → B |
| --- | --- | --- | ---: |
| legacy-1000x10 | changed-data-update | -1.03% [-4.38%, +2.20%] | 54,863,648 → 54,863,648 |
| legacy-1000x10 | mount | +3.23% [-1.82%, +9.13%] | 212,192,684 → 212,193,256 |
| legacy-1000x10 | same-data-update | +1.40% [-2.47%, +5.27%] | 54,939,320 → 54,939,320 |

1 万和 4 万单元格默认路径的配对耗时点估计为 −0.68% 至 +1.79%，六个区间均包含零变化，最大上界 +5.83%，分配基本不变。本轮未观察到一致的默认路径退化，不等于证明零开销。

这是本轮 **af1f021 与 2b1cafd** 的同轮比较，不是与原始 master 或初始 PR 的直接对比。描述性置信区间不证明零开销或等效性；未预设等效性边界，也未做多重比较校正，小表格的区间可能较宽。

## 完整 26 种微基准场景

| Inputs | Entries per nonempty input | Median ns/call A → B | Paired time change (95% CI) | Bytes/call A → B |
| --- | ---: | ---: | --- | ---: |
| both-disjoint | 1 | 121.34 → 95.59 | -21.10% [-24.47%, -17.76%] | 328 → 216 |
| both-disjoint | 4 | 347.02 → 226.37 | -33.79% [-36.06%, -30.93%] | 968 → 440 |
| both-disjoint | 8 | 581.37 → 441.26 | -22.98% [-25.88%, -19.88%] | 1,248 → 608 |
| both-disjoint | 16 | 1104.04 → 831.95 | -27.39% [-33.53%, -23.21%] | 1,808 → 1,168 |
| both-empty | 0 | 5.03 → 5.57 | +11.84% [+8.37%, +15.40%] | 0 → 0 |
| both-null | 0 | 3.05 → 2.93 | -3.09% [-7.90%, +2.02%] | 0 → 0 |
| both-overlap | 1 | 127.09 → 100.13 | -20.22% [-23.31%, -16.22%] | 328 → 216 |
| both-overlap | 4 | 307.08 → 276.08 | -9.91% [-14.28%, -5.47%] | 440 → 328 |
| both-overlap | 8 | 572.93 → 493.29 | -12.93% [-16.38%, -8.95%] | 552 → 440 |
| both-overlap | 16 | 1157.15 → 981.04 | -9.73% [-14.59%, -4.29%] | 720 → 608 |
| legacy-empty | 1 | 6.58 → 6.91 | +6.27% [+4.47%, +8.10%] | 0 → 0 |
| legacy-empty | 4 | 6.67 → 6.91 | +5.61% [+3.66%, +7.72%] | 0 → 0 |
| legacy-empty | 8 | 6.66 → 6.95 | +5.01% [+1.19%, +8.65%] | 0 → 0 |
| legacy-empty | 16 | 6.72 → 7.02 | +1.14% [-8.09%, +10.67%] | 0 → 0 |
| legacy-only | 1 | 3.90 → 3.62 | -5.42% [-8.21%, -2.47%] | 0 → 0 |
| legacy-only | 4 | 3.78 → 3.67 | -3.74% [-7.04%, -0.28%] | 0 → 0 |
| legacy-only | 8 | 3.83 → 3.64 | -5.92% [-9.20%, -2.42%] | 0 → 0 |
| legacy-only | 16 | 3.91 → 3.64 | -7.03% [-12.32%, -2.55%] | 0 → 0 |
| shared-empty | 1 | 4.90 → 5.59 | +12.22% [+8.15%, +16.45%] | 0 → 0 |
| shared-empty | 4 | 4.96 → 5.51 | +8.70% [+3.28%, +14.02%] | 0 → 0 |
| shared-empty | 8 | 4.85 → 5.58 | +14.37% [+11.89%, +16.85%] | 0 → 0 |
| shared-empty | 16 | 5.14 → 5.56 | +14.12% [+8.10%, +21.32%] | 0 → 0 |
| shared-only | 1 | 4.96 → 3.64 | -27.37% [-28.80%, -26.00%] | 0 → 0 |
| shared-only | 4 | 4.97 → 3.64 | -26.62% [-27.95%, -25.44%] | 0 → 0 |
| shared-only | 8 | 5.01 → 3.62 | -26.69% [-28.30%, -25.06%] | 0 → 0 |
| shared-only | 16 | 5.04 → 3.61 | -28.75% [-30.96%, -26.79%] | 0 → 0 |

## 启用额外属性后仍有的成本

这组是 **新版本 legacy 与新版本 both**，不是优化前后比较；包含额外参数、属性和渲染树工作，不能全部归为合并开销。

| Case | Operation | Paired time change (95% CI) | Median allocated bytes A → B |
| --- | --- | --- | ---: |
| merge-1000x10 | changed-data-update | +15.98% [+13.01%, +19.47%] | 54,863,652 → 67,985,200 |
| merge-1000x10 | mount | +6.51% [+3.46%, +9.63%] | 212,192,794 → 222,053,544 |
| merge-1000x10 | same-data-update | +13.31% [+10.66%, +16.14%] | 54,939,376 → 68,060,872 |

## 方法、方案选择与边界

正式实验共 **168 个独立串行进程、4224 条测量样本**，12 组配对，每进程预热 3 次、测量 4 次，AB/BA 交替、种子 4852；保留全部预定样本。测量程序及 `run.ps1`、`prepare.ps1`、`verify.py`、`analyze.py` 均与预先公开的固定版本相同。五组等价工作量表格用例逐样本检查批次、更新组件和编辑次数；实际单元格/表头、更新文本和共享属性计数在计时外验证。两侧程序集各自 SHA256 全程稳定、彼此不同，运行时/OS/架构/GC 设置一致。

正式协议固定前做过两次探索性微预跑，各 3 组配对，选择第二个方案。全部 12 个预跑进程文件（1248 条样本）保存在 `pilots/` 子目录，[第一方案补丁和说明](experiments/capacity-pilots.md)也公开；预跑数据不混入正式 12 组分析。预跑程序集来自提交前工作区，正式程序集从固定提交归档、按版本隔离源码并使用 `--no-incremental` 完整构建；元数据和源码路径可使两者程序集哈希不同。

统计方法不变：先取每进程中位数，再求配对 B/A 的几何均值，20000 次配对 bootstrap；不移除异常点，不把进程内样本当独立重复。配对效果不是表中全局中位数的直接相除，区间未做多重比较校正。

同一机器：i7-12700F，12 核/20 线程，63.81 GiB 内存，Windows build 26200，高性能电源方案，默认 CPU 亲和性；SDK 10.0.103、.NET 10.0.13、Release x64、工作站 GC，关闭分层编译和 ReadyToRun。使用真实 AntDesign Table/Column 和无浏览器的 Blazor Renderer，全量渲染 100×10、1000×10、2000×20 个单元格，不分页/虚拟化，`RerenderStrategy.Always`；操作为首次渲染、同引用更新、同键新值更新。字典准备和委托/反射绑定在计时外，调用实际内部方法而非复制实现。

这是单机 .NET 渲染和差异计算，不包含浏览器 DOM、布局、绘制、WASM 或 SignalR；操作间强制 GC 与真实应用不同，不能推广为所有应用的用户体验，也不能跨实验轮次直接比较绝对耗时。

## 公开证据与复现

[原始数据与汇总](results/windows-net10-capacity-run3/)、[完整统计](results/windows-net10-capacity-run3/summary.md)、[环境/提交记录](results/windows-net10-capacity-run3/manifest-all.json)、[校验结果](results/windows-net10-capacity-run3/verification.json)、[校验和](results/windows-net10-capacity-run3/SHA256SUMS)及隐藏本地路径后的验证日志均公开。正式与预跑 JSONL 原始字节保持不变；前两轮[优化报告](OPTIMIZATION.zh-CN.md)和[初始 PR 报告](REPORT.zh-CN.md)及其原始数据也保持原样。

原 Windows 机器上，涉及 dotnet 的命令必须在提升权限的终端运行：

```powershell
./scripts/prepare.ps1 -DefinitionFile protocols/capacity-optimization.json
./scripts/run.ps1 -DefinitionFile protocols/capacity-optimization.json -Pairs 12 -Samples 4 -Warmups 3 -OutputName my-run
python scripts/verify.py results/my-run
python scripts/analyze.py results/my-run
```
