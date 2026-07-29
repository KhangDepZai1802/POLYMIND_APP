# 📓 WORKLOG — POLYMIND OLMS (file phối hợp giữa các session AI)

> **File này là nguồn sự thật chung cho Claude và Codex.** Đọc TRƯỚC khi làm, cập nhật SAU khi làm.

---

## ⚠️ QUY TẮC BẮT BUỘC (đọc mỗi lần)

1. **TRƯỚC khi bắt đầu session:** đọc hết file này — phần `TRẠNG THÁI HIỆN TẠI` + `VIỆC TIẾP THEO` + entry mới nhất trong `NHẬT KÝ`.
2. **Chọn việc:** làm theo `VIỆC TIẾP THEO`. Nếu muốn làm khác, ghi rõ lý do vào nhật ký.
3. **SAU khi làm xong (hoặc trước khi hết session):**
   - Cập nhật `TRẠNG THÁI HIỆN TẠI` (1-2 dòng).
   - Cập nhật `VIỆC TIẾP THEO` (việc kế cho người sau — càng cụ thể càng tốt).
   - **Thêm 1 entry mới** vào đầu `NHẬT KÝ SESSION` (mới nhất ở trên).
   - **GIỮ GỌN (bắt buộc):** chỉ giữ **6 mục gần nhất** ở `TRẠNG THÁI HIỆN TẠI` và **6 entry gần nhất** ở `NHẬT KÝ SESSION`. Khi thêm mục/entry mới → **xóa mục/entry cũ nhất** để file không phình to (đỡ tốn token đọc lại mỗi phiên).
   - Nếu có blocker/lỗi chưa fix → ghi vào `BLOCKERS`.
4. **Luôn để app build được** (`dotnet build Polymind.slnx`) trước khi kết thúc. Nếu để dở, ghi rõ "đang dở, chưa build".
5. Chi tiết backlog đầy đủ + bẫy kỹ thuật + quy ước code: xem **docs/05-handoff-codex.md** (đọc 1 lần để nắm nền).

**Format 1 entry nhật ký:**
```
### [YYYY-MM-DD] Session N — <Claude|Codex>
- **Làm được:** ...
- **File thay đổi chính:** ...
- **Đã test:** ... (build? chạy? smoke test gì?)
- **Lưu ý/cảnh báo cho người sau:** ...
```

---

## 🔒 RÀNG BUỘC NGHIỆP VỤ — CHỐT VỚI USER 2026-07-10 (BẮT BUỘC — Claude + Codex phải tuân)

> 7 yêu cầu user đã chốt (kèm quyết định qua hỏi–đáp). **Trạng thái: ĐÃ CODE XONG (Session 63) — BUILD 0/0, CHƯA smoke test trình duyệt (Docker/DB local chưa chạy) → cần duyệt mắt.** KHÔNG tự đổi quyết định đã chốt; nếu vướng, hỏi user trước.

**✅ RB-1 — Ẩn thông tin nhạy cảm CTV với Phụ huynh/Học sinh.** Với role `parent` và `student`, mọi nơi hiển thị thông tin CTV (card/modal "Thông tin cộng tác viên") PHẢI ẩn đúng 2 dòng: **"Ứng viên đã giới thiệu"** và **"Tỷ lệ hoa hồng CTV %"**. Các dòng khác (tên, đại lý, SĐT, email, trạng thái) vẫn hiện. Kiểm tra qua `AgentScope.IsParent/IsStudent`.

**✅ RB-2 — Chỉ super_admin đổi TVV/CTV + card Job trên chi tiết ứng viên.**
- Chỉ `super_admin` được đổi **TVV (consultant)** và **CTV (collaborator)** đã gắn vào ứng viên; role khác chỉ xem.
- Thêm **card riêng "Đơn hàng (Job)"** trên trang chi tiết ứng viên (giống card chọn job lúc convert lead→ứng viên). **1 ứng viên = 1 job active** (đổi job = thay job cũ).
- Đổi job / đổi TVV / đổi CTV: chỉ super_admin, mỗi thao tác PHẢI qua hộp xác nhận **"Chắc chưa?" + nhập lại mật khẩu đăng nhập của CHÍNH super admin** (reuse `ConfirmPasswordDialog`; verify bằng `SignInManager.CheckPasswordSignInAsync` với user hiện tại). Ghi audit cho mỗi lần đổi.

**✅ RB-3 — Tìm kiếm theo tên ở trang quản lý tài khoản Phụ huynh & Học sinh.** Trang admin `/admin/parents-students` (`ParentStudentAccounts.razor` / `AccountManagerPanel`): thêm ô **search theo tên** (lọc họ tên, có thể kèm email). (Không phải search trong portal của PH/HS.)

