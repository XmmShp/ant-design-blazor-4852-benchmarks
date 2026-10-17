| Case | Operation / merge case | A | B | B/A (95% paired bootstrap CI) | Allocated bytes A → B |
| --- | --- | ---: | ---: | --- | ---: |
| both-1000x10 | changed-data-update | 432.439 ms | 427.527 ms | 0.9607 [0.8583, 1.0384] | 72,465,172 → 67,985,200 |
| both-1000x10 | mount | 618.335 ms | 588.484 ms | 0.9293 [0.8241, 1.0080] | 225,414,042 → 222,053,482 |
| both-1000x10 | same-data-update | 439.971 ms | 423.967 ms | 0.9450 [0.8414, 1.0305] | 72,540,944 → 68,060,872 |
| legacy-1000x10 | changed-data-update | 376.201 ms | 384.004 ms | 0.9897 [0.9562, 1.0220] | 54,863,648 → 54,863,648 |
| legacy-1000x10 | mount | 546.168 ms | 554.931 ms | 1.0323 [0.9818, 1.0913] | 212,192,684 → 212,193,256 |
| legacy-1000x10 | same-data-update | 373.597 ms | 395.746 ms | 1.0140 [0.9753, 1.0527] | 54,939,320 → 54,939,320 |
| merge-1000x10 | changed-data-update | 381.873 ms | 451.327 ms | 1.1598 [1.1301, 1.1947] | 54,863,652 → 67,985,200 |
| merge-1000x10 | mount | 578.142 ms | 608.999 ms | 1.0651 [1.0346, 1.0963] | 212,192,794 → 222,053,544 |
| merge-1000x10 | same-data-update | 399.587 ms | 438.238 ms | 1.1331 [1.1066, 1.1614] | 54,939,376 → 68,060,872 |
| micro | both-disjoint (1 entries per dictionary) | 121.341 ns | 95.593 ns | 0.7890 [0.7553, 0.8224] | 328 → 216 |
| micro | both-disjoint (4 entries per dictionary) | 347.022 ns | 226.365 ns | 0.6621 [0.6394, 0.6907] | 968 → 440 |
| micro | both-disjoint (8 entries per dictionary) | 581.371 ns | 441.263 ns | 0.7702 [0.7412, 0.8012] | 1,248 → 608 |
| micro | both-disjoint (16 entries per dictionary) | 1104.041 ns | 831.954 ns | 0.7261 [0.6647, 0.7679] | 1,808 → 1,168 |
| micro | both-empty (0 entries per dictionary) | 5.030 ns | 5.572 ns | 1.1184 [1.0837, 1.1540] | 0 → 0 |
| micro | both-null (0 entries per dictionary) | 3.045 ns | 2.934 ns | 0.9691 [0.9210, 1.0202] | 0 → 0 |
| micro | both-overlap (1 entries per dictionary) | 127.088 ns | 100.127 ns | 0.7978 [0.7669, 0.8378] | 328 → 216 |
| micro | both-overlap (4 entries per dictionary) | 307.077 ns | 276.078 ns | 0.9009 [0.8572, 0.9453] | 440 → 328 |
| micro | both-overlap (8 entries per dictionary) | 572.933 ns | 493.285 ns | 0.8707 [0.8362, 0.9105] | 552 → 440 |
| micro | both-overlap (16 entries per dictionary) | 1157.153 ns | 981.036 ns | 0.9027 [0.8541, 0.9571] | 720 → 608 |
| micro | legacy-empty (1 entries per dictionary) | 6.578 ns | 6.911 ns | 1.0627 [1.0447, 1.0810] | 0 → 0 |
| micro | legacy-empty (4 entries per dictionary) | 6.674 ns | 6.907 ns | 1.0561 [1.0366, 1.0772] | 0 → 0 |
| micro | legacy-empty (8 entries per dictionary) | 6.656 ns | 6.945 ns | 1.0501 [1.0119, 1.0865] | 0 → 0 |
| micro | legacy-empty (16 entries per dictionary) | 6.720 ns | 7.017 ns | 1.0114 [0.9191, 1.1067] | 0 → 0 |
| micro | legacy-only (1 entries per dictionary) | 3.896 ns | 3.621 ns | 0.9458 [0.9179, 0.9753] | 0 → 0 |
| micro | legacy-only (4 entries per dictionary) | 3.777 ns | 3.671 ns | 0.9626 [0.9296, 0.9972] | 0 → 0 |
| micro | legacy-only (8 entries per dictionary) | 3.831 ns | 3.640 ns | 0.9408 [0.9080, 0.9758] | 0 → 0 |
| micro | legacy-only (16 entries per dictionary) | 3.915 ns | 3.638 ns | 0.9297 [0.8768, 0.9745] | 0 → 0 |
| micro | shared-empty (1 entries per dictionary) | 4.896 ns | 5.588 ns | 1.1222 [1.0815, 1.1645] | 0 → 0 |
| micro | shared-empty (4 entries per dictionary) | 4.959 ns | 5.508 ns | 1.0870 [1.0328, 1.1402] | 0 → 0 |
| micro | shared-empty (8 entries per dictionary) | 4.853 ns | 5.585 ns | 1.1437 [1.1189, 1.1685] | 0 → 0 |
| micro | shared-empty (16 entries per dictionary) | 5.142 ns | 5.559 ns | 1.1412 [1.0810, 1.2132] | 0 → 0 |
| micro | shared-only (1 entries per dictionary) | 4.964 ns | 3.640 ns | 0.7263 [0.7120, 0.7400] | 0 → 0 |
| micro | shared-only (4 entries per dictionary) | 4.970 ns | 3.644 ns | 0.7338 [0.7205, 0.7456] | 0 → 0 |
| micro | shared-only (8 entries per dictionary) | 5.013 ns | 3.622 ns | 0.7331 [0.7170, 0.7494] | 0 → 0 |
| micro | shared-only (16 entries per dictionary) | 5.044 ns | 3.611 ns | 0.7125 [0.6904, 0.7321] | 0 → 0 |
| none-1000x10 | changed-data-update | 180.087 ms | 182.421 ms | 1.0096 [0.9705, 1.0491] | 29,567,768 → 29,567,840 |
| none-1000x10 | mount | 421.907 ms | 433.398 ms | 1.0179 [0.9877, 1.0445] | 178,855,942 → 178,856,128 |
| none-1000x10 | same-data-update | 149.188 ms | 146.688 ms | 0.9932 [0.9587, 1.0236] | 29,163,512 → 29,163,512 |
| none-100x10 | changed-data-update | 9.811 ms | 9.466 ms | 0.9712 [0.8443, 1.1304] | 2,977,840 → 2,977,840 |
| none-100x10 | mount | 22.582 ms | 21.517 ms | 0.9664 [0.8790, 1.0667] | 17,077,452 → 17,077,288 |
| none-100x10 | same-data-update | 11.287 ms | 10.201 ms | 0.9574 [0.8308, 1.1175] | 2,937,152 → 2,937,088 |
| none-2000x20 | changed-data-update | 785.872 ms | 770.744 ms | 0.9946 [0.9655, 1.0256] | 116,750,424 → 116,750,424 |
| none-2000x20 | mount | 1635.676 ms | 1608.762 ms | 1.0007 [0.9752, 1.0319] | 675,812,670 → 675,812,418 |
| none-2000x20 | same-data-update | 655.011 ms | 651.258 ms | 1.0153 [0.9757, 1.0583] | 114,989,544 → 114,989,544 |
