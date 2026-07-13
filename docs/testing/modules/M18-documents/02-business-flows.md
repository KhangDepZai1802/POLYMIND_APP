# M18 — File Upload / Documents · Business Flows

## BF-M18-01 — Upload hồ sơ (staff)
- **Actor:** staff `CanEditCandidateProfile`.
- **Main:** chọn loại hồ sơ + file → `UploadAsync` (validate size/ext/sanitize, objectKey server-gen) → tạo/cập nhật `CandidateDocument` + `DocumentVersion` (version+1) → set `CurrentVersionId` → audit `create`/`upload_version`.
- **Validate:** size ≤ Max; extension whitelist; file rỗng bị chặn.

## BF-M18-02 — Tải hồ sơ
- **Main:** bấm Tải → `DownloadDocument(versionId)` → load version → `GetDownloadUrlAsync(FileUrl)` → presigned URL → `NavigateTo(forceLoad)`.
- **Auth:** qua trang CandidateDetail đã scope (M05). Hàm không re-check scope (OBS-M18-01, không exploit).

## BF-M18-03 — Quản lý version
- Restore version cũ → `_canUpdateCandidate` + kiểm `document.CandidateId == Id` (dòng 2132) → đổi `CurrentVersionId` + audit `restore_version`.

## BF-M18-04 — Phân quyền
| Role | Upload/Restore | Tải |
|---|---|---|
| SuperAdmin/RM/Recruiter/Consultant/Document | ✓ | ✓ (ứng viên trong scope) |
| VisaStaff/Accountant | ✗ (không CanEditCandidateProfile) | ✓ nếu xem được ứng viên |
| Agent/CTV/Parent/Student | ✗ | ✓ chỉ ứng viên của mình (scope M05) |

## BF-M18-05 — Xóa ứng viên
- `DeleteRange(DocumentVersions + CandidateDocuments)` — xóa DB; **không** xóa object MinIO (OBS-M18-03).
