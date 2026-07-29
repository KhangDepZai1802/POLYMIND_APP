# PROMPT DÀNH CHO CLAUDE CODE
## ĐỌC TOÀN BỘ SOURCE CODE, TẠO MỤC LỤC RÀNG BUỘC VÀ VIẾT HOÀN CHỈNH BÁO CÁO THỰC TẬP

Bạn phải thực hiện toàn bộ công việc từ đầu đến cuối trong một quy trình liên tục.

Không chỉ lập mục lục.

Sau khi tạo mục lục, phải lập tức dựa vào mục lục đó để viết hoàn chỉnh báo cáo thực tập tốt nghiệp.

**Nguyên tắc vận hành cốt lõi:** đây là một quy trình liên tục từ đầu đến cuối, nhưng file DOCX phải được tạo sớm, cập nhật và lưu bền vững theo từng phần lớn. Tuyệt đối không được giữ toàn bộ nội dung gần 100 trang trong context rồi chỉ ghi DOCX một lần ở cuối.

Chỉ một agent/tiến trình chính được quyền ghi vào file DOCX. Nếu có sử dụng subagent hoặc workflow để khảo sát source code, các agent phụ chỉ được đọc, phân tích và trả về bằng chứng; không được đồng thời chỉnh sửa cùng một file DOCX.

---

# 1. KẾT QUẢ BẮT BUỘC

Chỉ được tạo đúng hai file sau:

```text
Thực tập/00_MUC_LUC_RANG_BUOC_BAO_CAO.md
Thực tập/BAO_CAO_THUC_TAP_TOT_NGHIEP.docx
```

Không tạo:

- File báo cáo tiến độ.
- File checkpoint.
- File worklog.
- File báo cáo Markdown.
- File ghi chú riêng.
- File phân tích riêng.
- File danh sách thông tin cần bổ sung riêng.
- Bất kỳ file đầu ra lâu dài nào khác.

Được phép tạo file tạm phục vụ an toàn dữ liệu, kiểm tra và khả năng tiếp tục, nhưng chỉ bên trong:

```text
Thực tập/.tmp_bao_cao/
```

File tạm có thể gồm:

- Bản DOCX đang làm việc.
- Bản DOCX hợp lệ gần nhất.
- File DOCX mới chờ kiểm tra trước khi thay thế.
- Script tạo/chỉnh DOCX.
- PDF và ảnh render dùng để kiểm tra bố cục.
- File dữ liệu phân tích tạm.

Không được coi các file trên là đầu ra cuối cùng. Chỉ được xóa thư mục tạm sau khi file DOCX chính thức đã được mở lại, kiểm tra cấu trúc, kiểm tra bố cục và xác nhận hoàn thành. Khi kết thúc thành công, thư mục `Thực tập/` cuối cùng chỉ còn đúng hai file bắt buộc nêu trên.

---

# 2. PHẠM VI THAO TÁC

## 2.1. Source code chỉ được đọc

Bạn được phép:

- Đọc toàn bộ source code.
- Tìm kiếm trong source code.
- Phân tích kiến trúc.
- Phân tích nghiệp vụ.
- Đọc database schema và migration.
- Đọc API, route, controller, service, component và page.
- Đọc authentication, authorization, role và permission.
- Đọc file cấu hình.
- Đọc test case, tài liệu QA và tài liệu triển khai.
- Đọc lịch sử Git nếu cần để hiểu dự án.
- Chạy các lệnh chỉ đọc và không làm thay đổi repository.

Bạn tuyệt đối không được:

- Sửa source code.
- Format code.
- Refactor code.
- Sửa bug.
- Thêm, xóa hoặc thay đổi chức năng.
- Thay đổi database.
- Chạy migration hoặc seed làm thay đổi dữ liệu.
- Thêm, xóa hoặc nâng cấp dependency.
- Làm thay đổi lockfile.
- Thay đổi cấu hình build hoặc deploy.
- Xóa hoặc đổi tên file dự án.
- Commit hoặc push Git.
- Làm lộ secret, password, API key, token hoặc connection string.

## 2.2. Chỉ được tạo hoặc cập nhật file trong thư mục `Thực tập/`

Không được tạo, sửa, xóa hoặc đổi tên bất kỳ file nào nằm ngoài thư mục:

```text
Thực tập/
```

Nếu thư mục chưa tồn tại, được phép tạo thư mục này.

Mọi script hoặc artifact tạm phục vụ việc tạo DOCX cũng phải nằm trong `Thực tập/.tmp_bao_cao/`. Không được cài dependency mới vào repository; chỉ được sử dụng công cụ đã có sẵn trong môi trường. Nếu cần chạy Python, LibreOffice, Pandoc hoặc công cụ tương đương, phải bảo đảm lệnh không làm thay đổi source code, dependency hoặc lockfile của dự án.

---

# 3. FILE MẪU BẮT BUỘC

Phải tìm và đọc toàn bộ file:

```text
TTTN_Mau9_SINHVIENtrinhbaybaocaothuctaptotnghiep.docx
```

Tên file thực tế có thể có thêm hậu tố, ví dụ:

```text
TTTN_Mau9_SINHVIENtrinhbaybaocaothuctaptotnghiep(1).docx
```

File này là mẫu chính thức bắt buộc.

Toàn bộ cấu trúc và định dạng báo cáo phải dựa trên mẫu này.

