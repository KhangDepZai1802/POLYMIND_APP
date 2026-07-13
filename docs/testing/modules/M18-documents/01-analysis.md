# M18 — File Upload / Documents · Analysis

## 1. Module Overview
- **Module ID:** M18 · **Name:** File Upload / Documents (Hồ sơ ứng viên trên MinIO)
- **Purpose:** Upload/tải/quản lý phiên bản hồ sơ ứng viên (CCCD, hộ chiếu, hợp đồng…) lưu trên MinIO (S3), có versioning + audit. Dùng chung storage cho attachment tin nhắn (M14) + đính kèm đào tạo (M08).
- **Actor/Role:** Upload = staff sửa hồ sơ (`CanEditCandidateProfile`: SuperAdmin/RM/Recruiter/Consultant/DocumentStaff). Tải = ai xem được CandidateDetail (scope M05).
- **Dependencies:** M05 Candidate (scope + IDOR đã Verified), MinIO.
- **Entry:** khu "Hồ sơ ứng viên" trong `CandidateDetail`; tương tự LeadDetail/TrainingDetail (đính kèm).

## 2. Source Code Map
| File | Vai trò |
|---|---|
| `Storage/IDocumentStorage.cs` | Interface upload/download; `UploadedDocumentObject` |
| `Storage/MinioDocumentStorage.cs` | Upload (objectKey **do server tạo**), presigned download, whitelist, sanitize |
| `Storage/MinioStorageOptions.cs` | Endpoint/AccessKey/SecretKey/Bucket/MaxUploadBytes/PresignedUrlExpirySeconds |
| `CandidateDetail.razor` | UI upload/tải/version (dòng 735-906, `UploadDocument`:2027, `DownloadDocument`:2100, `RestoreVersion`:2131) |
| Entity `CandidateDocument`, `DocumentVersion` | Domain — 1 doc/loại; nhiều version, `CurrentVersionId` |

## 3. UI Inventory
- Khu "Hồ sơ ứng viên (N)": form upload (loại + file + ghi chú) **chỉ khi `_canUpdateCandidate`**; bảng tài liệu + nút Tải + lịch sử version (restore).

## 4. API / Storage Inventory
| Thao tác | Cơ chế | Auth | Validation |
|---|---|---|---|
| Upload | `MinioDocumentStorage.UploadAsync` → PutObject | `_canUpdateCandidate` (staff) | size ≤ Max, extension whitelist, SanitizeFileName; **objectKey = `candidates/{candidateId:N}/{docType}/{ts}-{guid}{ext}`** (server-gen, không lấy tên user) |
| Download | `GetDownloadUrlAsync(objectKey)` → **presigned URL** MinIO (hết hạn N giây) | qua trang scoped; **không** re-check scope trong hàm | — |
| Không có REST endpoint app cho document (tải trực tiếp qua URL presigned MinIO `:9000`, khác origin app `:5177`) |

## 5. Database Impact
- Ghi: `CandidateDocuments`, `DocumentVersions` (+ audit `create`/`upload_version`/`restore_version`). Xóa ứng viên → xóa 2 bảng này (nhưng **không** xóa object MinIO — OBS-M18-03).

## 6. Roles & Permissions
| Action | Ai | Nguồn |
|---|---|---|
| Upload/restore version | `_canUpdateCandidate` (SuperAdmin/RM/Recruiter/Consultant/DocumentStaff) | CandidateDetail:741, 2124-2130 |
| Tải | Ai xem được CandidateDetail của ứng viên đó (scope M05 Verified) | CandidateDetail:2100 |
| Parent/Student/Agent/CTV upload | **KHÔNG** (không `CanEditCandidateProfile`) | — |

## 7. Risk Analysis
- **Path traversal — ĐÓNG:** objectKey do server tạo (candidateId + docType + timestamp + GUID + ext), **không** dùng tên file người dùng làm path. `SanitizeFileName` + `Path.GetFileName` cho display name. Không chèn `../`.
- **Extension whitelist — ĐÓNG:** chỉ pdf/ảnh/office/audio; **loại** `.html`/`.svg`/executable → không stored-XSS web-renderable trực tiếp.
- **Size limit — ĐÓNG:** `MaxUploadBytes` cả UploadObject lẫn audio.
- **[OBS-M18-01, defense-in-depth] `DownloadDocument`/attachment không re-check candidate scope:** load `DocumentVersion` theo `versionId` rồi cấp presigned URL, không xác minh doc thuộc ứng viên trong scope. **Không khai thác được** hiện tại: (a) versionId chỉ render cho ứng viên trang đang scope (M05 Verified), (b) Blazor Server bind delegate + tham số **server-side** → client không gọi được với id tùy ý. Nên vẫn re-check scope cho chắc.
- **[OBS-M18-02, Low] Content-Type do client cung cấp** (`file.ContentType`) lưu nguyên; tải qua `Nav.NavigateTo(forceLoad)` tới MinIO. Nếu upload `.pdf` nhưng content-type `text/html` + nội dung HTML → có thể render. **Giảm nhẹ:** upload staff-only + MinIO **khác origin** (`:9000`) nên script chạy ở origin storage, **không** chạm cookie/session app. Đề xuất: server tự suy content-type theo ext, hoặc `Content-Disposition: attachment`.
- **[OBS-M18-03, Low] Xóa ứng viên để lại object MinIO orphan:** `DeleteRange` chỉ xóa DB rows, không xóa object trong bucket → rác storage (cùng lớp M14-OBS-02).
- **[OBS-M18-04, Low] Presigned URL chia sẻ được trong thời gian hết hạn** (bản chất presigned). Cấu hình `PresignedUrlExpirySeconds` hợp lý.

## 8. Unknowns
- Không có điểm nghiệp vụ cần user chốt cho M18 (các observation là hardening kỹ thuật, không phải quyết định nghiệp vụ).
