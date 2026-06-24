# AGENTS.md — POLYMIND OLMS

## ⚠️ BẮT BUỘC TRƯỚC KHI LÀM BẤT KỲ VIỆC GÌ

**Đọc [WORKLOG.md](WORKLOG.md) trước tiên.** Đó là file phối hợp chung giữa các session AI (Claude và Codex luân phiên):
- Đọc `TRẠNG THÁI HIỆN TẠI` + `VIỆC TIẾP THEO` + entry mới nhất.
- Làm theo `VIỆC TIẾP THEO`.
- **Sau khi làm xong / trước khi kết thúc session:** cập nhật `TRẠNG THÁI HIỆN TẠI`, `VIỆC TIẾP THEO`, và thêm 1 entry vào `NHẬT KÝ SESSION`.

## Tài liệu nền (đọc 1 lần để nắm)
- [docs/05-handoff-codex.md](docs/05-handoff-codex.md) — cách chạy, cấu trúc, quy ước code, **bẫy kỹ thuật**, backlog P1→P4.
- docs/01→03 — nghiệp vụ, schema DB, workflow 17 bước (vẫn đúng; stack đã đổi từ Node sang .NET).

## Tóm tắt nhanh
- .NET 10 + Blazor (Interactive Server) + MudBlazor + EF Core + PostgreSQL. Solution: `Polymind.slnx`.
- Chạy: `docker compose up -d` rồi `dotnet run --project src/Polymind.Web` → http://localhost:5177 (admin@polymind.local / Admin@123).
- Luôn để `dotnet build Polymind.slnx` xanh trước khi kết thúc session.