**✅ RB-4 — Đổi mật khẩu Phụ huynh/Học sinh.** User `parent`/`student` **tự đổi mật khẩu** trong portal (`/me`), dùng Identity — **KHÔNG lưu plaintext**. Super admin **chỉ có nút "Đặt lại mật khẩu"** (đã có ở `AccountManagerPanel` — bảo đảm chạy được), **KHÔNG xem được** mật khẩu user tự đặt. (User đã chọn phương án an toàn thay vì lưu plaintext.)

**✅ RB-5 — Giữ dữ liệu AI trong 1 phiên đăng nhập.** Hội thoại AI + kết quả trích xuất CV/ảnh PHẢI **lưu server-side theo `UserId`**, sống sót khi chuyển chức năng **và khi F5/refresh**; chỉ **mất khi đăng xuất**. Không để state chỉ trong component (mất khi điều hướng). Gợi ý: bảng/cache theo user + nạp lại khi mở trang AI + dọn khi logout (hook logout endpoint).

**✅ RB-6 — Bấm thông báo → tới đúng trang nguồn.** Click 1 thông báo (chuông + trang thông báo) PHẢI điều hướng theo `Notification.ReferenceType` + `ReferenceId`: lead→`/leads/{id}`, candidate→`/candidates/{id}`, agent→`/agents/{id}`, payment/finance→trang tài chính ứng viên, visa→`/visas/...`, v.v.; đánh dấu đã đọc khi click. Thiếu reference thì không điều hướng.

**✅ RB-7 — Bổ sung thông báo cho các module (user chốt: Tài chính, Hoa hồng & Đại lý, Visa & Xuất cảnh; KHÔNG thêm nhóm Tài khoản/Bảo mật).** (Visa/Xuất cảnh đã có sẵn; bổ sung mới: hoa hồng chờ duyệt, kỳ trả nợ vay đến hạn, khoản chi chờ duyệt.)
- **Tài chính** → Kế toán + Director/super_admin: khoản thu đến hạn/quá hạn, công nợ ứng viên, khoản chi chờ duyệt, khoản vay (Loan) đến hạn trả.
- **Hoa hồng & Đại lý** → CTV/Đại lý liên quan + Kế toán: hoa hồng phát sinh → chờ duyệt → đã chi.
- **Visa & Xuất cảnh** → Visa staff + RM: nộp/bổ sung/kết quả visa, lịch phỏng vấn sắp tới, lịch bay sắp tới, xác nhận xuất cảnh.
- Mọi thông báo mới PHẢI set `ReferenceType`/`ReferenceId` để RB-6 điều hướng được.

---

## 🎯 TRẠNG THÁI HIỆN TẠI

