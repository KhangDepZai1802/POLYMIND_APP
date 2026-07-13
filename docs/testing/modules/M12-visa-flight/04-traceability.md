# M12 — Visa & Flight / Exit · Traceability

| Business Flow | Page | API | Role | State | Test Case IDs | Automated | Coverage | Gap |
|---|---|---|---|---|---|---|---|---|
| BF-M12-01 Tạo visa | /visa VisaDialog | — | VisaStaff/super | none→visa | TC_M12_001,002,005,006 | TC_M12_027,028 | Code + partial | **BUG_M12_01** (HandledBy); runtime (Blocked) |
| BF-M12-02 Sửa visa | VisaDialog | — | VisaStaff/super | update | TC_M12_003,004,022 | TC_M12_026 | Code | Runtime (Blocked); no state-machine (Obs) |
| BF-M12-03 Tạo vé | FlightDialog | — | VisaStaff/super | none→flight | TC_M12_007,008,011 | TC_M12_029,030 | Code + partial | **BUG_M12_02** (AssignedTo); runtime (Blocked) |
| BF-M12-04 Sửa vé | FlightDialog | — | VisaStaff/super | update | TC_M12_009,010 | — | Code | ActualDepartureAt no UI (Obs) |
| BF-M12-05 Reminder visa/departure | NotificationService | — | (job) | — | TC_M12_019,020,021 | — | Code | **BUG_M12_01** misroute visa reminder; runtime (Blocked) |
| BF-M12-06 Report actual departure | CsvExportEndpoints | REST export | reports roles | — | TC_M12_010 | — | Code | ActualDepartureAt luôn rỗng (Obs) |
| AuthZ (page + create + edit) | Visas.razor | — | mọi role | — | TC_M12_012..018 | — | Code | Runtime role matrix (Blocked) |
| Enum/entity contract | — | — | — | — | TC_M12_026..030 | **5 unit** | **Automated** | — |

## Gap Analysis

- **Đã phủ ở source (Pass code):** page authorize, create AuthorizeView + Save re-check permission (visa+flight), CJO auto-fill + khóa khi edit, RejectionReason chỉ khi Rejected, departure reminder recipient đúng (owners), Approved/Rejected không nhắc, không role scoped truy cập → không IDOR.
- **Bug (Application Defect) → Codex:** BUG_M12_01 (VisaDialog HandledBy first-user → **misroute visa reminder**, Medium), BUG_M12_02 (FlightDialog AssignedTo first-user, cosmetic, Low).
- **Automated ngay (5 unit):** VisaStatus contract + entity default/nullable (HandledBy/AssignedTo/ActualDepartureAt) — bảo vệ hợp đồng dữ liệu cho notification/report.
- **Blocked (harness):** mọi flow runtime tạo/sửa visa/flight qua DB + UI; kiểm `handled_by`/`assigned_to` thực tế; reminder routing thực tế.
- **Observations:** OBS-M12-01 (no audit visa/flight), OBS-M12-02 (no VisaStatus state-machine + no rowversion), OBS-M12-03 (không set được ActualDepartureAt runtime → report rỗng — req U-M12-1), OBS-M12-04 (no unique (candidate,job) → trùng).
- **Không tuyên bố 100%:** runtime DB/UI + notification routing chưa đo tự động.
