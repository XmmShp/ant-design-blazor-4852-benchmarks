| Case | Operation / merge case | A | B | B/A (95% paired bootstrap CI) | Allocated bytes A → B |
| --- | --- | ---: | ---: | --- | ---: |
| enabled-vs-baseline-1000x10 | changed-data-update | 365.414 ms | 428.352 ms | 1.1706 [1.1161, 1.2417] | 54,863,648 → 67,985,272 |
| enabled-vs-baseline-1000x10 | mount | 548.662 ms | 588.497 ms | 1.0751 [1.0331, 1.1148] | 212,112,836 → 222,053,718 |
| enabled-vs-baseline-1000x10 | same-data-update | 370.063 ms | 426.339 ms | 1.1589 [1.1206, 1.1920] | 54,939,308 → 68,060,932 |
| legacy-1000x10 | changed-data-update | 369.974 ms | 369.112 ms | 1.0040 [0.9566, 1.0480] | 54,863,648 → 54,863,504 |
| legacy-1000x10 | mount | 534.494 ms | 547.331 ms | 1.0353 [0.9774, 1.0920] | 212,112,714 → 212,192,818 |
| legacy-1000x10 | same-data-update | 380.209 ms | 371.246 ms | 1.0172 [0.9705, 1.0645] | 54,939,248 → 54,939,176 |
| merge-1000x10 | changed-data-update | 374.751 ms | 434.273 ms | 1.1466 [1.1061, 1.1961] | 54,863,648 → 67,985,272 |
| merge-1000x10 | mount | 547.405 ms | 595.524 ms | 1.0713 [1.0311, 1.1139] | 212,193,040 → 222,053,784 |
| merge-1000x10 | same-data-update | 377.131 ms | 413.918 ms | 1.1052 [1.0764, 1.1454] | 54,939,320 → 68,060,944 |
| micro | both-disjoint (1 entries per dictionary) | 3.558 ns | 96.990 ns | 27.6523 [27.1877, 28.1559] | 0 → 216 |
| micro | both-disjoint (4 entries per dictionary) | 3.565 ns | 228.773 ns | 66.4861 [64.3658, 69.2323] | 0 → 440 |
| micro | both-disjoint (8 entries per dictionary) | 3.478 ns | 432.282 ns | 126.8877 [123.0492, 131.9716] | 0 → 608 |
| micro | both-disjoint (16 entries per dictionary) | 3.407 ns | 838.193 ns | 244.0465 [240.0589, 248.1397] | 0 → 1,168 |
| micro | both-empty (0 entries per dictionary) | 3.493 ns | 5.572 ns | 1.6827 [1.6047, 1.8003] | 0 → 0 |
| micro | both-null (0 entries per dictionary) | 2.766 ns | 2.904 ns | 1.0607 [1.0330, 1.0921] | 0 → 0 |
| micro | both-overlap (1 entries per dictionary) | 3.461 ns | 100.629 ns | 29.1574 [28.6585, 29.6713] | 0 → 216 |
| micro | both-overlap (4 entries per dictionary) | 3.472 ns | 281.022 ns | 81.9504 [79.0543, 84.9838] | 0 → 328 |
| micro | both-overlap (8 entries per dictionary) | 3.570 ns | 502.771 ns | 144.1704 [140.3620, 148.3882] | 0 → 440 |
| micro | both-overlap (16 entries per dictionary) | 3.537 ns | 993.057 ns | 291.2341 [278.9662, 304.8013] | 0 → 608 |
| micro | legacy-empty (1 entries per dictionary) | 3.522 ns | 7.091 ns | 2.0391 [1.9814, 2.1070] | 0 → 0 |
| micro | legacy-empty (4 entries per dictionary) | 3.433 ns | 6.934 ns | 2.1050 [2.0139, 2.2135] | 0 → 0 |
| micro | legacy-empty (8 entries per dictionary) | 3.464 ns | 6.964 ns | 2.0484 [1.9673, 2.1553] | 0 → 0 |
| micro | legacy-empty (16 entries per dictionary) | 3.475 ns | 6.997 ns | 2.1401 [2.0070, 2.3281] | 0 → 0 |
| micro | legacy-only (1 entries per dictionary) | 3.480 ns | 3.731 ns | 1.0668 [1.0450, 1.0901] | 0 → 0 |
| micro | legacy-only (4 entries per dictionary) | 3.469 ns | 3.720 ns | 1.0699 [1.0475, 1.0941] | 0 → 0 |
| micro | legacy-only (8 entries per dictionary) | 3.452 ns | 3.664 ns | 1.0583 [1.0328, 1.0858] | 0 → 0 |
| micro | legacy-only (16 entries per dictionary) | 3.443 ns | 3.669 ns | 1.0611 [1.0378, 1.0827] | 0 → 0 |
| micro | shared-empty (1 entries per dictionary) | 3.453 ns | 5.583 ns | 1.6569 [1.5715, 1.7920] | 0 → 0 |
| micro | shared-empty (4 entries per dictionary) | 3.498 ns | 5.629 ns | 1.6758 [1.5949, 1.7944] | 0 → 0 |
| micro | shared-empty (8 entries per dictionary) | 3.397 ns | 5.593 ns | 1.6554 [1.5664, 1.7861] | 0 → 0 |
| micro | shared-empty (16 entries per dictionary) | 3.563 ns | 5.740 ns | 1.7149 [1.5829, 1.8902] | 0 → 0 |
| micro | shared-only (1 entries per dictionary) | 2.756 ns | 3.675 ns | 1.3370 [1.3062, 1.3690] | 0 → 0 |
| micro | shared-only (4 entries per dictionary) | 2.772 ns | 3.672 ns | 1.3338 [1.3125, 1.3575] | 0 → 0 |
| micro | shared-only (8 entries per dictionary) | 2.741 ns | 3.698 ns | 1.3554 [1.3246, 1.3912] | 0 → 0 |
| micro | shared-only (16 entries per dictionary) | 2.771 ns | 3.673 ns | 1.3357 [1.3003, 1.3761] | 0 → 0 |
| none-1000x10 | changed-data-update | 186.258 ms | 179.452 ms | 0.9585 [0.9292, 0.9912] | 29,567,912 → 29,567,912 |
| none-1000x10 | mount | 447.149 ms | 430.720 ms | 0.9632 [0.9153, 1.0079] | 178,775,602 → 178,855,958 |
| none-1000x10 | same-data-update | 150.697 ms | 147.195 ms | 0.9595 [0.9067, 1.0158] | 29,163,584 → 29,163,584 |
| none-100x10 | changed-data-update | 9.806 ms | 9.628 ms | 0.9838 [0.9073, 1.0732] | 2,977,840 → 2,977,840 |
| none-100x10 | mount | 22.448 ms | 21.966 ms | 1.0021 [0.9267, 1.0884] | 17,069,140 → 17,077,486 |
| none-100x10 | same-data-update | 11.581 ms | 10.589 ms | 0.9913 [0.8911, 1.1076] | 2,937,152 → 2,937,152 |
| none-2000x20 | changed-data-update | 781.819 ms | 763.727 ms | 0.9814 [0.9597, 1.0000] | 116,750,424 → 116,750,356 |
| none-2000x20 | mount | 1602.185 ms | 1575.593 ms | 0.9912 [0.9746, 1.0077] | 675,492,206 → 675,812,854 |
| none-2000x20 | same-data-update | 663.892 ms | 633.785 ms | 0.9792 [0.9559, 1.0026] | 114,989,472 → 114,989,544 |
