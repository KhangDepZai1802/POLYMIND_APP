# Luật phân bậc tin nhắn — POLYMIND OLMS

> **Đây là tài liệu GỐC của luật "ai được nhắn ai".** Muốn đổi luật thì sửa file này trước, rồi mới sửa code.
>
> - **Chốt bởi:** user (chủ dự án) — **2026-07-13**
> - **Mã thay đổi:** CR-M14-3 (thu hẹp CR-M14-1/CR-M14-2)
> - **Code thực thi:**
>   - Ma trận bậc → [`src/Polymind.Domain/Messaging/MessagingTiers.cs`](../src/Polymind.Domain/Messaging/MessagingTiers.cs)
>   - Tầng quan hệ ứng viên → [`src/Polymind.Domain/Messaging/CandidateMessagingRelationship.cs`](../src/Polymind.Domain/Messaging/CandidateMessagingRelationship.cs) + [`Messages.razor`](../src/Polymind.Web/Components/Pages/Messages/Messages.razor)
>   - Adapter UI → [`src/Polymind.Web/Identity/MessagingPolicy.cs`](../src/Polymind.Web/Identity/MessagingPolicy.cs)
> - **Test khóa luật:** `tests/Polymind.Tests/M14_MessagingMatrixTests.cs` (ma trận) + `M14_MessagingRulesTests.cs` (quan hệ)

---

## 1. Năm bậc

| Bậc | Tên gọi | Role (kỹ thuật) | Role (tiếng Việt) |
|:---:|---|---|---|
| **1** | Quyền cao nhất | `super_admin` | Super Admin |
| **2** | Ban giám đốc | `director` | Giám đốc |
| **3** | Vận hành / văn phòng | `accountant`, `recruitment_manager`, `document_staff`, `visa_staff` | Kế toán, Trưởng phòng tuyển dụng, Bộ phận hồ sơ, Bộ phận visa |
| **4** | Tuyến đầu / đối tác | `consultant`, `recruiter`, `agent` | Tư vấn viên (TVV), Nhân viên tuyển dụng, Đại lý |
| **5** | Cổng cá nhân | `parent`, `student`, `collaborator` | Phụ huynh, Học viên, Cộng tác viên (CTV) |

---

## 2. Bốn mệnh đề

**(1) Super Admin — hai chiều, không giới hạn.**
Super Admin nhắn được tất cả, và **ai cũng nhắn được Super Admin** (kênh hỗ trợ/SOS). Super Admin luôn hiện trong danh bạ của mọi người, kể cả bậc 5.

**(2) Chênh bậc ≤ 1 thì được nhắn.**
Được: `2↔3`, `3↔4`, `4↔5`, và **cùng bậc** (`2↔2`, `3↔3`, `4↔4`, `5↔5`).
Chặn: `2↔4`, `2↔5`, `3↔5` (chênh từ 2 bậc trở lên).

**(3) Ba ngoại lệ CHẶN — đè lên mệnh đề (2).**

| Ngoại lệ | Lý do |
|---|---|
| TVV ✗ TVV | Không cho tư vấn viên trao đổi chéo với nhau |
| CTV ✗ CTV | CTV là **đối thủ** của nhau |
| **Đại lý ✗ toàn bộ bậc 4** (Đại lý, TVV, NV tuyển dụng) | Các đại lý là **đối thủ** của nhau (cùng logic đã chốt ở CR-M09-2 — ẩn doanh số giữa các đại lý); và đại lý là **đối tác NGOÀI công ty**, không trao đổi ngang hàng với nhân sự tuyến đầu |

> Đại lý vẫn nhắn được **lên** bậc 3 và **xuống** CTV của mình — hai chiều đó chênh đúng 1 bậc, không thuộc bậc 4.

**(4) Tầng quan hệ dữ liệu siết THÊM lên trên ma trận.**
Ma trận chỉ quyết định **loại role** nào được nhắn nhau. **Đúng người nào** thì do quan hệ ứng viên quyết định:

- Người nhận là **Phụ huynh/Học viên** → phải thuộc quan hệ ứng viên của người gửi (CTV giới thiệu / TVV phụ trách / người nhà cùng ứng viên).
- **Đại lý → CTV** → chỉ CTV **thuộc đại lý mình**.

> ⚠️ Hệ quả cần nhớ: `recruiter` (NV tuyển dụng, bậc 4) chênh 1 bậc với bậc 5 nên **ma trận cho phép**, nhưng **quan hệ chặn** (NV tuyển dụng không nằm trong quan hệ portal). Kết quả cuối: **NV tuyển dụng KHÔNG nhắn được học viên/phụ huynh.** Hai tầng chồng lên nhau, không thay thế nhau.

---

## 3. Tra nhanh — vai trò nào nhắn được với ai

| Vai trò | Nhắn được với |
|---|---|
| **Super Admin** (1) | **Tất cả mọi người** |
| **Giám đốc** (2) | Bậc 3 (Kế toán, TP tuyển dụng, Hồ sơ, Visa) · Giám đốc khác · Super Admin |
| **Kế toán / TP tuyển dụng / Bộ phận hồ sơ / Bộ phận visa** (3) | Giám đốc · toàn bộ bậc 3 (kể cả cùng role) · TVV · NV tuyển dụng · Đại lý · Super Admin |
| **Tư vấn viên** (4) | Bậc 3 · NV tuyển dụng · **Học viên/Phụ huynh mình phụ trách** · Super Admin<br>✗ TVV khác · ✗ Đại lý · ✗ Giám đốc |
| **NV tuyển dụng** (4) | Bậc 3 · TVV · NV tuyển dụng khác · Super Admin<br>✗ Đại lý · ✗ bậc 5 · ✗ Giám đốc |
| **Đại lý** (4) | Bậc 3 · **CTV thuộc đại lý mình** · Super Admin<br>✗ Đại lý khác · ✗ TVV · ✗ NV tuyển dụng · ✗ Giám đốc · ✗ Học viên/Phụ huynh |
| **CTV** (5) | **Đại lý chủ quản** · **Học viên/Phụ huynh của ứng viên mình giới thiệu** · Super Admin<br>✗ CTV khác · ✗ TVV · ✗ nhân sự nội bộ |
| **Học viên** (5) | CTV giới thiệu · TVV phụ trách · Phụ huynh của mình · Super Admin |
| **Phụ huynh** (5) | CTV giới thiệu · TVV phụ trách · Học viên của mình · Super Admin |

---

## 4. Ma trận đầy đủ

`✓` = ma trận cho phép · `✗` = chặn · `✓*` = ma trận cho phép nhưng **tầng quan hệ siết tiếp** (xem mệnh đề 4)

Đối xứng — đọc theo hàng hay cột đều như nhau.

| | SA | GĐ | KT | TP.TD | Hồ sơ | Visa | TVV | NV.TD | Đại lý | CTV | H.viên | P.huynh |
|---|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|
| **SA** (1) | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| **Giám đốc** (2) | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✗ | ✗ | ✗ | ✗ | ✗ | ✗ |
| **Kế toán** (3) | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✗ | ✗ | ✗ |
| **TP tuyển dụng** (3) | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✗ | ✗ | ✗ |
| **Bộ phận hồ sơ** (3) | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✗ | ✗ | ✗ |
| **Bộ phận visa** (3) | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✗ | ✗ | ✗ |
| **TVV** (4) | ✓ | ✗ | ✓ | ✓ | ✓ | ✓ | **✗** | ✓ | **✗** | ✓* | ✓* | ✓* |
| **NV tuyển dụng** (4) | ✓ | ✗ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | **✗** | ✓* | ✓* | ✓* |
| **Đại lý** (4) | ✓ | ✗ | ✓ | ✓ | ✓ | ✓ | **✗** | **✗** | **✗** | ✓* | ✓* | ✓* |
| **CTV** (5) | ✓ | ✗ | ✗ | ✗ | ✗ | ✗ | ✓* | ✓* | ✓* | **✗** | ✓* | ✓* |
| **Học viên** (5) | ✓ | ✗ | ✗ | ✗ | ✗ | ✗ | ✓* | ✓* | ✓* | ✓* | ✓ | ✓* |
| **Phụ huynh** (5) | ✓ | ✗ | ✗ | ✗ | ✗ | ✗ | ✓* | ✓* | ✓* | ✓* | ✓* | ✓ |