Phải ưu tiên **sao chép trực tiếp file mẫu thành bản DOCX làm việc** rồi điền nội dung vào cấu trúc đó để bảo toàn section, style, header, footer, lề, bảng biểu mẫu, trường Word và thiết lập đánh số trang. Không được tạo một tài liệu trắng rồi tự mô phỏng lại mẫu nếu file mẫu có thể chỉnh sửa được.

Chỉ khi cấu trúc file mẫu thực sự không thể sử dụng mới được tạo tài liệu mới. Khi đó phải tái tạo đầy đủ định dạng theo mẫu và ghi rõ giới hạn này trong phản hồi cuối cùng; tuyệt đối không âm thầm thay bằng mẫu khác.

Không được thay bằng mẫu luận văn, khóa luận hoặc mẫu báo cáo khác.

---

# 4. QUY TRÌNH BẮT BUỘC

Thực hiện đúng thứ tự sau:

## BƯỚC 1 — Đọc toàn bộ mẫu báo cáo

Phải đọc toàn bộ file mẫu để xác định:

- Cấu trúc trang bìa.
- Trang lót bìa nếu có.
- Nhận xét của chuyên gia doanh nghiệp.
- Nhận xét của giảng viên hướng dẫn.
- Mục lục.
- Lời mở đầu.
- Chương 1.
- Chương 2.
- Chương 3.
- Chương 4.
- Tài liệu tham khảo.
- Phụ lục.
- Bảng ghi nhận kết quả thực tập hàng tuần.
- Bảng đánh giá quá trình thực tập.
- Phiếu đánh giá kết quả thực tập.
- Cách đánh số trang.
- Cách đánh số hình và bảng.
- Cấp tiêu đề.
- Font, cỡ chữ, căn lề, giãn dòng và khổ giấy.

## BƯỚC 2 — Đọc toàn bộ source code và tài liệu dự án

Phải đọc kỹ toàn bộ phần liên quan của repository, tối thiểu gồm:

- README.
- Tài liệu Markdown.
- Tài liệu nghiệp vụ.
- Tài liệu kiến trúc.
- Tài liệu phân quyền.
- Frontend.
- Backend.
- Database schema.
- Migration.
- API.
- Route.
- Page.
- Layout.
- Component.
- Controller.
- Service.
- Repository hoặc data-access layer.
- Middleware.
- Authentication.
- Authorization.
- Role.
- Permission.
- Validation.
- Xử lý lỗi.
- Upload và storage.
- Báo cáo và xuất file.
- Docker.
- Cấu hình triển khai.
- CI/CD nếu có.
- Test tự động.
- Test case.
- Báo cáo QA.
- Nhật ký hoặc tài liệu phát triển nếu có.

Có thể bỏ qua nội dung thư viện và file sinh tự động như:

- `.git`
- `node_modules`
- `bin`
- `obj`
- `dist`
- `build`
- `.next`
- `coverage`
- cache
- file nhị phân
- output build

Tuy nhiên vẫn phải đọc các file dependency và cấu hình quan trọng.

Không được chỉ đọc README rồi suy đoán toàn bộ dự án.

## BƯỚC 3 — Tạo mục lục ràng buộc

Sau khi đọc xong toàn bộ mẫu và source code, tạo:

```text
Thực tập/00_MUC_LUC_RANG_BUOC_BAO_CAO.md
```

## BƯỚC 4 — Viết ngay báo cáo DOCX hoàn chỉnh

Sau khi tự kiểm tra file mục lục, phải dùng chính mục lục đó làm ràng buộc để viết từ đầu đến cuối:

```text
Thực tập/BAO_CAO_THUC_TAP_TOT_NGHIEP.docx
```

Không được dừng sau khi tạo mục lục.

Không cần báo cáo tiến độ.

Không cần chờ xác nhận lại.

Được phép sử dụng file trung gian **chỉ trong** `Thực tập/.tmp_bao_cao/` để bảo đảm DOCX không mất dữ liệu hoặc hỏng khi phiên bị gián đoạn. Không được tạo thêm tài liệu đầu ra lâu dài.

## BƯỚC 4.1 — Khởi tạo bản DOCX làm việc an toàn

Phải thực hiện theo thứ tự:

1. Tạo `Thực tập/.tmp_bao_cao/` nếu chưa tồn tại.
2. Sao chép file mẫu chính thức thành:

   ```text
   Thực tập/.tmp_bao_cao/BAO_CAO_WORKING.docx
   ```

3. Không chỉnh trực tiếp file mẫu gốc.
4. Không tạo ngay file chính thức bằng một tài liệu trắng.
5. Ngay sau khi khởi tạo, phải mở lại bản làm việc bằng thư viện/công cụ xử lý DOCX để xác nhận file đọc được.

## BƯỚC 4.2 — Viết và lưu tăng dần

Phải viết theo thứ tự của `00_MUC_LUC_RANG_BUOC_BAO_CAO.md` và lưu bền vững sau từng khối lớn:

1. Phần đầu báo cáo.
2. Lời mở đầu.
3. Chương 1.
4. Từng nhóm mục cấp `2.x` của Chương 2; không chờ viết xong toàn bộ Chương 2 mới lưu.
5. Chương 3.
6. Chương 4.
7. Tài liệu tham khảo.
8. Phụ lục và các biểu mẫu bắt buộc.

Sau mỗi khối:

- Ghi nội dung vào một file mới chờ kiểm tra, ví dụ `BAO_CAO_NEXT.docx`.
- Không ghi đè trực tiếp lên bản hợp lệ gần nhất.
- Kiểm tra file mới không rỗng và có dung lượng hợp lý.
- Kiểm tra gói DOCX/ZIP không lỗi.
- Mở lại file mới bằng công cụ xử lý DOCX.
- Xác nhận heading và nội dung vừa viết thực sự tồn tại.
- Chỉ khi kiểm tra thành công mới dùng thao tác thay thế file an toàn/atomic để cập nhật `BAO_CAO_WORKING.docx`.
- Giữ lại một bản `BAO_CAO_LAST_GOOD.docx` cho đến khi lần lưu tiếp theo đã được xác minh.

Tuyệt đối không được giữ nội dung nhiều chương chỉ trong context mà chưa ghi xuống file.

## BƯỚC 4.3 — Khả năng tiếp tục khi phiên bị gián đoạn

Nếu `Thực tập/.tmp_bao_cao/BAO_CAO_WORKING.docx` đã tồn tại:

- Không được tự động bắt đầu lại từ đầu.
- Không được ghi đè bằng tài liệu trắng.
- Phải mở và kiểm tra file hiện có.
- Phải đọc lại `00_MUC_LUC_RANG_BUOC_BAO_CAO.md`.
- Phải xác định heading/mục cuối cùng đã hoàn thành trong DOCX.
- Phải tiếp tục từ mục chưa hoàn thành tiếp theo.
- Phải kiểm tra để tránh lặp nội dung hoặc bỏ sót mục.
- Nếu bản working bị lỗi, phải khôi phục từ `BAO_CAO_LAST_GOOD.docx` và tiếp tục.

Nếu context bị compact hoặc phiên được resume, phải đọc lại mục lục ràng buộc và các phần liên quan trong DOCX trước khi viết tiếp; không được dựa vào trí nhớ tóm tắt để suy đoán nội dung đã làm.

## BƯỚC 4.4 — Không viết DOCX theo cách dễ mất định dạng

Không được dùng quy trình chính kiểu:

1. Viết toàn bộ báo cáo thành Markdown.
2. Chờ đến cuối.
3. Chuyển một lần sang DOCX.

Có thể dùng dữ liệu tạm có cấu trúc để hỗ trợ, nhưng bản DOCX làm việc phải tồn tại sớm và được cập nhật tăng dần trên nền file mẫu.

Không được nối/ghi nhiều tiến trình đồng thời vào cùng một DOCX. Chỉ agent chính được quyền ghi file.

## BƯỚC 4.5 — Hoàn thiện file chính thức

Chỉ khi toàn bộ nội dung đã hoàn thành và vượt qua kiểm tra cuối cùng mới được tạo/thay thế an toàn thành:

```text
Thực tập/BAO_CAO_THUC_TAP_TOT_NGHIEP.docx
```

Không được xóa bản working, bản last-good, PDF hoặc ảnh kiểm tra trước khi file chính thức đã được xác minh thành công. Sau khi xác minh xong mới xóa toàn bộ `Thực tập/.tmp_bao_cao/`.

---

# 5. YÊU CẦU ĐỐI VỚI FILE MỤC LỤC RÀNG BUỘC

File:

```text
Thực tập/00_MUC_LUC_RANG_BUOC_BAO_CAO.md
```

phải là kế hoạch bắt buộc để viết báo cáo.

## 5.1. Chỉ tạo mục lục và kế hoạch nội dung

Trong file này:

- Không viết nội dung hoàn chỉnh của báo cáo.
- Chỉ tạo tên phần, chương, mục và tiểu mục.
- Chia chi tiết tối thiểu đến cấp `1.1.1`.
- Chỉ dùng cấp `1.1.1.1` khi thật sự cần.
- Không vượt quá bốn cấp.
- Mỗi mục phải có số trang dự kiến.
- Mỗi mục phải có khoảng trang bắt đầu và kết thúc.
- Mỗi mục phải có nội dung bắt buộc cần viết.
- Mỗi mục phải có source code hoặc tài liệu cần đối chiếu.
- Mỗi mục phải có ghi chú hình, bảng hoặc sơ đồ nếu cần.
- Mỗi mục phải ghi thông tin cần sinh viên bổ sung nếu source không có.

## 5.2. Tổng dung lượng

Báo cáo dự kiến và bản dàn trang thực tế phải nằm trong khoảng:

```text
97–103 trang
```

Ưu tiên gần 100 trang.

Mặc định, đây là **tổng số trang vật lý của toàn bộ file DOCX**, bao gồm phần đầu, nội dung chính, tài liệu tham khảo, phụ lục và các biểu mẫu. Nếu file mẫu hoặc quy định trong mẫu xác định cách tính khác, phải tuân theo mẫu và ghi rõ đồng thời:

- Tổng số trang vật lý toàn file.
- Số trang được đánh số Ả Rập từ Lời mở đầu.
- Số trang phần nội dung chính.

Không được đạt gần 100 trang bằng cách dồn bất hợp lý vào phụ lục hoặc biểu mẫu.

Phải:

- Tính số trang cho từng mục.
- Tính tổng từng chương.
- Tính tổng toàn báo cáo.
- Đảm bảo khoảng trang liên tục.
- Không trùng trang.
- Không bỏ số trang.
- Không tạo mục rỗng để kéo dài báo cáo.
- Không dồn trang bất hợp lý vào phụ lục.
- Chương 2 phải là chương dài nhất và chi tiết nhất.
- Số trang trong mục lục chỉ là ước lượng; phải được đối chiếu lại bằng kết quả render/dàn trang thực tế.
- Với vị trí chưa có ảnh thật, phải tạo khung placeholder có kích thước gần với ảnh dự kiến để số trang không thay đổi quá lớn khi sinh viên thay ảnh sau này.

