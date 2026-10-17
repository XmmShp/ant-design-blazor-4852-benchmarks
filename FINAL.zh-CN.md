# 最终 PR 与 PR 前版本：直接配对对比（2026-10-17）

本轮重新配对比较最终提交与最初 PR 之前的版本，不再累加或相乘前三轮的变化率。[PR #4855](https://github.com/ant-design-blazor/ant-design-blazor/pull/4855) 的生产代码保持 `2b1cafd`，本次没有修改实现。

不传额外属性时，六组大表格耗时点估计为 **−4.15% 至 −0.88%**；本轮未分辨出耗时退化，四个区间包含零变化、两个低于零（其中一个仅略低）。这不证明零开销或普遍加速：区间最大上界仍为 **+1.58%**。首次渲染分配约增加 **0.05%**，更新分配基本不变。

启用属性场景中，最终版 both 对比 PR 前 legacy，首次渲染耗时 **+7.51%**，同数据更新 **+15.89%**，变数据更新 **+17.06%**，三个区间均高于零；分配分别增加 **9.480 MiB、12.514 MiB、12.514 MiB**。两侧功能/输出不同，这些不是等价功能的纯退化率，也不是合并方法本身的成本。

| Arm | Production commit |
| --- | --- |
| A: pre-PR baseline (parent of initial PR commit) | `943f6af77201ba1ebfd4756c054ff838d8bef0d0` |
| B: final PR | `2b1cafd50978e1622b68c45aba7ca572506aa5d4` |

Measured harness/prospective protocol commit: `4c57fef4e61f68d8718574321196a32a5d86971d`.
Protocol: [final-vs-baseline.md](protocols/final-vs-baseline.md), [machine-readable definition](protocols/final-vs-baseline.json).

## 不传额外属性：默认路径的直接对比

完全省略 `AdditionalAttributes` 参数，连显式 null 都不传；不分页、不虚拟化，所有单元格/表头均渲染。主要用例是 1 万和 4 万单元格，分别测首次渲染、同引用更新、同键新值更新。

| Case | Operation | Paired time change B/A − 1 (95% CI) | Median allocated bytes A → B |
| --- | --- | --- | ---: |
| none-1000x10 | changed-data-update | -4.15% [-7.08%, -0.88%] | 29,567,912 → 29,567,912 |
| none-1000x10 | mount | -3.68% [-8.47%, +0.79%] | 178,775,602 → 178,855,958 |
| none-1000x10 | same-data-update | -4.05% [-9.33%, +1.58%] | 29,163,584 → 29,163,584 |
| none-2000x20 | changed-data-update | -1.86% [-4.03%, -0.0031%] | 116,750,424 → 116,750,356 |
| none-2000x20 | mount | -0.88% [-2.54%, +0.77%] | 675,492,206 → 675,812,854 |
| none-2000x20 | same-data-update | -2.08% [-4.41%, +0.26%] | 114,989,472 → 114,989,544 |

这些用例检查实际渲染树，跨版本逐样本核对批次、更新组件和编辑次数。描述性置信区间不证明零开销或等效性，本轮没有预设等效性边界。分配是整次操作的托管总分配，不是保留堆大小或峰值内存。

1 万单元格首次渲染增加 80356 字节（78.47 KiB、0.04495%），4 万单元格增加 320648 字节（313.13 KiB、0.04747%）。更新的差异为零或几十字节，总分配则为数十/上百 MiB。4 万单元格变数据更新的区间上界保留更多小数，否则两位小数会显示为负零；本轮未做多重比较校正，不能据这个略低于零的区间宣称普遍加速。

1000 单元格的小表对照也完整保留：

| Case | Operation | Paired time change B/A − 1 (95% CI) | Median allocated bytes A → B |
| --- | --- | --- | ---: |
| none-100x10 | changed-data-update | -1.62% [-9.27%, +7.32%] | 2,977,840 → 2,977,840 |
| none-100x10 | mount | +0.21% [-7.33%, +8.84%] | 17,069,140 → 17,077,486 |
| none-100x10 | same-data-update | -0.87% [-10.89%, +10.76%] | 2,937,152 → 2,937,152 |

## 启用额外属性：最终版本与旧版本的直接比较

旧版这些列没有共享的 `AdditionalAttributes` 参数，无法运行完全相同的新功能场景。因此 A 使用已有的四条目 `OnCell` 字典，B 在相同旧字典上额外使用四条目 `AdditionalAttributes`，其中两个键重叠；输入均提前构造。
这是“最终版启用新功能”与“PR 前已有属性接口”的直接版本比较。两侧参数、属性和输出不同，也包括已记录的 title 优先级变化；不是等价功能的纯性能退化，也不是孤立的字典合并成本。

| Case | Operation | Paired time change B/A − 1 (95% CI) | Median allocated bytes A → B |
| --- | --- | --- | ---: |
| enabled-vs-baseline-1000x10 | changed-data-update | +17.06% [+11.61%, +24.17%] | 54,863,648 → 67,985,272 |
| enabled-vs-baseline-1000x10 | mount | +7.51% [+3.31%, +11.48%] | 212,112,836 → 222,053,718 |
| enabled-vs-baseline-1000x10 | same-data-update | +15.89% [+12.06%, +19.20%] | 54,939,308 → 68,060,932 |

| Operation | Median allocation change B − A, MiB | Change relative to A |
| --- | ---: | ---: |
| changed-data-update | +12.514 | +23.917% |
| mount | +9.480 | +4.687% |
| same-data-update | +12.514 | +23.884% |

收益是组件能一致地把无障碍、测试和原生 HTML 属性转发到目标元素，这是功能收益，并非性能收益。启用功能仍增加处理和渲染工作，不能把本组版本总差异全部归因到属性捕获、合并或 splatting 中的某一项。

## 最终版合并方法的实际成本

以下仅列最终版本的绝对耗时和分配，通过缓存委托调用实际内部方法；字典构造、准备和反射绑定在计时外，循环/委托/结果存储开销在计时内。一次逻辑操作可能合并多次，不能按每单元格一次直接推算整表成本。

| Final helper inputs | Entries per nonempty input | Final median ns/call | Final B/call |
| --- | ---: | ---: | ---: |
| both-disjoint | 1 | 96.99 | 216 |
| both-disjoint | 4 | 228.77 | 440 |
| both-disjoint | 8 | 432.28 | 608 |
| both-disjoint | 16 | 838.19 | 1,168 |
| both-empty | 0 | 5.57 | 0 |
| both-null | 0 | 2.90 | 0 |
| both-overlap | 1 | 100.63 | 216 |
| both-overlap | 4 | 281.02 | 328 |
| both-overlap | 8 | 502.77 | 440 |
| both-overlap | 16 | 993.06 | 608 |
| legacy-empty | 1 | 7.09 | 0 |
| legacy-empty | 4 | 6.93 | 0 |
| legacy-empty | 8 | 6.96 | 0 |
| legacy-empty | 16 | 7.00 | 0 |
| legacy-only | 1 | 3.73 | 0 |
| legacy-only | 4 | 3.72 | 0 |
| legacy-only | 8 | 3.66 | 0 |
| legacy-only | 16 | 3.67 | 0 |
| shared-empty | 1 | 5.58 | 0 |
| shared-empty | 4 | 5.63 | 0 |
| shared-empty | 8 | 5.59 | 0 |
| shared-empty | 16 | 5.74 | 0 |
| shared-only | 1 | 3.68 | 0 |
| shared-only | 4 | 3.67 | 0 |
| shared-only | 8 | 3.70 | 0 |
| shared-only | 16 | 3.67 | 0 |

`shared-only` 表示共享字典非空、旧字典为 null；`legacy-only` 相反。`shared-empty` 表示共享字典非 null 但为空、旧字典有 N 项；`legacy-empty` 相反。`overlap` 表示全部键重叠。

PR 前版本没有同等共享属性/合并方法，其微基准调用原有的不内联“返回旧字典”委托，不处理共享字典。因此完整统计里的共享/合并场景 B/A 比值不是等价功能的退化率，不能这样解读；这些场景应看最终版的绝对 ns/字节。计时输入是普通默认比较器的字典，不代表所有字典实现和比较器。

即使不合并，返回路径对照也比最终 helper 略便宜：双方 null 约 **2.77 → 2.90 ns**，四条目的仅旧字典约 **3.47 → 3.72 ns**，均为零分配。这些数值包含委托/循环开销，不是 Blazor 属性捕获机制或应用延迟的测量；不构造合并字典不等于零 CPU 成本。

最终版在 null/空输入时复用已有输入，非空合并估算并集容量，并对常见 Dictionary 使用结构体枚举器；保留大小写不敏感、旧属性值优先，不改输入、不按引用缓存。相对于中间版的取舍（包括非 null 空字典路径的小幅耗时增加）见之前的[容量优化报告](CAPACITY.zh-CN.md)，不把那轮百分比混入本轮直接对比。

## 补充对照，单独解读

### 跨版本仅旧属性接口

| Case | Operation | Paired time change B/A − 1 (95% CI) | Median allocated bytes A → B |
| --- | --- | --- | ---: |
| legacy-1000x10 | changed-data-update | +0.40% [-4.34%, +4.80%] | 54,863,648 → 54,863,504 |
| legacy-1000x10 | mount | +3.53% [-2.26%, +9.20%] | 212,112,714 → 212,192,818 |
| legacy-1000x10 | same-data-update | +1.72% [-2.95%, +6.45%] | 54,939,248 → 54,939,176 |

固定字典含 `title`，其与列管理的 title 冲突时，PR 前后优先级不同，输出并非严格相同；本组不作为默认路径等效性的额外证据。

### 仅最终版：legacy 与 both

| Case | Operation | Paired time change B/A − 1 (95% CI) | Median allocated bytes A → B |
| --- | --- | --- | ---: |
| merge-1000x10 | changed-data-update | +14.66% [+10.61%, +19.61%] | 54,863,648 → 67,985,272 |
| merge-1000x10 | mount | +7.13% [+3.11%, +11.39%] | 212,193,040 → 222,053,784 |
| merge-1000x10 | same-data-update | +10.52% [+7.64%, +14.54%] | 54,939,320 → 68,060,944 |

这组两侧都使用最终程序集，只衡量同版本中启用额外属性的成本，不能代替前面的“最终版与 PR 前版本”直接比较。

## 方法、校验与边界

共 **168 个独立串行进程、4224 条正式样本**，12 组配对，每进程预热 3 次、测量 4 次，AB/BA 交替、用例乱序、种子 4852；保留所有计划样本，不移除异常值，也不混合前三轮数据。每进程先取中位数，再求配对 B/A 的几何均值，并做原有的 20000 次配对 bootstrap。不把进程内样本当独立重复，配对效果也不等于全局中位数直接相除。置信区间未做多重比较校正。

使用真实 AntDesign Table/Column 与 Blazor Renderer 的生命周期和差异计算，计时外验证单元格/表头、更新文本及共享属性数量。三组不传属性的等价用例逐样本核对渲染工作量；两侧程序集各自哈希稳定且彼此不同，运行时/OS/架构/GC 设置一致。测量程序、构建/运行/验证/分析脚本与预先公开的固定版本相同。

两版均从固定提交归档源码，按提交隔离并以 `--no-incremental` 完整构建。最终提交已有的 3058 项 .NET 10 测试和 netstandard2.1 构建证据见[前一轮报告](CAPACITY.zh-CN.md)；因本次不改生产代码，没有把既有结果伪称为本轮新跑的回归测试。

环境保持同一机器/配置：i7-12700F，12 核/20 线程，63.81 GiB 内存，Windows build 26200，高性能电源方案，默认 CPU 亲和性；SDK 10.0.103、.NET 10.0.13、Release x64、工作站 GC，关闭分层编译和 ReadyToRun。`RerenderStrategy.Always`，全量渲染 100×10、1000×10、2000×20 个单元格；显式 GC 在计时外，操作内发生的自动 GC 计入耗时。

本实验仅覆盖单机单运行时的无浏览器 .NET 渲染/差异计算；显示批次应用和 JS 为空操作，不含浏览器 DOM、布局、绘制、WASM 或 SignalR。操作间强制 GC 不同于生产环境。启用属性场景使用预构造、显式传入的字典，不包含由多个未匹配 Razor 参数构造 catch-all 字典的成本。不能证明零开销、推广为全部应用的延迟，或跨轮次直接比较绝对耗时。

## 公开证据与复现

[原始数据和汇总](results/windows-net10-final-vs-baseline-run4/)、[完整统计](results/windows-net10-final-vs-baseline-run4/summary.md)、[环境/源码记录](results/windows-net10-final-vs-baseline-run4/manifest-all.json)、[验证结果](results/windows-net10-final-vs-baseline-run4/verification.json)、[校验和](results/windows-net10-final-vs-baseline-run4/SHA256SUMS)均公开。JSONL 字节不变，构建日志仅替换本地用户/工作区路径。前三轮报告及原始证据不改动，未向 issue 或 PR 发送评论。

原 Windows 机器上，涉及 dotnet 的命令必须在提升权限的终端运行：

```powershell
./scripts/prepare.ps1 -DefinitionFile protocols/final-vs-baseline.json
./scripts/run.ps1 -DefinitionFile protocols/final-vs-baseline.json -Pairs 12 -Samples 4 -Warmups 3 -OutputName my-run
python scripts/verify.py results/my-run
python scripts/analyze.py results/my-run
```
