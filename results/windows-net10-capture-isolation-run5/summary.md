| Case | Operation / merge case | A | B | B/A (95% paired bootstrap CI) | Allocated bytes A → B |
| --- | --- | ---: | ---: | --- | ---: |
| both-1000x10 | changed-data-update | 407.022 ms | 418.843 ms | 1.0676 [0.9850, 1.1700] | 67,985,188 → 67,985,272 |
| both-1000x10 | mount | 567.181 ms | 579.962 ms | 1.0436 [0.9633, 1.1395] | 222,053,416 → 222,053,696 |
| both-1000x10 | same-data-update | 407.105 ms | 419.980 ms | 1.0531 [0.9642, 1.1638] | 68,060,944 → 68,060,880 |
| legacy-1000x10 | changed-data-update | 346.794 ms | 348.321 ms | 0.9967 [0.9270, 1.0723] | 54,863,648 → 54,863,648 |
| legacy-1000x10 | mount | 491.770 ms | 507.230 ms | 0.9981 [0.9493, 1.0441] | 212,192,330 → 212,193,026 |
| legacy-1000x10 | same-data-update | 351.471 ms | 353.553 ms | 1.0428 [0.9978, 1.0932] | 54,939,320 → 54,939,320 |
| micro | both-disjoint (1 entries per dictionary) | 94.632 ns | 94.423 ns | 0.9733 [0.9449, 1.0014] | 216 → 216 |
| micro | both-disjoint (4 entries per dictionary) | 225.578 ns | 222.617 ns | 0.9800 [0.9612, 0.9995] | 440 → 440 |
| micro | both-disjoint (8 entries per dictionary) | 415.592 ns | 403.466 ns | 0.9745 [0.9369, 1.0063] | 608 → 608 |
| micro | both-disjoint (16 entries per dictionary) | 790.504 ns | 800.386 ns | 1.0028 [0.9802, 1.0226] | 1,168 → 1,168 |
| micro | both-empty (0 entries per dictionary) | 5.456 ns | 5.571 ns | 1.0426 [1.0074, 1.0887] | 0 → 0 |
| micro | both-null (0 entries per dictionary) | 2.929 ns | 2.981 ns | 1.0176 [0.9926, 1.0449] | 0 → 0 |
| micro | both-overlap (1 entries per dictionary) | 96.912 ns | 93.165 ns | 0.9629 [0.9435, 0.9839] | 216 → 216 |
| micro | both-overlap (4 entries per dictionary) | 260.633 ns | 259.097 ns | 0.9967 [0.9807, 1.0141] | 328 → 328 |
| micro | both-overlap (8 entries per dictionary) | 470.826 ns | 471.788 ns | 0.9930 [0.9778, 1.0077] | 440 → 440 |
| micro | both-overlap (16 entries per dictionary) | 925.087 ns | 922.409 ns | 0.9829 [0.9550, 1.0056] | 608 → 608 |
| micro | legacy-empty (1 entries per dictionary) | 6.966 ns | 7.008 ns | 0.9888 [0.9625, 1.0192] | 0 → 0 |
| micro | legacy-empty (4 entries per dictionary) | 7.091 ns | 6.944 ns | 0.9895 [0.9570, 1.0232] | 0 → 0 |
| micro | legacy-empty (8 entries per dictionary) | 7.012 ns | 7.087 ns | 1.0208 [0.9887, 1.0557] | 0 → 0 |
| micro | legacy-empty (16 entries per dictionary) | 6.972 ns | 6.912 ns | 0.9883 [0.8869, 1.1115] | 0 → 0 |
| micro | legacy-only (1 entries per dictionary) | 3.660 ns | 3.701 ns | 1.0064 [0.9884, 1.0244] | 0 → 0 |
| micro | legacy-only (4 entries per dictionary) | 3.691 ns | 3.703 ns | 1.0151 [0.9946, 1.0392] | 0 → 0 |
| micro | legacy-only (8 entries per dictionary) | 3.618 ns | 3.669 ns | 1.0044 [0.9890, 1.0186] | 0 → 0 |
| micro | legacy-only (16 entries per dictionary) | 3.652 ns | 3.704 ns | 1.0204 [1.0015, 1.0393] | 0 → 0 |
| micro | shared-empty (1 entries per dictionary) | 5.470 ns | 5.538 ns | 1.0202 [0.9992, 1.0453] | 0 → 0 |
| micro | shared-empty (4 entries per dictionary) | 5.350 ns | 5.530 ns | 1.0266 [1.0054, 1.0565] | 0 → 0 |
| micro | shared-empty (8 entries per dictionary) | 5.472 ns | 5.495 ns | 1.0145 [0.9926, 1.0402] | 0 → 0 |
| micro | shared-empty (16 entries per dictionary) | 5.595 ns | 5.513 ns | 1.0168 [0.9523, 1.1039] | 0 → 0 |
| micro | shared-only (1 entries per dictionary) | 3.634 ns | 3.667 ns | 1.0117 [0.9922, 1.0343] | 0 → 0 |
| micro | shared-only (4 entries per dictionary) | 3.620 ns | 3.678 ns | 1.0169 [0.9948, 1.0403] | 0 → 0 |
| micro | shared-only (8 entries per dictionary) | 3.642 ns | 3.666 ns | 1.0045 [0.9899, 1.0169] | 0 → 0 |
| micro | shared-only (16 entries per dictionary) | 3.651 ns | 3.667 ns | 0.9983 [0.9777, 1.0146] | 0 → 0 |
| none-1000x10 | changed-data-update | 172.022 ms | 175.482 ms | 1.0041 [0.9730, 1.0357] | 29,567,912 → 29,567,912 |
| none-1000x10 | mount | 418.494 ms | 410.686 ms | 0.9770 [0.9208, 1.0377] | 178,856,078 → 178,856,080 |
| none-1000x10 | same-data-update | 148.395 ms | 142.960 ms | 0.9761 [0.9075, 1.0414] | 29,163,584 → 29,163,512 |
| none-100x10 | changed-data-update | 9.454 ms | 8.327 ms | 0.9316 [0.8642, 0.9982] | 2,977,840 → 2,977,840 |
| none-100x10 | mount | 21.587 ms | 19.852 ms | 0.9311 [0.8762, 0.9807] | 17,077,308 → 17,077,448 |
| none-100x10 | same-data-update | 9.621 ms | 8.652 ms | 0.9363 [0.8599, 1.0132] | 2,937,008 → 2,937,152 |
| none-2000x20 | changed-data-update | 715.460 ms | 708.978 ms | 0.9858 [0.9375, 1.0346] | 116,750,424 → 116,750,336 |
| none-2000x20 | mount | 1507.713 ms | 1544.367 ms | 1.0141 [0.9764, 1.0535] | 675,812,400 → 675,812,394 |
| none-2000x20 | same-data-update | 604.312 ms | 610.749 ms | 1.0592 [1.0011, 1.1391] | 114,989,544 → 114,989,616 |
| shared-1000x10 | changed-data-update | 354.327 ms | 359.704 ms | 1.0333 [0.9607, 1.1084] | 54,864,768 → 54,864,768 |
| shared-1000x10 | mount | 513.675 ms | 524.451 ms | 1.0060 [0.9568, 1.0499] | 212,214,182 → 212,214,496 |
| shared-1000x10 | same-data-update | 349.791 ms | 366.285 ms | 1.0152 [0.9584, 1.0634] | 54,940,440 → 54,940,440 |