## 5.3. Cấu trúc lớn phải bám mẫu trường

Tối thiểu gồm:

1. Trang bìa.
2. Trang lót bìa nếu mẫu yêu cầu.
3. Nhận xét của chuyên gia doanh nghiệp.
4. Nhận xét của giảng viên hướng dẫn.
5. Mục lục.
6. Danh mục chữ viết tắt nếu cần.
7. Danh mục bảng nếu có.
8. Danh mục hình nếu có.
9. Lời mở đầu.
10. Chương 1. Giới thiệu.
11. Chương 2. Nội dung chính của quá trình thực tập và dự án.
12. Chương 3. Kết quả thực tập.
13. Chương 4. Kết luận và kiến nghị.
14. Tài liệu tham khảo.
15. Phụ lục.
16. Các biểu mẫu bắt buộc theo mẫu trường.

Tên Chương 2 phải được đặt dựa trên nội dung thật của dự án.

Không được để:

```text
CHƯƠNG 2. TÊN CHƯƠNG ?
```

## 5.4. Chia task theo từng mục

Mỗi mục phải có task tương ứng.

Ví dụ:

```text
TASK 1.1 — Viết mục 1.1
TASK 1.1.1 — Viết mục 1.1.1
TASK 2.3 — Viết mục 2.3
TASK 2.3.1 — Viết mục 2.3.1
```

Mỗi task phải ghi:

- Mã task.
- Tên mục.
- Số trang dự kiến.
- Khoảng trang dự kiến.
- Nội dung bắt buộc phải viết.
- Source code hoặc tài liệu cần đọc lại.
- Hình, bảng hoặc sơ đồ cần chèn.
- Thông tin còn thiếu.

Ví dụ:

```md
## TASK 2.3.1 — 2.3.1. TÊN TIỂU MỤC

- Số trang dự kiến: 3 trang
- Khoảng trang dự kiến: 30–32
- Nội dung bắt buộc:
  - ...
  - ...
- Source code/tài liệu cần đối chiếu:
  - `đường/dẫn/file`
  - `đường/dẫn/thư/mục`
- Hình, bảng hoặc sơ đồ cần chèn:
  - ...
- Thông tin cần sinh viên bổ sung:
  - ...
```

Các task phải đủ nhỏ để viết kỹ từng phần, nhưng không được chia vụn vô lý.

## 5.5. Bảng tổng hợp số trang

Cuối file phải có:

```md
| Phần | Số trang dự kiến |
|---|---:|
| Phần đầu | ... |
| Lời mở đầu | ... |
| Chương 1 | ... |
| Chương 2 | ... |
| Chương 3 | ... |
| Chương 4 | ... |
| Tài liệu tham khảo | ... |
| Phụ lục | ... |
| **TỔNG CỘNG** | **...** |
```

Tổng cộng phải nằm trong khoảng 97–103 trang.

---

# 6. YÊU CẦU ĐỐI VỚI BÁO CÁO DOCX

Sau khi tạo xong mục lục, phải viết hoàn chỉnh:

```text
Thực tập/BAO_CAO_THUC_TAP_TOT_NGHIEP.docx
```

Báo cáo phải được viết từ đầu đến cuối theo đúng:

1. Mẫu `TTTN_Mau9_SINHVIENtrinhbaybaocaothuctaptotnghiep.docx`.
2. File `00_MUC_LUC_RANG_BUOC_BAO_CAO.md`.
3. Toàn bộ bằng chứng trong source code và tài liệu dự án.

## 6.1. Không được làm qua loa

Đây là báo cáo thực tập rất quan trọng.

Phải làm thật kỹ từng chương.

Không được:

- Chỉ liệt kê công nghệ.
- Chỉ liệt kê module.
- Sao chép README.
- Chỉ kể lại giao diện.
- Chỉ mô tả code.
- Sao chép nhiều đoạn code dài.
- Viết câu sáo rỗng.
- Lặp lại nội dung giữa các mục.
- Thêm nội dung không có bằng chứng.
- Viết vài đoạn ngắn rồi coi như hoàn thành.
- Kéo dài báo cáo bằng nội dung vô nghĩa.

Mỗi nội dung kỹ thuật quan trọng cần thể hiện:

- Bài toán.
- Yêu cầu.
- Giải pháp.
- Quy trình xử lý.
- Thành phần source code liên quan.
- Dữ liệu liên quan.
- Phân quyền liên quan.
- Vai trò hoặc công việc của sinh viên.
- Kết quả đạt được.
- Bằng chứng minh họa.

## 6.2. Chỉ viết nội dung có bằng chứng

Mọi nội dung phải dựa trên:

- Source code thật.
- Tài liệu thật.
- Cấu hình thật.
- Database thật.
- Giao diện thật.
- Test case thật.
- Kết quả kiểm thử thật.
- Thông tin do sinh viên cung cấp.

Không được bịa:

- Chức năng.
- Module.
- Công nghệ.
- Quy trình.
- Kết quả.
- Số liệu.
- Hiệu năng.
- Thành tích.
- Thông tin doanh nghiệp.
- Nhiệm vụ thực tập.
- Nhật ký từng tuần.
- Nhận xét.
- Điểm đánh giá.
- Chữ ký.

Nếu thiếu dữ liệu, chèn trực tiếp vào đúng vị trí trong báo cáo:

```text
[CẦN SINH VIÊN BỔ SUNG: ghi rõ thông tin cần cung cấp]
```

Không dừng toàn bộ công việc vì thiếu một vài thông tin.