- **ORACLE PRODUCTION (Session 71, Codex) — CHỜ USER TẠO VM.** User đổi quyết định và chốt **Oracle Cloud Always Free** ngày 29/07/2026. Đã chuẩn bị bundle 2 MB, bootstrap Docker/firewall/swap, sinh secret, deploy Caddy/PostgreSQL/MinIO/Web, persist Data Protection keys và chạy web non-root. Release build 0 warning/0 error, **236/236 test pass**, Compose + Bash + Dockerfile ARM64 static check hợp lệ. Checklist chính thức: `docs/06-deploy-oracle.md`; `docs/09-deploy-vps-vn.md` chỉ còn là fallback trả phí. Cần user gửi public IP + đường dẫn SSH key + domain để Codex tiếp tục SSH deploy/runtime smoke-test.
- **QA VERIFY M11 + QA M14→M18 (Session 69, Claude) — 88/88 TEST PASS, WEB 0/0.** **M11 Verified Fixed (code + RUNTIME):** áp migration `20260711123000` lên DB test (đã DROP), DB PoC unique-index chặn thu-trùng kỳ. **M14** đồng bộ board (No Confirmed Bugs). **M15 AI — BUG_M15_01 (Med):** đại lý/CTV lộ toàn bộ ứng viên qua Trợ lý AI. **M16 Reports — BUG_M16_01 (Low):** export bỏ qua khoảng thời gian. **M17 Dashboard — No Confirmed Bugs** (authz staff-only + partner redirect; portal `/me` cô lập; OBS-M17-01/U-M17-1 KPI tài chính hiện cho mọi staff). **M18 Documents — No Confirmed Bugs** (MinIO objectKey server-gen không path traversal, whitelist, upload staff-only; 3 obs hardening). **✅ USER ĐÃ CHỐT 5 quyết định (U-M13-1/2, U-M14-1, U-M15-1, U-M16-1)** → chuyển Codex thực thi; **U-M17-1 mới đang mở** (không blocking). KHÔNG sửa business logic, không đụng production DB.
- **QA BUG-FIX M11 + M13 (Session 68, Codex) — 86/86 TEST PASS, WEB BUILD 0/0.** M11 BUG_M11_01 + CR-M11-1/2/3 đã Fixed: Company-only gate, finance-only collection, Income Receipt, Thu hết, no-forgiveness; migration chưa áp DB; **chờ Claude verify**. M13 đã sửa phần chắc chắn của BUG_M13_01: Agent owner + Accountant/Director nhận Pending/Approved/Paid, null-safe; toàn bug chuyển **Needs Requirement Clarification** vì CTV direct/all-tree và mức tiền được xem chưa chốt.
- **QA BUG-FIX M12 VISA & FLIGHT (Session 67, Codex) — 2 BUG FIXED, CLAUDE ĐÃ VERIFIED CODE-LEVEL.** BUG_M12_01/02 dùng authenticated actor qua `VisaFlightCreationRules`; visa reminder route đúng `HandledBy`. Codex 64/64 + Web 0/0; Claude phiên #5 đã xác minh độc lập.
- **QA BUG-FIX BATCH M01→M04 (Session 65, Codex) — QUEUE ĐÃ HẾT, 6 BUG FIXED, 29/29 TEST PASS, SOLUTION BUILD 0/0.** M01: khóa user/gỡ link đổi security stamp + revalidation chặn `IsActive=false`, login web/API dùng phản hồi chung chống enumeration. M02: Candidate REST API lọc data-scope fail-closed + đổi role-permission invalidate phiên (Claude đã **Verified code-level**, runtime HTTP pending). M03: xóa user dọn cả `OwnerUserId` và `ParentUserId`. M04: Lead convert ghi `Candidate.CreatedBy` đúng actor. **Bổ sung quyết định:** accountant được approve thu/chi/hoa hồng/vay; role map hiện tại đúng, TC_M02_021 đã resolved. Đã tạo/cập nhật tài liệu QA; M01/M03/M04 đang chờ Claude xác minh độc lập. Không migration, không đụng production data, chưa restart dev host `:5177`.
- **2 CHỈNH SỬA THEO YÊU CẦU USER (Session 64, Claude) — CODE XONG, BUILD 0/0, CHƯA restart/duyệt mắt/commit.** (1) **Đổi đơn hàng = RESET quy trình 20 bước.** Trên chi tiết ứng viên, khu "Đổi đơn hàng (chỉ Super Admin)": khi super admin đổi sang đơn hàng mới, `ChangeJobOrderAsync` nay set `CurrentStep = WorkflowStep.Lead` (B1) + `Status = Active` + **xóa toàn bộ `WorkflowStepRecords`** của CJO đó → tiến trình bắt đầu sạch từ đầu (trước đây RB-2 giữ nguyên tiến trình). Cập nhật caption + text xác nhận mật khẩu + snackbar + audit (log cả `CurrentStep` cũ/mới). **Lưu ý:** chỉ reset workflow, KHÔNG hoàn tiền/hoa hồng đã phát sinh. (2) **Bỏ Đại lý & CTV khỏi dropdown vai trò ở trang Quản trị.** `AccountManagerPanel` thêm param `CreatableRoles` (mặc định = `ManagedRoles`) tách "vai trò được tạo/gán qua dropdown" khỏi "vai trò hiển thị/quản lý"; dropdown tạo + dropdown chuyển vai trò dùng `CreatableSet`; tài khoản mang role ngoài `CreatableSet` vẫn hiện nhưng coi như **cố định** (chip khóa, không đổi role). `Admin.razor` truyền `CreatableRoles=StaffCreatableRoles` (= StaffRoles trừ agent/collaborator) → trang `/admin` KHÔNG còn tạo/đổi sang Đại lý/CTV (chỉ tạo ở trang Đại lý & Hoa hồng `/agents/{id}`); trang P&S không đổi (CreatableRoles null → fallback ManagedRoles).
## ⏭️ VIỆC TIẾP THEO (baton — làm cái này trước)

