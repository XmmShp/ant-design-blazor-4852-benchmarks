# CaptureUnmatchedValues 隔离 A/B 测试

两组均来自最终生产提交 `2b1cafd50978e1622b68c45aba7ca572506aa5d4`。A 为 `false`，B 为原来的 `true`，只切换 AntDomComponentBase 上一个布尔标记；字典合并、Razor、属性输入和渲染输出保持一致。B/A 大于 1 表示启用捕获更慢。生产分支和 PR 均未修改，也未向 issue/PR 发布消息。

默认 1 万格的三项点估计为 -2.39% 至 +0.41%，区间均跨零。默认 4 万格挂载 +1.41%、变更数据更新 -1.42%，区间跨零；同数据更新 +5.92% [95% CI +0.11%, +13.91%]，因此不能称完全无退化。显式 shared-only 的挂载/同数据/变更数据更新为 +0.60% / +1.52% / +3.33%；双字典为 +4.36% / +5.31% / +6.76%，这些区间全部跨零。表格分配量差异最多 696 B（进程中位数的中位数之差），没有与单元格数成比例的额外分配；全部 26 项合并微基准的每次调用分配量相同。

取舍：保留 true 可以继续支持直接写 HTML 属性。当前证据未显示默认路径一致的大幅退化，显式单字典的点估计较小，但区间仍允许更大的成本；尤其 4 万格同数据更新提示实际退化的可能。基于这轮数据，保留便利性仍是合理选择，但不能把‘性能开销极小’当作已证实事实。没有实际捕获未知参数的测试，也不能承诺直接写 HTML 属性时总成本与本轮完全相同。小表对照和绕过参数分发的微基准也存在波动，不能把每项差异都归因于捕获算法。

## 全部表格结果

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

4 万格同数据更新的 12 对耗时比值变化范围为 -7.41% 至 +48.27%；+48.27% 的那一对按预案保留，没有剔除。该项毫秒列为 604.312 → 610.749 ms，但配对几何平均估计为 +5.92%，两者是不同统计量，波动需要如实保留。逐对数据见 [process-pairs.csv](results/windows-net10-capture-isolation-run5/process-pairs.csv)。

## 方法

12 对独立进程/场景，AB/BA 平衡交替、场景顺序种子 4852；每进程 3 次预热、4 次采样。共 168 个正式进程、4224 条样本。先取每进程中位数，再对 12 对耗时比值取几何平均，进行 20000 次配对 bootstrap，给出 95% 区间。毫秒列为进程中位数的中位数，不是比值估计的分子分母，也不是配对绝对差估计；跨 0 的百分比区间不能证明零开销。未剔除离群值，未作多重比较校正。核心计时和分析与此前相同；本轮在计时外增加输出哈希校验，并增加 shared-only 用例，未与历史样本混合。

## 场景与范围

none 不传 AdditionalAttributes（连 null 也不传）；legacy 仅传 OnCell；shared 显式传预先构造的四项 AdditionalAttributes；both 同时传同样的四项 AdditionalAttributes 和四项 OnCell（两键重叠）。每对 A/B 参数完全相同，字典在计时外创建，旧字典优先级不变。没有向组件传未知参数，因此测的是捕获标记对参数分发路径的影响，不包含实际收集未知 HTML 参数时创建字典的成本。26 个直接调用合并函数的微基准作为负对照；其代码不经过参数分发，波动不能归因于捕获开关。

## 一致性与来源验证

所有 1728 条表格操作记录均完成单元格/表头校验；每对对应采样的原生输出 SHA256、批次数、组件更新数、编辑数相同。哈希保留原生元素树、属性名与标量值、文本/markup；跳过组件参数、框架 ID/引用捕获，事件委托用其类型表示，未比较委托身份或浏览器行为。构建日志仅替换本地工作目录路径，原始采样保持字节不变。另有 8 个预检进程、24 条操作，单独保留且不进入统计。源码审计比较构建前所有归档文件，两组原始归档相同，仅一个文件的一处 true/false 不同；实际加载的 ParameterAttribute 标记也逐进程验证。

## 限制

Windows / .NET 10 / 一台机器；真实组件生命周期和渲染树 diff，但无浏览器 DOM、布局、JS、网络或 WASM。较高频率的小表交互、其他参数数量或真实未知属性捕获可能不同。这是实验性开关对照，不是可直接提交的兼容性方案；移除已有捕获 API 的兼容性需要另外评估。此前最终版相对 PR 前的 7.5% / 15.9–17.1% 功能成本包含不同工作，不能直接全归因于此开关，也不能把两轮比例相减来分解成本。

## 公开证据

- [Prospective protocol](protocols/capture-isolation.md) / [definition](protocols/capture-isolation.json) / [one-line library patch](experiments/capture-disabled.patch)
- [Raw records, manifest, source audits, build logs and checksums](results/windows-net10-capture-isolation-run5/)
- [All 44 table/helper estimates](results/windows-net10-capture-isolation-run5/summary.md) / [JSON](results/windows-net10-capture-isolation-run5/summary.json) / [samples CSV](results/windows-net10-capture-isolation-run5/samples.csv)
- [Validation](results/windows-net10-capture-isolation-run5/validation.json) / [source equivalence](results/windows-net10-capture-isolation-run5/source-equivalence.json) / [SHA256SUMS](results/windows-net10-capture-isolation-run5/SHA256SUMS)
- [Separate correctness preflight](results/capture-preflight/)
- [Earlier final vs pre-PR study](FINAL.md)