Tiếp tục hoàn thiện các phần có đủ bằng chứng.


## 6.3. Tính nhất quán và truy xuất bằng chứng

Trong toàn bộ báo cáo phải dùng thống nhất:

- Tên dự án.
- Tên hệ thống.
- Tên module.
- Tên vai trò.
- Thuật ngữ nghiệp vụ.
- Tên công nghệ và phiên bản.
- Tên bảng dữ liệu, API, route, class và component.

Không được gọi cùng một đối tượng bằng nhiều tên khác nhau nếu source không làm như vậy.

Trước khi viết mỗi mục kỹ thuật, phải đọc lại đúng source/tài liệu đã liệt kê trong task tương ứng. Không được dựa vào nội dung đã nhớ từ nhiều giờ trước nếu có thể kiểm tra lại file thật.

Mỗi kết luận quan trọng về kiến trúc, nghiệp vụ, phân quyền, database, kiểm thử hoặc triển khai phải có ít nhất một đường dẫn source/tài liệu xác nhận trong file mục lục ràng buộc. Nếu bằng chứng mâu thuẫn, phải mô tả đúng sự mâu thuẫn hoặc dùng placeholder; không tự chọn một phiên bản rồi trình bày như sự thật.

## 6.4. Chống lặp và chống kéo dài giả tạo

Sau khi hoàn thành mỗi chương, phải kiểm tra nội dung trùng lặp với các chương trước. Không được lặp lại nguyên đoạn để tăng số trang.

Được phép nhắc lại ngắn gọn thông tin cần thiết để tạo mạch văn, nhưng phải bổ sung góc nhìn mới phù hợp với mục đang viết, ví dụ:

- Chương 1 nói về bối cảnh và đơn vị.
- Chương 2 nói về giải pháp kỹ thuật và quá trình thực hiện.
- Chương 3 nói về kết quả, kỹ năng, hạn chế và bằng chứng kiểm thử.
- Chương 4 tổng kết và kiến nghị.

Không được tăng số trang bằng khoảng trắng, page break vô lý, font lớn hơn mẫu, giãn dòng bất thường, bảng rỗng, ảnh giả hoặc nội dung lý thuyết không liên quan.

---

# 7. YÊU CẦU CHO TỪNG CHƯƠNG

## 7.1. Lời mở đầu

Phải trình bày phù hợp:

- Bối cảnh thực tập.
- Lý do tham gia dự án.
- Mục tiêu.
- Phạm vi.
- Phương pháp thực hiện.
- Cấu trúc báo cáo.

## 7.2. Chương 1 — Giới thiệu

Phải xem xét và trình bày:

- Tên và thông tin công ty.
- Địa chỉ và thông tin liên hệ.
- Cơ sở vật chất nếu có thông tin.
- Cơ cấu tổ chức.
- Lĩnh vực hoạt động.
- Sản phẩm hoặc dịch vụ.
- Đối tác nếu có thông tin.
- Quy trình công việc liên quan.
- Phòng ban thực tập.
- Người hướng dẫn doanh nghiệp.
- Nội dung cần học hỏi.
- Nhiệm vụ thực tập.
- Mục tiêu.
- Phạm vi công việc.
- Kết luận chương.

Không có thông tin thì dùng placeholder, không được bịa.

## 7.3. Chương 2 — Nội dung chính

Chương 2 phải:

- Là chương dài nhất.
- Được viết kỹ nhất.
- Có nhiều bằng chứng kỹ thuật nhất.
- Phản ánh rõ công việc thực tế.
- Không biến thành chương lý thuyết thuần túy.

Tùy source code thực tế, phải xem xét:

- Bối cảnh và bài toán.
- Phân tích yêu cầu.
- Yêu cầu chức năng.
- Yêu cầu phi chức năng.
- Vai trò người dùng.
- Quy trình nghiệp vụ.
- Kiến trúc tổng thể.
- Kiến trúc frontend.
- Kiến trúc backend.
- Công nghệ sử dụng.
- Thiết kế database.
- Authentication.
- Authorization.
- Role và permission.
- Validation.
- Xử lý lỗi.
- Bảo mật.
- Các module nghiệp vụ.
- API.
- Upload và storage.
- Báo cáo và xuất file.
- Realtime nếu có.
- Kiểm thử.
- Triển khai.
- Khó khăn kỹ thuật.
- Giải pháp xử lý.
- Kết luận chương.

Chỉ giữ các mục có bằng chứng thực tế.

Với mỗi module quan trọng, phải xem xét:

- Mục tiêu module.
- Đối tượng sử dụng.
- Quy trình nghiệp vụ.
- Giao diện.
- Route.
- API.
- Xử lý backend.
- Database.
- Phân quyền.
- Validation.
- Xử lý lỗi.
- Kết quả kiểm thử.
- Kết quả đạt được.

## 7.4. Chương 3 — Kết quả thực tập

Phải dựa trên kết quả thật:

- Hạng mục đã hoàn thành.
- Chức năng đã thực hiện.
- Kết quả kiểm thử.
- Kết quả triển khai nếu có.
- Mức độ hoàn thành nhiệm vụ.
- Kiến thức chuyên môn đạt được.
- Kỹ năng nghề nghiệp đạt được.
- Khó khăn.
- Cách xử lý.
- Hạn chế.
- Bảng ghi nhận kết quả thực tập hàng tuần.
- Bảng đánh giá quá trình thực tập.
- Phiếu đánh giá kết quả thực tập.
- Kết luận chương.

Không được tự tạo nhận xét, chữ ký, điểm số hoặc kết quả giả.

