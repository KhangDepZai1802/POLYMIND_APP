# M18 — File Upload / Documents · Bug Report

> Chỉ ghi bug có bằng chứng source. Quy ước `BUG_M18_<NN>`.

## Kết luận: **No Confirmed Bugs** (Verified code-level)

Thuộc tính bảo mật lõi đúng ở source:
- **Path traversal — đóng:** objectKey **do server tạo** (`candidates/{candidateId:N}/{docType}/{ts}-{guid}{ext}`), không dùng tên file người dùng làm path; `SanitizeFileName`/`Path.GetFileName` cho display.
- **Extension whitelist — đóng:** chỉ pdf/ảnh/office/audio; loại html/svg/executable.
- **Size limit — đóng:** `MaxUploadBytes`.
- **Upload authz — đóng:** chỉ staff `CanEditCandidateProfile`; parent/student/agent/CTV không upload được.
- **Tải theo scope trang:** versionId chỉ render cho ứng viên trong scope (M05 Verified); Blazor Server không cho gọi hàm với id tùy ý.
- **Audit đủ:** create/upload_version/restore_version. Restore kiểm `document.CandidateId == Id`.

---

## Observations (hardening — không handoff Codex trừ khi user muốn siết)

- **OBS-M18-01 — `DownloadDocument`/attachment không re-check candidate scope (defense-in-depth, Low):** load `DocumentVersion` theo `versionId` → cấp presigned URL, không xác minh doc thuộc ứng viên trong phạm vi người gọi. **Không khai thác được hiện tại** (versionId server-side + trang scoped). **Đề xuất:** trong `DownloadDocument`, join `DocumentVersion→CandidateDocument.CandidateId` và re-check quyền xem ứng viên đó (nhất quán với M14-OBS-05, M16-OBS-01). Cùng lớp latent-IDOR presigned URL.
- **OBS-M18-02 — Content-Type do client cung cấp lưu nguyên (Low):** upload `.pdf` với content-type `text/html` + nội dung HTML → tải qua presigned URL có thể render. **Giảm nhẹ:** upload staff-only + MinIO khác origin (`:9000`) nên không chạm cookie/session app. **Đề xuất:** server suy content-type theo extension, hoặc set `Content-Disposition: attachment` khi tạo presigned/serving.
- **OBS-M18-03 — Xóa ứng viên để lại object MinIO orphan (Low, data-hygiene):** `DeleteRange` xóa `DocumentVersions`/`CandidateDocuments` DB nhưng không xóa object bucket → rác storage (cùng lớp M14-OBS-02 recall). **Đề xuất:** xóa object MinIO theo `FileUrl` khi xóa ứng viên/version.
- **OBS-M18-04 — Presigned URL chia sẻ được trong thời gian hết hạn (Low):** bản chất presigned; giữ `PresignedUrlExpirySeconds` ngắn hợp lý.

## Codex Handoff Queue

| Order | Bug ID | Severity | Status |
|---:|---|---|---|
| — | — | — | Không có confirmed bug. OBS-M18-01/02/03 là hardening — chỉ handoff nếu user muốn siết defense-in-depth. |

> **Kết luận M18:** `QA=No Confirmed Bugs`, `Codex=Not Required`, `Verification=Verified (code)`. Observations là hardening kỹ thuật (không phải quyết định nghiệp vụ). Runtime MinIO E2E pending harness.