**#-3 — TRIỂN KHAI PRODUCTION ORACLE ALWAYS FREE (user chốt 2026-07-29) — ĐANG Ở GĐ1, CHỜ USER TẠO VM:**
- **GĐ0 chuẩn bị deploy: XONG** — xem `docs/06-deploy-oracle.md`; bundle hiện tại ở `artifacts/polymind-oracle-deploy.tar.gz`.
- **GĐ1 (user bắt buộc làm):** tạo/xác minh Oracle account; Home Region Singapore; tạo `VM.Standard.A1.Flex` 2 OCPU/8GB, Ubuntu 24.04 ARM64, boot 100GB, public IPv4; tải private SSH key; mở ingress 80/443 và 22; trỏ domain/DuckDNS vào public IP. **User chỉ gửi lại IP + đường dẫn key local + domain, không gửi nội dung private key/OTP/thẻ.**
- **GĐ2 (Codex làm khi có thông tin):** SSH, upload bundle, bootstrap, tạo secret, deploy, migration và runtime smoke-test theo `docs/06-deploy-oracle.md`.
- **⚠️ CHƯA VERIFY RUNTIME (Session 70 build sạch nhưng Docker local chưa chạy):** phải smoke-test ở GĐ2 — **CSP mới** (upload ảnh CCCD, ghi âm tin nhắn thoại `blob:`, Trợ lý AI), **duyệt khoản chi** (chưa duyệt không xuất được phiếu chi), **nút "Xác nhận đã bay"**, rate limit `POST /login`. Nếu CSP chặn nhầm: đặt env `Security__ContentSecurityPolicy` nới hơn (hoặc rỗng để tắt), KHÔNG cần build lại.
- **Sau go-live, theo thứ tự user đã chốt:** GĐ5 PWA + CI/CD (0đ) → GĐ6 SMS OTP 2FA (**tốn phí**) → GĐ7 Zalo ZNS (**tốn phí**) → GĐ8 app MAUI (**tốn phí**, cuối cùng).

**#-2 — CODEX THỰC THI QUEUE (user đã chốt 4 quyết định 2026-07-11); CLAUDE QA M17→M20:**
- **M11 đã VERIFIED** (code + runtime migration/DB PoC) — không làm lại.
- **User ĐÃ CHỐT (2026-07-11)** — Codex thực thi theo `docs/testing/SESSION_CHECKPOINT.md` mục "🟩 QUYẾT ĐỊNH": **(1) BUG_M15_01** — AI cho đại lý/CTV nhưng **chỉ nạp ứng viên trong phạm vi họ** (lọc AgentId/CollaboratorId). **(2) BUG_M13_01** — thông báo hoa hồng gửi **CHỈ CTV trực tiếp** (`Candidate.CollaboratorId`), CTV **chỉ thấy phần share của mình**. **(3) CR-M14-1** — giới hạn tin nhắn staff/CTV/đại lý chỉ tới phụ huynh/học viên **thuộc ứng viên mình phụ trách**. **(4) CR-M16-1** — bỏ báo cáo **tài chính** khỏi RecruitmentManager (RM chỉ báo cáo tuyển dụng). **(5) BUG_M16_01** (Low) — export truyền khoảng thời gian.
- **Còn mở nhỏ (không blocking):** U-M13-1 (recipient tài chính thêm super_admin? payment reminder owner-first?) — có đề xuất mặc định, user chốt khi rảnh.
- **+ CR-M13-1** (bỏ Giám đốc khỏi finance recipients, `NotificationService.cs:266-270`) **+ CR-M17-1** (ẩn KPI tài chính Home dashboard, chỉ Director/Accountant/SuperAdmin, `Home.razor`). **Change request đã chốt trước:** M08 (U-M08-1), M09 (U-M09-1/2), M10 (U-M10-1), M12 (U-M12-1/2).
- **✅ User đã chốt HẾT 6 quyết định** (U-M13-1/2, U-M14-1, U-M15-1, U-M16-1, U-M17-1) → **7 mục Codex Queue** đã hết gate. Không còn gì chờ user.
- **Claude:** đã xong QA M17/M18 (No Confirmed Bugs). QA tiếp **M19 Audit → M20 Security**; sau khi Codex fix M13/M15/M16 thì Claude verify.

**#-1 — DUYỆT MẮT 2 CHỈNH SỬA Session 64 (CODE XONG, BUILD 0/0, CHƯA commit):**
- Chi tiết ứng viên (super_admin) → khu "Đổi đơn hàng": đổi sang đơn mới → xác nhận mật khẩu → tiến trình phải nhảy về **B1 (Lead)**, timeline sạch, `WorkflowStepRecords` cũ đã xóa. Kiểm cả ứng viên đang ở bước giữa (VD B8/B15).
- Trang `/admin` tab Tài khoản: form "Tạo tài khoản" dropdown vai trò **KHÔNG còn Đại lý/CTV**; dropdown "Chuyển vai trò" cũng không có; tài khoản Đại lý/CTV đang có (nếu hiện) chip **"Cố định"**. Trang `/admin/parents-students` không đổi. Tạo Đại lý/CTV vẫn ở `/agents/{id}`.