## 7.5. Chương 4 — Kết luận và kiến nghị

Phải có:

- Tổng kết công việc.
- Kết quả đạt được.
- Kiến thức chuyên môn học được.
- Kỹ năng nghề nghiệp học được.
- Hạn chế.
- Định hướng phát triển.
- Kiến nghị với doanh nghiệp.
- Kiến nghị với nhà trường.

---

# 8. GHI CHÚ HÌNH, BẢNG VÀ SƠ ĐỒ

Chỗ nào cần hình, phải note thật rõ và **in đậm ngay tại đúng vị trí trong file DOCX**.

Không được chỉ ghi:

```text
[Chèn hình ở đây]
```

Ghi chú phải nói rõ:

- Cần hình gì.
- Chụp giao diện, code, database, sơ đồ hay kết quả kiểm thử.
- Chụp ở đâu.
- Route hoặc menu nào.
- File code, class, function hoặc component nào.
- Cần đăng nhập bằng vai trò nào.
- Thực hiện thao tác gì trước khi chụp.
- Cần chuẩn bị dữ liệu mẫu gì.
- Dữ liệu nào phải che.
- Tên chú thích hình dự kiến.

## 8.1. Mẫu hình giao diện

```text
[CHÈN HÌNH 2.5: Chụp giao diện trang quản lý người dùng tại route `/users`, đăng nhập bằng vai trò quản trị viên, mở danh sách người dùng và hiển thị trạng thái hoạt động. Dùng dữ liệu mẫu hoặc che tên, email và số điện thoại thật. Chú thích dự kiến: “Hình 2.5. Giao diện quản lý người dùng”.]
```

Toàn bộ ghi chú trên phải được in đậm trong DOCX.

## 8.2. Mẫu hình code

```text
[CHÈN HÌNH 2.8: Chụp đoạn code trong file `[đường dẫn thật]`, tập trung vào class/function `[tên thật]` thể hiện logic phân quyền. Chỉ chụp phần logic chính, không chụp mật khẩu, token, API key hoặc connection string. Chú thích dự kiến: “Hình 2.8. Cơ chế kiểm tra quyền truy cập”.]
```

Toàn bộ ghi chú trên phải được in đậm trong DOCX.

## 8.3. Mẫu sơ đồ

```text
[CHÈN SƠ ĐỒ 2.2: Dựng sơ đồ kiến trúc tổng thể dựa trên source code thực tế, thể hiện frontend, backend, database, storage và dịch vụ bên ngoài. Chú thích dự kiến: “Hình 2.2. Kiến trúc tổng thể của hệ thống”.]
```

Toàn bộ ghi chú trên phải được in đậm trong DOCX.

## 8.4. Mẫu bảng

```text
[CHÈN BẢNG 2.1: Bảng tổng hợp công nghệ gồm các cột Nhóm công nghệ, Công nghệ/thư viện, Phiên bản, Vai trò và File xác nhận. Chỉ lấy dữ liệu từ dependency và cấu hình thực tế. Chú thích dự kiến: “Bảng 2.1. Công nghệ sử dụng trong dự án”.]
```

Toàn bộ ghi chú trên phải được in đậm trong DOCX.

## 8.5. Cách tạo placeholder để giữ bố cục

Khi chưa có hình thật, không được chỉ chèn một dòng chữ ngắn rồi bỏ trống toàn bộ dung lượng ảnh.

Phải tạo một khung placeholder rõ ràng tại đúng vị trí dự kiến, ưu tiên dùng bảng một ô hoặc khung có viền, gồm:

- Ghi chú in đậm theo đúng mẫu ở trên.
- Kích thước gần với hình/sơ đồ dự kiến.
- Caption dự kiến đặt đúng vị trí.
- Không dùng ảnh giả hoặc ảnh không lấy từ hệ thống.

Mục đích là giữ bố cục và số trang gần với bản hoàn chỉnh sau khi sinh viên thay ảnh thật.

## 8.6. Bảo mật hình ảnh

Không được yêu cầu hoặc chèn hình để lộ:

- Mật khẩu.
- Token.
- API key.
- Secret.
- Connection string đầy đủ.
- Cookie đăng nhập.
- Dữ liệu cá nhân thật chưa được phép.
- Dữ liệu production nhạy cảm.

Phải dùng dữ liệu mẫu hoặc ghi rõ phần cần che.

---

# 9. ĐỊNH DẠNG DOCX THEO MẪU TRƯỜNG

Phải kiểm tra trực tiếp file mẫu và áp dụng đúng.

Tối thiểu phải bảo đảm:

- Font Times New Roman.
- Khổ A4.
- In một mặt.
- Lề trái 3 cm.
- Lề phải 2 cm.
- Lề trên 2 cm.
- Lề dưới 2 cm.
- Giãn dòng từ 1.3 đến 1.5.
- Đánh số trang ở cuối giữa trang.
- Trang 1 bắt đầu từ Lời mở đầu.
- Các trang trước Lời mở đầu dùng số La Mã, trừ trang bìa và trang lót bìa.
- Mục lục có khả năng cập nhật tự động trong Word.
- Hình và bảng đánh số theo chương.
- Hình và bảng có caption.
- Chương `x`: cỡ 16, in đậm, chữ hoa.
- Mục `x.1`: cỡ 14, in đậm, chữ hoa.
- Mục `x.1.1`: cỡ 13, in đậm.
- Mục `x.1.1.1`: cỡ 13, in nghiêng.
- Không đánh mục quá bốn cấp.
- Không để sót cú pháp Markdown trong DOCX.
- Không để tràn trang, chồng chữ hoặc cắt nội dung.
- Bản DOCX phải có thể xuất thành một file PDF duy nhất.
- Phải dùng style Heading thực sự cho các cấp tiêu đề, không chỉ làm đậm/cỡ chữ thủ công.
- Mục lục phải là trường TOC hoặc cấu trúc có thể cập nhật trong Microsoft Word; phải bật tùy chọn cập nhật field khi mở file nếu công cụ hỗ trợ.
- Page number, caption, danh mục hình và danh mục bảng phải sử dụng field/cấu trúc Word phù hợp khi công cụ hỗ trợ; không được gõ số trang mục lục giả rồi coi là hoàn thành.
- Không được làm mất header, footer, section break, bảng biểu mẫu, content control hoặc trường Word bắt buộc có sẵn trong file mẫu.
- Không được để tracked changes, comment kỹ thuật, metadata nhạy cảm hoặc đường dẫn tạm xuất hiện trong bản nộp cuối.

---

# 10. TỰ KIỂM TRA, RENDER VÀ XÁC MINH TRƯỚC KHI HOÀN THÀNH

Không được tuyên bố hoàn thành chỉ vì script đã chạy không báo lỗi. Phải thực hiện đầy đủ các lớp kiểm tra sau.

## 10.1. Kiểm tra file mục lục

- Đã đọc toàn bộ mẫu trường.
- Đã đọc toàn bộ source code/tài liệu cần thiết, không chỉ README.
- Mục lục chia tối thiểu đến cấp `1.1.1`.
- Mỗi mục có số trang dự kiến và khoảng trang dự kiến.
- Mỗi mục có task.
- Mỗi task có source/tài liệu cần đối chiếu.
- Các vị trí cần hình, bảng và sơ đồ được ghi rõ.
- Các thông tin thiếu có placeholder cụ thể.
- Tổng số trang dự kiến nằm trong khoảng 97–103.
- Không có chức năng, kết quả, số liệu hoặc thông tin doanh nghiệp bị bịa.
- Tổng số trang theo từng phần cộng lại chính xác.

## 10.2. Kiểm tra cấu trúc DOCX

Phải:

- Xác nhận file tồn tại, không rỗng và có dung lượng hợp lý.
- Kiểm tra gói DOCX/ZIP không bị lỗi hoặc thiếu thành phần bắt buộc.
- Mở lại file bằng ít nhất một thư viện/công cụ đọc DOCX.
- Kiểm tra đầy đủ các section, heading, bảng, header, footer và page number.
- Kiểm tra tất cả mục trong mục lục ràng buộc đều có mặt trong DOCX.
- Kiểm tra không còn cú pháp Markdown, marker kỹ thuật, đường dẫn file tạm hoặc nội dung debug.
- Kiểm tra không còn comment/tracked changes ngoài những thành phần mẫu bắt buộc.
- Kiểm tra không lộ secret, token, mật khẩu, cookie, connection string hoặc dữ liệu thật nhạy cảm.

## 10.3. Render và kiểm tra bố cục

Phải dùng Microsoft Word, LibreOffice headless hoặc công cụ render DOCX tương thích có sẵn để:

1. Xuất bản DOCX làm việc thành PDF tạm.
2. Render PDF/DOCX thành ảnh từng trang nếu môi trường cho phép.
3. Đếm số trang thực tế.
4. Kiểm tra toàn bộ các trang ở mức phóng đại đủ đọc, không chỉ xem vài trang đầu.
5. Kiểm tra không có:
   - Chữ bị cắt hoặc chồng lên nhau.
   - Bảng tràn lề hoặc vỡ hàng.
   - Heading nằm cuối trang nhưng nội dung sang trang sau bất hợp lý.
   - Trang trắng ngoài ý muốn.
   - Header/footer sai section.
   - Số trang sai hoặc mất.
   - Font lỗi, ký tự lỗi hoặc caption tách sai.
   - Placeholder hình quá nhỏ, quá lớn hoặc phá bố cục.
6. Sửa lỗi và render lại cho đến khi đạt.

Nếu môi trường thực sự không có công cụ render đủ tin cậy, phải thực hiện tối đa kiểm tra cấu trúc có thể và ghi rõ giới hạn trong phản hồi cuối cùng. Không được tuyên bố đã xác nhận chính xác bố cục hoặc số trang thực tế nếu chưa render.

## 10.4. Kiểm tra số trang thật

- Bản render cuối phải nằm trong khoảng 97–103 trang theo cách tính đã xác định ở mục 5.2.
- Không được chỉ dựa vào số trang ước tính trong mục lục.
- Nếu thiếu trang, phải bổ sung chiều sâu kỹ thuật, phân tích quy trình, bằng chứng, bảng hoặc sơ đồ có giá trị; không dùng nội dung lặp hoặc khoảng trắng.
- Nếu dư trang, phải tinh gọn phần lặp, tối ưu bảng/ảnh và bố cục; không cắt bỏ nội dung bắt buộc.
- Sau khi điều chỉnh số trang, phải render và đếm lại.

## 10.5. Kiểm tra nội dung và tính nhất quán

