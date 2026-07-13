# M18 — File Upload / Documents · Test Cases

> `TC_M18_<NNN>`. Source-verified; runtime cần MinIO + DB.

## Upload validation / Security
| TC | Name | Steps | Expected | Kết quả |
|---|---|---|---|---|
| TC_M18_001 | Chặn extension ngoài whitelist (High) | upload `.exe`/`.html`/`.svg` | Throw "Chỉ hỗ trợ PDF, ảnh, Word, Excel" | **Pass** (AllowedExtensions) |
| TC_M18_002 | Chặn file quá lớn (Med) | upload > MaxUploadBytes | Throw "vượt quá giới hạn" | **Pass** |
| TC_M18_003 | Chặn file rỗng (Low) | upload size 0 | Throw "File rỗng" | **Pass** |
| TC_M18_004 | Path traversal qua tên file (High) | tên `../../etc/passwd.pdf` | objectKey server-gen (GUID), không chèn path; display sanitize | **Pass** (BuildStoredFileName + SanitizeFileName) |
| TC_M18_005 | Tên Unicode/tiếng Việt (Low) | `hồ sơ.pdf` | Lưu OK, display giữ tên sạch | **Pass** |
| TC_M18_006 | Content-Type giả `text/html` (Med) | `.pdf` + content-type text/html + nội dung HTML | Tải render? | **OBS-M18-02** — MinIO khác origin nên không chạm session app; đề xuất hardening |

## Authorization
| TC | Name | Role | Expected | Kết quả |
|---|---|---|---|---|
| TC_M18_010 | Parent/Student/Agent/CTV không upload (High) | non-staff | Form upload ẩn (`_canUpdateCandidate=false`); UploadDocument re-check | **Pass** (CandidateDetail:741) |
| TC_M18_011 | Chỉ tải ứng viên trong scope (High) | agent/parent | Chỉ thấy/tải doc ứng viên của mình (trang scoped M05) | **Pass** (scope trang) |
| TC_M18_012 | DownloadDocument không re-check scope (Med) | — | Hàm không verify scope, nhưng versionId server-side + trang scoped → không exploit | **OBS-M18-01** (defense-in-depth) |
| TC_M18_013 | Restore version kiểm CandidateId (Med) | staff | Chỉ restore version thuộc `document.CandidateId == Id` | **Pass** (dòng 2132) |

## Versioning / Functional
| TC | Name | Expected | Kết quả |
|---|---|---|---|
| TC_M18_020 | Upload cùng loại → version+1 | VersionNumber tăng, CurrentVersionId cập nhật | Source-verified (2049-2065) |
| TC_M18_021 | Audit upload/restore | Ghi `create`/`upload_version`/`restore_version` | Source-verified |
| TC_M18_022 | Presigned URL hết hạn | URL không dùng được sau expiry | Source-verified (WithExpiry) |
| TC_M18_023 | Xóa ứng viên xóa doc DB | DocumentVersions + CandidateDocuments removed | Source-verified (1466-1467); object MinIO orphan → OBS-M18-03 |