**#0 — RESTART/REDEPLOY + TEST LẠI ĐĂNG NHẬP KHÔNG 2FA (Session 62 — CODE XONG, BUILD 0/0, CHƯA restart/redeploy/commit):**
- Restart web local hoặc redeploy Render để nạp code mới. Nếu web cũ đang chạy `:5177`, cần tắt/chạy lại rồi Ctrl+F5 lấy `login.css?v=20260709-login`.
- Test các tài khoản demo dùng chung mật khẩu `Admin@123` như `admin@polymind.local`, `ctv-ctv0001@polymind.local`, tài khoản Học sinh/Phụ huynh demo: sau email+mật khẩu phải vào thẳng app/landing theo role, **không** xuất hiện `/login-2fa` hoặc `/account/2fa-setup`.
- Vào `/admin` → sửa tài khoản: dialog chỉ còn họ tên/email/mật khẩu mới, không còn khối “Xác thực 2 lớp/Đặt lại 2FA”.
- Mở `POLYMIND_Báo cáo tiến độ.docx` kiểm mục “Bảo mật nâng cao” đã ghi đúng: hiện tạm bỏ 2FA để đối tác test; bản final sau này dùng SMS OTP về số điện thoại người dùng nhập.
- Khi user duyệt xong → commit gộp các thay đổi chưa commit gần đây; khi đưa vào sử dụng thật, thiết kế SMS OTP mới (provider SMS, chi phí, lưu số điện thoại, rate limit, audit) thay cho TOTP Authenticator cũ.

**#1 — TIẾP TỤC DUYỆT MẮT CỔNG CÁ NHÂN HÓA PHỤ HUYNH/HỌC SINH (Session 60 — CODE XONG, BUILD 0/0, MIGRATION ĐÃ ÁP):**
- Tạo/gắn tài khoản test ở chi tiết ứng viên đã đặt cọc (gợi ý **UV-20260608-2001 Lê Hữu Khang**), rồi đăng nhập Học sinh/Phụ huynh để kiểm `/me`, bottom-nav mobile, dữ liệu self-scoped, tin nhắn theo quan hệ và AI không lộ hồ sơ người khác. Lưu ý mới: **không cần Authenticator** khi test.
- Không hồi quy admin/nhân sự: `/candidates` có cột Trạng thái; laptop ≤1600px cột Giới tính chỉ còn icon; `/training` hiện ứng viên đã đặt cọc; ghi âm tin nhắn có nghe lại trước Gửi/Bỏ.
- Render: DB đã sync nhưng code mới còn phải redeploy; nhớ đặt env `Ai__Gemini__ApiKey` trên Render trước/hoặc cùng lúc deploy.

## 🚧 BLOCKERS / NỢ KỸ THUẬT