- Đầy đủ cấu trúc theo mẫu trường.
- Đầy đủ các mục trong file mục lục.
- Không có chương nào làm qua loa.
- Chương 2 là chương chi tiết nhất.
- Nội dung bám source code và tài liệu thật.
- Không sao chép README một cách máy móc.
- Không có nội dung bịa.
- Không lặp đoạn để kéo dài báo cáo.
- Thuật ngữ, tên module, vai trò, công nghệ và số liệu nhất quán.
- Ghi chú hình được in đậm, đủ chi tiết và nằm đúng vị trí.
- Heading đúng cấp và dùng style thật.
- Font, lề, giãn dòng, section và đánh số trang đúng mẫu.
- Hình và bảng được đánh số đúng chương.
- Tài liệu tham khảo chỉ chứa nguồn thực sự đã sử dụng và không bịa thông tin xuất bản.

## 10.6. Kiểm tra đầu ra cuối cùng

Trước khi xóa file tạm phải:

1. Sao chép/thay thế an toàn bản working đã đạt kiểm tra thành file chính thức.
2. Mở lại chính file `Thực tập/BAO_CAO_THUC_TAP_TOT_NGHIEP.docx`.
3. Render lại chính file cuối nếu công cụ cho phép.
4. Xác nhận file cuối giống bản đã kiểm tra.
5. Chỉ sau đó mới xóa `Thực tập/.tmp_bao_cao/`.
6. Xác nhận thư mục `Thực tập/` chỉ còn đúng hai file bắt buộc.

---

# 11. XỬ LÝ LỖI VÀ NGUYÊN TẮC KHÔNG ĐƯỢC DỪNG SỚM

Nếu một lệnh hoặc thư viện tạo DOCX thất bại:

- Không được xóa bản working hoặc last-good còn hợp lệ.
- Phải đọc lỗi, sửa script/cách xử lý trong thư mục tạm và thử lại.
- Không được thay đổi source code để giải quyết lỗi tạo tài liệu.
- Không được chuyển sang tạo báo cáo Markdown thay thế.
- Không được tuyên bố hoàn thành khi chỉ có mục lục hoặc DOCX chưa kiểm tra.

Nếu thiếu thông tin cá nhân, doanh nghiệp, nhật ký tuần, chữ ký hoặc điểm đánh giá:

- Dùng placeholder đúng vị trí.
- Tiếp tục hoàn thành toàn bộ phần còn lại.
- Không dừng để hỏi từng thông tin nhỏ.

Chỉ được dừng mà chưa hoàn thành khi có trở ngại kỹ thuật thực sự không thể tiếp tục sau khi đã thử các phương án an toàn có sẵn. Khi đó phải giữ lại bản working/last-good, không xóa file tạm có giá trị và báo cáo chính xác phần đã hoàn thành cùng lỗi cụ thể; không được nói chung chung.

---

# 12. LỆNH THỰC THI CUỐI CÙNG

Hãy thực hiện toàn bộ công việc ngay theo đúng thứ tự:

1. Xác định chính xác file mẫu bắt buộc.
2. Đọc và phân tích toàn bộ cấu trúc, style, section, bảng và biểu mẫu trong file mẫu.
3. Đọc kỹ toàn bộ source code và tài liệu dự án có liên quan; bỏ qua dependency/build artifact sinh tự động.
4. Không thay đổi source code, database, dependency, cấu hình hoặc Git.
5. Chỉ ghi file trong `Thực tập/` và file tạm trong `Thực tập/.tmp_bao_cao/`.
6. Tạo `Thực tập/00_MUC_LUC_RANG_BUOC_BAO_CAO.md`.
7. Tự kiểm tra mục lục, bằng chứng và phân bổ 97–103 trang.
8. Sao chép file mẫu thành bản DOCX working.
9. Dựa vào mục lục vừa tạo, viết hoàn chỉnh báo cáo từ đầu đến cuối.
10. Lưu tăng dần sau từng phần lớn bằng cơ chế file next → kiểm tra → thay thế an toàn.
11. Nếu phiên bị gián đoạn, tiếp tục từ bản working hợp lệ; không bắt đầu lại.
12. Tạo placeholder hình/bảng/sơ đồ đúng vị trí và giữ gần đúng diện tích dự kiến.
13. Hoàn thiện mục lục tự động, style, section, header/footer, page number, caption và các biểu mẫu.
14. Kiểm tra cấu trúc DOCX.
15. Render thành PDF/ảnh, kiểm tra toàn bộ bố cục và đếm số trang thực tế.
16. Sửa và render lại đến khi đạt yêu cầu 97–103 trang và không còn lỗi bố cục đáng kể.
17. Tạo file DOCX chính thức bằng thao tác thay thế an toàn.
18. Mở và kiểm tra lại chính file DOCX cuối cùng.
19. Xóa toàn bộ file tạm chỉ sau khi xác minh thành công.
20. Không tạo báo cáo tiến độ, checkpoint, worklog hoặc bản báo cáo Markdown.
21. Không dừng sau khi tạo mục lục.
22. Không làm qua loa bất kỳ chương nào.
23. Hoàn tất và xác minh cả hai file rồi mới kết thúc.

Phản hồi cuối cùng phải ngắn gọn, chỉ nêu:

- Hai file đã hoàn thành.
- Số trang thực tế đã kiểm tra, nếu đã render được.
- Các placeholder `[CẦN SINH VIÊN BỔ SUNG: ...]` còn tồn tại.
- Bất kỳ giới hạn kiểm tra nào thực sự không thể thực hiện.

Không kể lại toàn bộ quá trình và không tạo file báo cáo tiến độ.

**Kết quả cuối cùng trong thư mục `Thực tập/` chỉ gồm đúng hai file:**

```text
Thực tập/00_MUC_LUC_RANG_BUOC_BAO_CAO.md
Thực tập/BAO_CAO_THUC_TAP_TOT_NGHIEP.docx
```