### Các ô `✓*` được tầng quan hệ giải quyết ra sao

| Cặp | Kết quả cuối cùng |
|---|---|
| TVV ↔ Học viên/Phụ huynh | **Được** — nhưng chỉ ứng viên mà TVV đó là `Candidate.ConsultantId` |
| NV tuyển dụng ↔ Học viên/Phụ huynh | **KHÔNG** — NV tuyển dụng không nằm trong quan hệ portal |
| Đại lý ↔ Học viên/Phụ huynh | **KHÔNG** — đại lý không nằm trong quan hệ portal |
| Đại lý ↔ CTV | **Được** — chỉ CTV thuộc đại lý đó (`Collaborator.AgentId`) |
| CTV ↔ Học viên/Phụ huynh | **Được** — chỉ ứng viên mà CTV đó là `Candidate.CollaboratorId` |
| Học viên ↔ Phụ huynh | **Được** — chỉ trong cùng một ứng viên |

---

## 5. Cách cài đặt (fail-closed)

Người gửi **bậc 5** (Phụ huynh/Học viên/CTV) → **danh sách ĐÓNG**: chỉ nhắn được đúng những người trong tập quan hệ (+ Super Admin). Ngoài tập là chặn.

Người gửi **bậc 1–4** → qua 3 cửa, trượt bất kỳ cửa nào là chặn:
1. `MessagingTiers.CanMessage` — ma trận bậc.
2. Người nhận là portal (Phụ huynh/Học viên) → phải thuộc quan hệ ứng viên.
3. Người gửi là Đại lý và người nhận là CTV → phải là CTV thuộc đại lý mình.

Cùng một luật được áp ở **cả hai nơi**: dựng danh bạ (`LoadContacts`) **và** re-check server khi bấm Gửi (`Send`) — dùng chung hàm `IsAllowedRecipient`. Ẩn UI thôi là không đủ.

> **Nguồn gốc bug "nhắn loạn xạ" (trước CR-M14-3):** `MessagingPolicy.CanMessage` kết thúc bằng `return true` — mặc định MỞ cho mọi cặp nhân sự nội bộ không khớp luật nào ở trên. Nay đã thay bằng ma trận fail-closed: **không khớp = chặn**.

---

## 6. Lịch sử quyết định

| Ngày | Mã | Nội dung |
|---|---|---|
| 2026-07-11 | CR-M14-1 | Giới hạn nhắn tin theo quan hệ ứng viên (staff/CTV/đại lý ↔ portal) |
| 2026-07-13 | CR-M14-2 | Thu hẹp bậc 5: Học viên chỉ nhắn CTV + TVV + Phụ huynh; Phụ huynh chỉ nhắn CTV + TVV + Học viên. Đại lý và nhân sự hồ sơ/visa/workflow bị loại khỏi quan hệ portal |
| 2026-07-13 | — | CTV chỉ nhắn đại lý chủ quản + học viên/phụ huynh mình giới thiệu |
| 2026-07-13 | **CR-M14-3** | **Mô hình 5 bậc cho toàn hệ thống.** Thay fallback-mở bằng ma trận fail-closed. Xếp `recruiter` vào bậc 4. Cô lập Đại lý khỏi bậc 4. Super Admin hai chiều với mọi người + bộ lọc vai trò ở danh bạ |
