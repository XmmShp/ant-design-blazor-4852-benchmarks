| Case | Operation / merge case | A | B | B/A (95% paired bootstrap CI) | Allocated bytes A → B |
| --- | --- | ---: | ---: | --- | ---: |
| legacy-1000x10 | changed-data-update | 370.090 ms | 367.310 ms | 0.9763 [0.9369, 1.0063] | 54,863,564 → 54,863,632 |
| legacy-1000x10 | mount | 531.727 ms | 539.114 ms | 0.9964 [0.9727, 1.0216] | 212,112,640 → 212,192,794 |
| legacy-1000x10 | same-data-update | 375.921 ms | 368.560 ms | 0.9716 [0.9181, 1.0160] | 54,939,320 → 54,939,320 |
| merge-1000x10 | changed-data-update | 360.566 ms | 415.071 ms | 1.1436 [1.1166, 1.1736] | 54,863,564 → 77,905,104 |
| merge-1000x10 | mount | 510.402 ms | 618.652 ms | 1.1832 [1.1477, 1.2232] | 212,192,872 → 229,494,464 |
| merge-1000x10 | same-data-update | 364.252 ms | 414.976 ms | 1.1336 [1.1051, 1.1689] | 54,939,176 → 77,980,704 |
| micro | both-disjoint (1 entries per dictionary) | 3.512 ns | 106.172 ns | 30.0631 [28.8568, 31.1117] | 0 → 328 |
| micro | both-disjoint (4 entries per dictionary) | 3.440 ns | 369.586 ns | 107.2569 [102.4906, 112.1519] | 0 → 1,104 |
| micro | both-disjoint (8 entries per dictionary) | 3.455 ns | 590.036 ns | 169.3173 [162.4777, 176.3175] | 0 → 1,104 |
| micro | both-disjoint (16 entries per dictionary) | 3.384 ns | 1167.157 ns | 334.7346 [317.6713, 349.3474] | 0 → 2,192 |
| micro | both-empty (0 entries per dictionary) | 3.455 ns | 27.647 ns | 7.8548 [7.4923, 8.1843] | 0 → 80 |
| micro | both-null (0 entries per dictionary) | 2.590 ns | 2.902 ns | 1.1095 [1.0760, 1.1441] | 0 → 0 |
| micro | both-overlap (1 entries per dictionary) | 3.451 ns | 120.114 ns | 33.8578 [32.3554, 35.2812] | 0 → 328 |
| micro | both-overlap (4 entries per dictionary) | 3.476 ns | 342.337 ns | 97.2998 [93.6777, 100.9359] | 0 → 576 |
| micro | both-overlap (8 entries per dictionary) | 3.429 ns | 653.657 ns | 187.3614 [178.5049, 195.1452] | 0 → 1,104 |
| micro | both-overlap (16 entries per dictionary) | 3.409 ns | 1181.707 ns | 341.7742 [324.8612, 358.7283] | 0 → 1,104 |
| micro | legacy-only (1 entries per dictionary) | 3.315 ns | 3.679 ns | 1.0809 [1.0336, 1.1200] | 0 → 0 |
| micro | legacy-only (4 entries per dictionary) | 3.309 ns | 3.695 ns | 1.0903 [1.0479, 1.1290] | 0 → 0 |
| micro | legacy-only (8 entries per dictionary) | 3.299 ns | 3.731 ns | 1.0993 [1.0582, 1.1388] | 0 → 0 |
| micro | legacy-only (16 entries per dictionary) | 3.349 ns | 3.720 ns | 1.0859 [1.0398, 1.1254] | 0 → 0 |
| micro | shared-only (1 entries per dictionary) | 2.598 ns | 3.696 ns | 1.4173 [1.3837, 1.4540] | 0 → 0 |
| micro | shared-only (4 entries per dictionary) | 2.580 ns | 3.682 ns | 1.4142 [1.3748, 1.4516] | 0 → 0 |
| micro | shared-only (8 entries per dictionary) | 2.556 ns | 3.756 ns | 1.4517 [1.4173, 1.4815] | 0 → 0 |
| micro | shared-only (16 entries per dictionary) | 2.610 ns | 3.684 ns | 1.4001 [1.3741, 1.4251] | 0 → 0 |
| none-1000x10 | changed-data-update | 170.741 ms | 173.783 ms | 1.0113 [0.9912, 1.0320] | 29,567,912 → 29,567,912 |
| none-1000x10 | mount | 411.215 ms | 404.959 ms | 1.0019 [0.9775, 1.0265] | 178,776,298 → 178,856,144 |
| none-1000x10 | same-data-update | 138.174 ms | 136.851 ms | 1.0011 [0.9634, 1.0411] | 29,163,584 → 29,163,584 |
| none-100x10 | changed-data-update | 9.326 ms | 9.457 ms | 1.0141 [0.9490, 1.0829] | 2,977,840 → 2,977,840 |
| none-100x10 | mount | 21.333 ms | 21.362 ms | 1.0038 [0.9169, 1.1025] | 17,069,198 → 17,077,380 |
| none-100x10 | same-data-update | 9.676 ms | 9.809 ms | 1.0104 [0.9094, 1.1209] | 2,937,152 → 2,937,152 |
| none-2000x20 | changed-data-update | 724.739 ms | 745.244 ms | 0.9971 [0.9550, 1.0357] | 116,750,280 → 116,750,424 |
| none-2000x20 | mount | 1527.247 ms | 1557.163 ms | 0.9922 [0.9376, 1.0354] | 675,492,052 → 675,812,680 |
| none-2000x20 | same-data-update | 601.395 ms | 615.575 ms | 1.0007 [0.9416, 1.0539] | 114,989,416 → 114,989,544 |
