# M16 — Reports & Export · Test Cases

> `TC_M16_<NNN>`. Phần lớn source-verified (endpoint + DbSeeder); runtime cần DB + đăng nhập theo role.

## Authorization
| TC | Name | Role | Expected | Kết quả |
|---|---|---|---|---|
| TC_M16_001 | Agent/CTV không xuất được (High) | agent/collaborator | `/export/commissions.csv` → 403 (không có `reports:read`) | **Pass** (DbSeeder: agent/CTV không có reports:read) |
| TC_M16_002 | Parent/Student không tải receipt PDF (High) | parent/student | `/receipts/{id}.pdf` → 403 | **Pass** (không có receipts:read) |
| TC_M16_003 | Recruiter không vào Reports (Med) | recruiter | `/reports` → access denied | **Pass** (recruiter không có reports:read) |
| TC_M16_004 | Accountant xuất + in phiếu (Med) | accountant | Có reports:read + receipts:read → OK | **Pass** |
| TC_M16_005 | Chưa đăng nhập (High) | anon | `/export/*`, `/receipts/*` → redirect/401 | **Pass** (RequireAuthorization) |
| TC_M16_006 | RM chỉ báo cáo tuyển dụng | recruitment_manager | lead/funnel export được; finance/commission/revenue/top-agent → 403 | **Pass (Codex access regression) — runtime pending** |

## Functional / Export
| TC | Name | Steps | Expected | Kết quả |
|---|---|---|---|---|
| TC_M16_010 | Export CSV có BOM UTF-8 (Low) | tải finance-monthly.csv | Mở Excel không lỗi dấu tiếng Việt | Source-verified (GetPreamble) |
| TC_M16_011 | Export Excel/PDF sinh đúng tiêu đề/cột (Low) | tải .xlsx/.pdf | Có title + header + rows | Source-verified |
| TC_M16_012 | Export theo range đang chọn (Med) | Chọn "Tháng này" → export finance-monthly | Link có `from/to`; file chỉ tháng này | **Pass (Codex range regression/source) — runtime file pending** |
| TC_M16_013 | CSV injection escape (Med) | dữ liệu có `,"` newline | Bọc ngoặc kép, escape đúng | Source-verified (EscapeCsv) — lưu ý §note |
| TC_M16_014 | Funnel đếm đúng mốc (Low) | recruitment-funnel | selected/visa/departed theo step + Visa/Flight | Source-verified |

## Receipt PDF / IDOR
| TC | Name | Steps | Expected | Kết quả |
|---|---|---|---|---|
| TC_M16_020 | Receipt PDF không kiểm ownership (Med) | accountant tải `/receipts/{anyId}.pdf` | Trả PDF phiếu bất kỳ | **Đúng hành vi hiện tại** (finance được xem mọi phiếu) → OBS-M16-01 latent |
| TC_M16_021 | (giả định) nếu self-scoped có receipts:read | tải phiếu người khác | **Sẽ là IDOR** | Không tái hiện được (seed không cấp) — defense-in-depth |

## Boundary
| TC | Name | Expected | Kết quả |
|---|---|---|---|
| TC_M16_030 | Receipt id không tồn tại | 404 | Source-verified (NotFound) |
| TC_M16_031 | Báo cáo rỗng (0 dữ liệu) | File hợp lệ, 0 dòng | Source-verified |
| TC_M16_032 | Range đảo `from > to` | 400 Bad Request, không query DB | Pass (unit/source) |

> **Note (TC_M16_013):** `EscapeCsv` bọc field chứa `,"`/newline nhưng **không** chặn công thức Excel (`=`,`+`,`-`,`@` đầu ô) → CSV formula injection lý thuyết. Dữ liệu là tên ứng viên/đại lý (ít khả năng), nhưng ghi OBS-M16-05.
