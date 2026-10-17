| Case | Operation / merge case | A | B | B/A (95% paired bootstrap CI) | Allocated bytes A → B |
| --- | --- | ---: | ---: | --- | ---: |
| both-1000x10 | changed-data-update | 413.726 ms | 404.346 ms | 0.9436 [0.8455, 1.0219] | 77,905,104 → 72,465,272 |
| both-1000x10 | mount | 602.211 ms | 559.045 ms | 0.9175 [0.8046, 1.0230] | 229,494,608 → 225,414,148 |
| both-1000x10 | same-data-update | 405.635 ms | 393.215 ms | 0.9553 [0.8251, 1.0846] | 77,980,776 → 72,540,944 |
| legacy-1000x10 | changed-data-update | 369.698 ms | 362.475 ms | 0.9994 [0.9696, 1.0256] | 54,863,648 → 54,863,576 |
| legacy-1000x10 | mount | 537.654 ms | 541.666 ms | 0.9761 [0.9461, 1.0043] | 212,193,116 → 212,193,116 |
| legacy-1000x10 | same-data-update | 367.357 ms | 366.026 ms | 0.9988 [0.9699, 1.0263] | 54,939,288 → 54,939,320 |
| merge-1000x10 | changed-data-update | 376.664 ms | 434.179 ms | 1.1207 [1.0661, 1.1710] | 54,863,648 → 72,465,272 |
| merge-1000x10 | mount | 538.460 ms | 585.691 ms | 1.0591 [1.0083, 1.1087] | 212,193,090 → 225,414,094 |
| merge-1000x10 | same-data-update | 361.498 ms | 423.103 ms | 1.1152 [1.0656, 1.1665] | 54,939,176 → 72,540,944 |
| micro | both-disjoint (1 entries per dictionary) | 108.254 ns | 118.779 ns | 1.1039 [1.0785, 1.1292] | 328 → 328 |
| micro | both-disjoint (4 entries per dictionary) | 374.283 ns | 347.957 ns | 0.9452 [0.9177, 0.9733] | 1,104 → 968 |
| micro | both-disjoint (8 entries per dictionary) | 586.650 ns | 577.482 ns | 0.9924 [0.9766, 1.0075] | 1,104 → 1,248 |
| micro | both-disjoint (16 entries per dictionary) | 1156.394 ns | 1039.297 ns | 0.9046 [0.8935, 0.9145] | 2,192 → 1,808 |
| micro | both-empty (0 entries per dictionary) | 27.235 ns | 5.073 ns | 0.1833 [0.1744, 0.1922] | 80 → 0 |
| micro | both-null (0 entries per dictionary) | 2.968 ns | 3.193 ns | 1.0498 [1.0139, 1.0830] | 0 → 0 |
| micro | both-overlap (1 entries per dictionary) | 118.836 ns | 125.196 ns | 1.0507 [1.0284, 1.0734] | 328 → 328 |
| micro | both-overlap (4 entries per dictionary) | 337.286 ns | 310.942 ns | 0.9260 [0.9149, 0.9370] | 576 → 440 |
| micro | both-overlap (8 entries per dictionary) | 644.201 ns | 567.258 ns | 0.8783 [0.8603, 0.8933] | 1,104 → 552 |
| micro | both-overlap (16 entries per dictionary) | 1130.300 ns | 1069.122 ns | 0.9235 [0.9002, 0.9428] | 1,104 → 720 |
| micro | legacy-empty (1 entries per dictionary) | 75.774 ns | 6.556 ns | 0.0874 [0.0857, 0.0894] | 272 → 0 |
| micro | legacy-empty (4 entries per dictionary) | 190.352 ns | 6.520 ns | 0.0349 [0.0333, 0.0367] | 520 → 0 |
| micro | legacy-empty (8 entries per dictionary) | 355.223 ns | 6.468 ns | 0.0174 [0.0158, 0.0186] | 1,048 → 0 |
| micro | legacy-empty (16 entries per dictionary) | 559.467 ns | 6.670 ns | 0.0117 [0.0111, 0.0123] | 1,048 → 0 |
| micro | legacy-only (1 entries per dictionary) | 3.685 ns | 3.910 ns | 1.0418 [1.0220, 1.0604] | 0 → 0 |
| micro | legacy-only (4 entries per dictionary) | 3.725 ns | 3.910 ns | 1.0278 [1.0067, 1.0469] | 0 → 0 |
| micro | legacy-only (8 entries per dictionary) | 3.731 ns | 3.910 ns | 1.0433 [1.0197, 1.0700] | 0 → 0 |
| micro | legacy-only (16 entries per dictionary) | 3.708 ns | 3.901 ns | 0.9860 [0.9217, 1.0384] | 0 → 0 |
| micro | shared-empty (1 entries per dictionary) | 75.946 ns | 5.148 ns | 0.0671 [0.0644, 0.0700] | 272 → 0 |
| micro | shared-empty (4 entries per dictionary) | 188.204 ns | 5.133 ns | 0.0266 [0.0258, 0.0275] | 520 → 0 |
| micro | shared-empty (8 entries per dictionary) | 361.249 ns | 5.020 ns | 0.0129 [0.0117, 0.0139] | 1,048 → 0 |
| micro | shared-empty (16 entries per dictionary) | 560.530 ns | 5.113 ns | 0.0088 [0.0082, 0.0093] | 1,048 → 0 |
| micro | shared-only (1 entries per dictionary) | 3.715 ns | 4.921 ns | 1.3134 [1.2856, 1.3425] | 0 → 0 |
| micro | shared-only (4 entries per dictionary) | 3.692 ns | 4.879 ns | 1.3248 [1.3057, 1.3452] | 0 → 0 |
| micro | shared-only (8 entries per dictionary) | 3.714 ns | 4.976 ns | 1.3325 [1.3122, 1.3549] | 0 → 0 |
| micro | shared-only (16 entries per dictionary) | 3.697 ns | 4.865 ns | 1.2061 [1.0025, 1.3479] | 0 → 0 |
| none-1000x10 | changed-data-update | 162.313 ms | 165.443 ms | 1.0103 [0.9907, 1.0304] | 29,567,912 → 29,567,840 |
| none-1000x10 | mount | 395.583 ms | 406.325 ms | 1.0076 [0.9817, 1.0375] | 178,856,354 → 178,855,984 |
| none-1000x10 | same-data-update | 131.153 ms | 135.087 ms | 1.0168 [0.9947, 1.0359] | 29,163,584 → 29,163,584 |
| none-100x10 | changed-data-update | 9.184 ms | 8.770 ms | 0.9730 [0.8537, 1.0940] | 2,977,840 → 2,977,840 |
| none-100x10 | mount | 21.961 ms | 20.873 ms | 1.0234 [0.9315, 1.1264] | 17,077,406 → 17,077,390 |
| none-100x10 | same-data-update | 9.833 ms | 9.361 ms | 0.9815 [0.8547, 1.1215] | 2,937,008 → 2,937,152 |
| none-2000x20 | changed-data-update | 711.388 ms | 709.104 ms | 0.9786 [0.9492, 1.0036] | 116,750,424 → 116,750,424 |
| none-2000x20 | mount | 1454.597 ms | 1442.850 ms | 0.9732 [0.9499, 0.9951] | 675,813,584 → 675,812,476 |
| none-2000x20 | same-data-update | 591.351 ms | 579.167 ms | 0.9714 [0.9535, 0.9881] | 114,989,544 → 114,989,688 |
