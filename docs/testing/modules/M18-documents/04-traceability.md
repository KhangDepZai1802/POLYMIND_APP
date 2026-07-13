# M18 — File Upload / Documents · Traceability

| Business Flow | Source | Role | Test Cases | Automated | Coverage | Gap |
|---|---|---|---|---|---|---|
| BF-M18-01 Upload | MinioDocumentStorage.UploadAsync / CandidateDetail | staff | TC_M18_001..005/010/020 | — | Source-verified | Runtime MinIO |
| BF-M18-02 Tải | GetDownloadUrlAsync / DownloadDocument | scoped | TC_M18_011/012/022 | — | Source-verified; OBS-M18-01 | Runtime IDOR probe |
| BF-M18-03 Version | CandidateDetail restore | staff | TC_M18_013/021 | — | Source-verified | Runtime |
| BF-M18-04 Phân quyền | _canUpdateCandidate | all | TC_M18_010/011 | — | Source-verified | Runtime role probe |
| BF-M18-05 Xóa | CandidateDetail delete | staff | TC_M18_023 | — | Source-verified; OBS-M18-03 | Runtime orphan check |

## Gap analysis
- **Automation:** Storage (`MinioDocumentStorage`) + upload/download đều ở `Polymind.Web` + cần MinIO → **0 automated test** (project không ref Web). Cần harness MinIO (Testcontainers) + WebApplicationFactory.
- **Runtime:** cần MinIO + DB để E2E upload/tải/version; xác nhận không path traversal thực tế; probe IDOR download (OBS-M18-01); kiểm content-type (OBS-M18-02); orphan object (OBS-M18-03).