- **Smoke UI local Session 53:** Docker Desktop/Linux engine chưa chạy; `docker compose up -d` lỗi `dockerDesktopLinuxEngine ... The system cannot find the file specified`, nên chưa mở được `http://localhost:5177/login` để click kiểm tài khoản Đại lý/CTV. Code đã build xanh; bật Docker Desktop rồi smoke lại.
- Nợ triển khai thật: cần `.env.production` với secret thật, chứng chỉ TLS thật (ưu tiên Caddy tự cấp Let's Encrypt cho DuckDNS; Nginx manual cert vẫn là fallback), SMTP/provider SMS/Zalo thật nếu bật gửi ngoài InApp.
- **CẢNH BÁO DEPLOY PUBLIC:** Không public production nếu còn user mẫu/mật khẩu `Admin@123`. DuckDNS/No-IP chỉ giải quyết DNS động; vẫn cần public IP hoặc port-forward 80/443 hoạt động. Nếu mạng công ty bị CGNAT/không mở port được thì không cố DuckDNS, chuyển fallback tunnel.
- ~~Module placeholder chờ làm thật~~ — ĐÃ XONG HẾT (finance/agents/visa/reports đều có trang thật).

---

## 📜 NHẬT KÝ SESSION (mới nhất ở trên)

### [2026-07-21] Session 70 — Claude — Kế hoạch triển khai production + GĐ0 dọn code
- **Yêu cầu user:** lên kế hoạch triển khai bản chính thức cho các role dùng thật, dựa theo `Du_Toan_Ha_Tang_App_PolyMind_DuongA.xlsx`, ghi rõ bước nào **(tốn phí)**.
- **Quyết định hạ tầng (user chốt):** **VPS VN 4vCPU/8GB tại HCM (~700k/tháng)** + domain thật + Cloudflare Free + Backblaze B2. **Bác bỏ Cloud Run + Supabase Pro trong dự toán** vì: `Program.cs:50` là Blazor **Interactive Server** (mỗi click = 1 round-trip → RTT Singapore 40–80ms làm app nặng tay, VPS VN 5–15ms), Cloud Run **cắt WebSocket sau 60'** làm rớt circuit, Hangfire cần always-on, `pgvector` **không được dùng ở đâu** nên Supabase Pro vô ích, MinIO đã chạy nên không cần GCS. Tổng ~760k/tháng thay vì ~5,85tr/tháng (tiết kiệm ~60tr/năm). Phạm vi đợt 1: **chỉ Web**, rồi PWA+CI/CD; SMS OTP → Zalo ZNS → MAUI để sau (đều tốn phí).
- **Làm được (GĐ0):** (1) **U-M10-1 duyệt khoản chi RB-7** — `Domain/Finance/ExpenseApprovalRules.cs` mới; `Finance.razor` có chip "Chờ duyệt/Đã duyệt" + nút Duyệt (`expenses:approve`) + audit `approve`; **`CreateReceiptForExpense` chặn server-side khi chưa duyệt** (đọc lại từ DB, không tin state màn hình). (2) **U-M12-1** nút "Xác nhận đã bay"/"Bỏ xác nhận" trong `FlightDialog` ghi `ActualDepartureAt` + cột "Xuất cảnh" ở `Visas.razor` (desktop + mobile). (3) **U-M12-2** audit create/update cho visa + flight, thêm action riêng `confirm_departure`/`clear_departure`; dùng `GetUserIdAsync()` (nullable) cho audit thay vì fallback về user đầu bảng. (4) **Hardening M20:** header **CSP** mới đọc từ `Security:ContentSecurityPolicy` (mặc định `script-src 'self'` vì App.razor không có inline script; `style-src 'unsafe-inline'` cho MudBlazor; `blob:`/`data:` cho ghi âm + ảnh; `ws:`/`wss:` cho SignalR; Google Fonts) — đặt rỗng để tắt mà không cần build lại; **rate limit `POST /login` web 30/phút/IP** qua `GlobalLimiter` (các request khác NoLimiter, không đụng SignalR). (5) Xóa **service redis chết** khỏi compose production (`grep Redis src` = 0 kết quả) + `x-logging` giới hạn log 10MB×3. (6) `DbSeeder.ShouldSeedSampleUsers(bool)` tách nhánh seed thành hợp đồng có tên để test pin.
- **File thay đổi chính:** `docker-compose.production.yml`; `src/Polymind.Domain/Finance/ExpenseApprovalRules.cs` (mới); `src/Polymind.Web/Program.cs`; `Components/Pages/Finance/Finance.razor`; `Components/Pages/Visas/{FlightDialog,VisaDialog,Visas}.razor`; `src/Polymind.Infrastructure/Persistence/DbSeeder.cs`; `tests/Polymind.Tests/{M10_ExpenseApprovalRulesTests,M20_ProductionSeedGuardTests}.cs` (mới); `scripts/{backup.sh,restore.sh}` (mới); `docs/09-deploy-vps-vn.md` (mới).
- **Đã test:** `dotnet test` = **236 passed, 0 failed, 0 skipped**; `dotnet build Polymind.slnx` = **0 warning / 0 error**; `docker compose ... --profile caddy config -q` hợp lệ. **KHÔNG migration** (dùng `Expense.ApprovedBy` đã có sẵn), không đụng production data, chưa commit.
- **Lưu ý/cảnh báo cho người sau:** **CHƯA verify runtime** — Docker local chưa chạy nên CSP/rate-limit/UI mới chỉ được kiểm ở mức build + unit test. **CSP là thứ dễ làm vỡ app nhất** → smoke-test bắt buộc ở GĐ2 (upload ảnh CCCD, nghe lại ghi âm `blob:`, Trợ lý AI); vỡ thì nới bằng env `Security__ContentSecurityPolicy`, không cần build lại. **Khoản chi cũ trong DB đều `ApprovedBy=null` → thành "Chờ duyệt" và không xuất phiếu chi được cho tới khi duyệt** (đúng thiết kế, nhưng phải báo kế toán trước khi go-live). Role map KHÔNG đổi: `expenses:approve` vẫn thuộc Accountant + SuperAdmin theo quyết định user 2026-07-10; nếu muốn Giám đốc duyệt chi thì phải chốt lại và sửa `RolePermissionMap`.

### [2026-07-11] Session 69 — Claude — Verify M11 (runtime) + QA M14→M18 + chốt 5 quyết định user
- **Làm được:** (1) **Verify M11** BUG_M11_01 + CR-M11-1/2/3 → **Verified Fixed (code + RUNTIME)**: áp migration `20260711123000` lên DB test `polymind_m11_verify`, kiểm `\d receipts` (loan_repayment_id UNIQUE), DB PoC chèn trùng bị chặn; DB test đã DROP. (2) **M14** đồng bộ board (No Confirmed Bugs). (3) **M15 AI** → **BUG_M15_01 (Med):** đại lý/CTV lộ toàn bộ ứng viên qua Trợ lý AI. (4) **M16 Reports** → **BUG_M16_01 (Low):** export bỏ qua khoảng thời gian. (5) **M17 Dashboard** → No Confirmed Bugs (authz staff-only + partner redirect; `/me` cô lập; OBS-M17-01/U-M17-1). (6) **M18 Documents** → No Confirmed Bugs (objectKey server-gen không path traversal; upload staff-only; 3 obs hardening). (7) **User chốt 5 quyết định** → ghi thành CR/hướng fix cho Codex (BUG_M15_01 lọc scope, BUG_M13_01 CTV trực tiếp, CR-M14-1 messaging scope, CR-M16-1 RM bỏ báo cáo tài chính, CR-M13-1 bỏ Director khỏi finance recipients).
- **File thay đổi chính:** `docs/testing/modules/{M11 08, M15 01→06, M16 01→06, M17 01→06, M18 01→06}`; `MODULE_QA_BOARD.md`; `SESSION_CHECKPOINT.md`; `WORKLOG.md`. **KHÔNG sửa** source ứng dụng.
- **Đã test:** `dotnet test` → **88 passed, 0 failed, 0 skipped**; Web build **0/0**; M11 migration áp sạch trên DB test.
- **Lưu ý/cảnh báo cho người sau:** M15/M16/M17/M18 không có automated test (component/endpoint/storage ở `Polymind.Web`, test project không ref Web — cố ý). **Codex queue nay hết gate** — thực thi 6 mục (BUG_M15_01, BUG_M13_01, CR-M14-1, CR-M16-1, BUG_M16_01, CR-M13-1) + change request cũ M08/M09/M10/M12. Design-time factory hardcode user `postgres` → verify migration phải dùng `--connection`. **U-M17-1 mới** chờ user (KPI tài chính dashboard). Claude QA tiếp **M19 Audit → M20 Security**.

### [2026-07-11] Session 68 — Codex — Fix M11 + xử lý phần chắc chắn M13
- **Làm được:** M11 BUG_M11_01 + CR-M11-1/2/3 Fixed (Company-only B20, Bank no-Settled, finance-only, receipt, Thu hết, no-forgiveness, migration). M13 BUG_M13_01: thêm Agent owner và Paid lifecycle an toàn; giữ finance recipients; chuyển module Needs Requirement Clarification cho CTV scope/content.
- **File thay đổi chính:** `Domain/{Loans/LoanCollectionRules,Notifications/CommissionNotificationRules}.cs`, `Enums.cs`, `Receipt.cs`, `ApplicationDbContext` + M11 migration/snapshot, `CandidateDetail`, `LoanDialog`, `DebtCollection`, `NotificationService`, `Labels`, `BusinessRoleAccess`, M11/M13 tests, module `03→07`, board/checkpoint.
- **Đã test:** toàn suite **86 passed, 0 failed, 0 skipped**; Web build output riêng **0 warning/0 error**. Không áp migration/không ghi production DB.
- **Lưu ý/cảnh báo cho người sau:** Claude verify M11 độc lập bằng DB test. M13 không được đánh dấu Fixed; user phải chốt CTV trực tiếp hay cả cây và CTV thấy tổng/share/không tiền.

### [2026-07-11] Session 67 — Codex — Fix M12 Visa & Flight attribution/reminder routing
- **Làm được:** xử lý toàn bộ queue M12. BUG_M12_01: visa mới lấy authenticated actor làm `HandledBy`, sửa nguồn recipient của visa reminder. BUG_M12_02: flight mới lấy actor làm `AssignedTo`. Tách `VisaFlightCreationRules` để khóa invariant bằng 2 regression test. Cập nhật đủ `06-bug-report`, tạo `07-fix-report`, board/checkpoint; queue Codex rỗng, M12 chờ Claude xác minh.
- **File thay đổi chính:** `src/Polymind.Domain/Visas/VisaFlightCreationRules.cs`, `VisaDialog.razor`, `FlightDialog.razor`, `tests/Polymind.Tests/M12_VisaFlightRulesTests.cs`, `docs/testing/modules/M12-visa-flight/{06-bug-report,07-fix-report}.md`, board/checkpoint, `WORKLOG.md`.
- **Đã test:** shared suite **64 passed, 0 failed, 0 skipped**; Web build và full `Polymind.slnx` ra output riêng đều **0 warning / 0 error**.
- **Lưu ý/cảnh báo cho người sau:** Claude phải verify độc lập DB attribution + NotificationJob recipient và mới được đánh dấu Verified Fixed. Không sửa observations M12 (audit, state-machine, ActualDepartureAt, unique pair) hoặc shared auth fallback; không migration/production data write.

### [2026-07-10] Session 66 — Codex — QA bug-fix batch M09/M10/M06/M01_03
- **Làm được:** xử lý tuần tự toàn bộ Codex queue. M09 BUG_M09_01/02: unique index commission + migration preflight duplicate, retry đúng unique constraint, atomic conditional commission state; PostgreSQL race probe 12 workers pass. M10 BUG_M10_01: gom mọi runtime Payment→Paid vào service chung với tuần tự/actor/audit/commission + re-check approve ở dialog; PostgreSQL posting probe pass. M06 BUG_M06_01: JobOrder CreatedBy đúng actor + unit factory. M01 BUG_M01_03: Partner unlink rotate security stamp và xử lý IdentityResult. Cập nhật đủ `06/07`, board/checkpoint; queue Codex hết.
- **File thay đổi chính:** `Domain/{Commissions/AgentCommissionTransitions,Finance/PaymentPostingRules,JobOrders/JobOrderCreationRules}.cs`; `Infrastructure/Persistence/ApplicationDbContext.cs` + migration `EnforceAgentCommissionIdempotency`; `Web/{Commissions/CommissionEngine,Finance/PaymentPostingService}.cs`; Finance/Agent/JobOrder/Candidate callers; 3 regression test files; `docs/testing/**`; `WORKLOG.md`.
- **Đã test:** shared suite **52 passed, 0 failed, 0 skipped**; `dotnet build Polymind.slnx --no-restore` ra output riêng = **0 warning/0 error**. PostgreSQL M09 race probe: 12 workers → 1 row/1 audit; M10 posting probe: out-of-order blocked, 4 stage Paid → 3 commission, thu lẻ → 0 commission. Local commission data preflight: 0 duplicate groups/20 rows.
- **Lưu ý/cảnh báo cho người sau:** M09/M10/M06/M01_03 chờ Claude xác minh độc lập. Migration M09 cố ý fail nếu môi trường đích có duplicate commission, không tự deduplicate dữ liệu tiền. Không sửa M12 first-user attribution/shared audit fallback vì chưa vào Codex queue. Probe dùng DB tạm đã xóa; không áp migration/sửa production; chưa commit/restart dev host.

### [2026-07-10] Session 65 — Codex — QA bug-fix batch M01→M04
- **Làm được:** xử lý tuần tự toàn bộ queue: M02 BUG_M02_02/01 (API Candidate data-scope + stale permission session), M01 BUG_M01_01/02 (revoke cookie khi khóa + phản hồi login chung), M03 BUG_M03_01 (dọn Owner/Parent link), M04 BUG_M04_01 (CreatedBy=actor). Bổ sung hardening cho khóa user trong Parent/Student unlink. Tạo đủ 4 `07-fix-report.md`, cập nhật bug report/board/checkpoint; Claude đã verify code-level M02. User chốt thêm accountant được approve thu/chi/hoa hồng/vay; đã chuyển TC_M02_021 thành requirement/source pass, không cần đổi code.
- **File thay đổi chính:** `Web/Api/{ResourceEndpoints,AuthEndpoints}.cs`, `Web/Identity/IdentityRevalidatingAuthenticationStateProvider.cs`, `Web/Components/{Account/Login,Pages/Admin/AccountManagerPanel,Pages/Admin/Admin,Pages/Candidates/{ParentAccountDialog,StudentAccountDialog},Pages/Leads/LeadDetail}.razor`; helper mới ở `Domain/{Security,Leads}` + `Infrastructure/Identity/AuthenticationSecurityPolicy.cs`; 4 file regression test; `docs/testing/**` + `WORKLOG.md`.
- **Đã test:** `dotnet test tests/Polymind.Tests/Polymind.Tests.csproj --no-restore --nologo` = **29 passed, 0 failed, 0 skipped**. `dotnet build Polymind.slnx --no-restore -p:OutputPath=C:\tmp\polymind-codex-solution-build\` = **0 warning/0 error**. Build output mặc định bị dev host PID 42884 khóa DLL nên dùng output riêng, không dừng host.
- **Lưu ý/cảnh báo cho người sau:** M01/M03/M04 cần Claude runtime verification; M02 HTTP PoC pending vì `:5177` đang chạy code cũ. JWT đã cấp vẫn stateless tới expiry 240 phút. Cleanup Candidate và Identity delete vẫn qua hai DbContext như thiết kế cũ. Không migration/production write/secret; chưa commit.
