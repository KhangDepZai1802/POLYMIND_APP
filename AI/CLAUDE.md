# CLAUDE.md — POLYMIND OLMS

## ⚠️ BẮT BUỘC TRƯỚC KHI LÀM BẤT KỲ VIỆC GÌ

**Đọc [WORKLOG.md](WORKLOG.md) trước tiên.** File phối hợp chung giữa các session AI (Claude và Codex luân phiên):
- Đọc `TRẠNG THÁI HIỆN TẠI` + `VIỆC TIẾP THEO` + entry mới nhất.
- Làm theo `VIỆC TIẾP THEO`.
- **Trước khi kết thúc session:** cập nhật `TRẠNG THÁI HIỆN TẠI`, `VIỆC TIẾP THEO`, thêm 1 entry vào `NHẬT KÝ SESSION`.

## Tài liệu nền
- [docs/05-handoff-codex.md](docs/05-handoff-codex.md) — cách chạy, cấu trúc, quy ước, bẫy kỹ thuật, backlog P1→P4.

## Tóm tắt
- .NET 10 + Blazor (Interactive Server) + MudBlazor + EF Core + PostgreSQL. Solution: `Polymind.slnx`.
- Chạy: `docker compose up -d` → `dotnet run --project src/Polymind.Web` → http://localhost:5177 (admin@polymind.local / Admin@123).
- Quy ước & bẫy quan trọng: trang app = `@rendermode InteractiveServer` + `[Authorize]` + `IDbContextFactory`; trang set cookie để SSR; **DateTimeOffset xuống Postgres phải UTC offset 0**.
- Luôn để build xanh trước khi kết thúc.
