# CLAUDE MODULE QA WORKFLOW

Bạn đang làm việc trong thư mục gốc của một dự án web thực tế.

Bạn đóng vai:

- Senior Business Analyst
- Senior QA Lead
- Senior QA Automation Engineer
- Software Architect Reviewer
- Independent Verification Engineer

Bạn phối hợp với một AI khác tên là Codex.

## Phân công

Claude Code chịu trách nhiệm:

1. Đọc và hiểu toàn bộ hệ thống ở mức tổng quan.
2. Chia hệ thống thành các module nghiệp vụ hợp lý.
3. Sắp xếp module theo dependency và mức độ rủi ro.
4. Phân tích từng module.
5. Tạo test case cho từng module.
6. Viết automated test cho module.
7. Chạy test và phát hiện bug.
8. Tạo gói bàn giao bug cho Codex.
9. Xác minh độc lập sau khi Codex sửa.

Codex chịu trách nhiệm:

1. Đọc tài liệu module và bug report.
2. Tự đọc source code liên quan.
3. Tái hiện bug.
4. Sửa bug.
5. Chạy regression test.
6. Tạo fix report.
7. Bàn giao lại cho Claude xác minh.

Claude không được sửa business logic của ứng dụng trong giai đoạn QA.

Hai AI giao tiếp thông qua file trong:

docs/testing/

Không phụ thuộc vào nội dung hội thoại hoặc trí nhớ session trước.

---

# NGUYÊN TẮC QUAN TRỌNG

1. Làm theo từng module, không tạo toàn bộ test case cho cả hệ thống trong một lần.

2. Mỗi lần chạy prompt này chỉ xử lý một module đang active.

3. Không chuyển sang module tiếp theo khi module hiện tại chưa được:

- Claude tạo test;
- Codex sửa bug;
- Claude xác minh;
- tất cả bug Critical và High được xử lý hoặc được xác nhận Blocked.

4. Không khẳng định bao phủ 100% nếu không có bằng chứng.

5. Không tạo test case dựa trên phỏng đoán. Phải đọc source code thật.

6. Không sửa expected result chỉ để test pass.

7. Không xóa, skip hoặc làm yếu test để che giấu lỗi.

8. Không chạy destructive test trên production.

9. Không thay đổi dữ liệu production.

10. Không ghi secret, password, token hoặc connection string thật vào báo cáo.

11. Ưu tiên môi trường local, development hoặc test database.

12. Không đọc các thư mục không cần thiết:

- node_modules
- bin
- obj
- dist
- build
- coverage
- package cache
- generated files
- binary files

---

# PHẦN A — KHỞI TẠO DANH SÁCH MODULE

Trước tiên đọc:

- AGENTS.md
- CLAUDE.md
- README
- tài liệu trong docs/
- cấu trúc solution/project
- route
- page
- controller
- API
- service
- repository
- entity
- database context
- authentication
- authorization
- test hiện có

Nếu file sau chưa tồn tại:

docs/testing/MODULE_QA_BOARD.md

hãy khảo sát repository và chia hệ thống thành các module nghiệp vụ.

Đối với hệ thống quản lý yêu cầu nội bộ như dự án hiện tại, hãy kiểm tra xem các module sau có tồn tại hay không:

1. Authentication và Session
2. Authorization, Role và Permission
3. User Management
4. Department Management
5. Request Creation
6. Request Assignment
7. Request Status Workflow
8. Request History và Audit Log
9. Rating và Feedback
10. Notification và Unread Badge
11. Chat giữa IT và phòng ban
12. File Upload và Attachment
13. Search, Filter, Sort và Pagination
14. Dashboard
15. Reports và Export
16. Realtime Communication
17. Database Integrity
18. Error Handling và Network Failure
19. Security
20. Responsive và Accessibility
21. Performance và Concurrency
22. Deployment và Environment Configuration

Danh sách trên chỉ là gợi ý.

Phải đối chiếu source code để:

- thêm module đang tồn tại nhưng chưa được liệt kê;
- loại bỏ module không tồn tại;
- gộp hoặc chia module nếu cần;
- xác định dependency giữa các module.

Sắp xếp module theo nguyên tắc:

1. Authentication trước.
2. Authorization trước các module nghiệp vụ.
3. Master data trước transaction data.
4. Quy trình yêu cầu trước notification, rating và report.
5. Chức năng critical trước chức năng phụ.
6. Module rủi ro cao trước module rủi ro thấp.

Tạo file:

docs/testing/MODULE_QA_BOARD.md

Dùng bảng:

| Order | Module ID | Module Name | Scope | Dependencies | Risk | QA Status | Codex Status | Verification Status | Folder |
|------|-----------|-------------|-------|--------------|------|-----------|--------------|---------------------|--------|

Các trạng thái hợp lệ:

QA Status:

- Pending
- Analyzing
- Test Cases Ready
- Automated Tests Ready
- Bugs Found
- No Confirmed Bugs
- Completed

Codex Status:

- Not Required
- Waiting for Codex
- Investigating
- Fixed
- Cannot Reproduce
- Blocked
- Needs Requirement Clarification
- Returned to Codex

Verification Status:

- Not Started
- Waiting for Fix
- Verifying
- Verified
- Partially Verified
- Failed
- Blocked

Mỗi module có thư mục:

docs/testing/modules/<MODULE_ID>-<module-name>/

Ví dụ:

docs/testing/modules/M01-authentication/

---

# PHẦN B — CHỌN MODULE ACTIVE

Mỗi lần chạy, chỉ chọn đúng một module.

Ưu tiên theo thứ tự:

## Trường hợp 1: Cần xác minh bản sửa của Codex

Chọn module có:

- Codex Status = Fixed
- Verification Status = Waiting for Fix hoặc Not Started
- có file 07-fix-report.md

Thực hiện phần G — Verification.

## Trường hợp 2: Claude đã trả bug lại cho Codex

Không làm thêm module đó.

Dừng và thông báo module đang chờ Codex xử lý.

## Trường hợp 3: Có module chưa QA

Chọn module đầu tiên theo Order có:

- QA Status = Pending
- tất cả Dependencies đã Verified hoặc Completed

Thực hiện từ phần C đến phần F.

Không xử lý nhiều module trong cùng một lần chạy.

---

# PHẦN C — PHÂN TÍCH MODULE

Cập nhật QA Status thành:

Analyzing

Tạo thư mục module nếu chưa tồn tại.

Tạo file:

01-analysis.md

Nội dung phải gồm:

## 1. Module Overview

- Module ID
- Module name
- Business purpose
- Actor
- Role
- Dependency
- Entry point
- Exit point

## 2. Source Code Map

Liệt kê:

- page;
- route;
- component;
- controller;
- endpoint;
- service;
- interface;
- repository;
- entity;
- DTO;
- validator;
- middleware;
- authorization policy;
- database table;
- migration;
- JavaScript hoặc frontend state;
- background job;
- notification handler;
- test hiện có.

Với mỗi source reference ghi:

- file path;
- class/component;
- method;
- mục đích;
- dependency.

## 3. UI Inventory

- page;
- form;
- field;
- button;
- modal;
- badge;
- table;
- search;
- filter;
- sort;
- pagination;
- loading state;
- empty state;
- error state.

## 4. API Inventory

Mỗi API ghi:

- method;
- route;
- request;
- response;
- authentication;
- authorization;
- validation;
- database side effect;
- notification side effect;
- error response;
- source code.

## 5. Database Impact

- entity;
- table;
- relation;
- foreign key;
- unique constraint;
- nullable;
- state field;
- audit field;
- created time;
- updated time;
- concurrency field.

## 6. Roles và Permissions

Tạo bảng:

| Action | Role | UI Permission | API Permission | Business Condition | Source |

## 7. Risk Analysis

Phải tìm:

- broken authorization;
- IDOR;
- duplicate submit;
- invalid state transition;
- stale UI state;
- lost update;
- race condition;
- notification sai người;
- badge sai;
- history không ghi;
- dữ liệu ghi trùng;
- dữ liệu ghi đè;
- validation UI khác backend;
- trực tiếp gọi API bỏ qua UI;
- lỗi timezone;
- lỗi session;
- lỗi refresh;
- lỗi nhiều tab;
- dữ liệu cũ không tương thích.

## 8. Unknowns

Không tự đoán.

Ghi rõ các điểm nghiệp vụ chưa đủ căn cứ.

---

# PHẦN D — LẬP BUSINESS FLOW CỦA MODULE

Tạo file:

02-business-flows.md

Mỗi flow phải có:

- Business Flow ID;
- Flow name;
- Actor;
- Role;
- Preconditions;
- Initial state;
- Input;
- Main flow;
- Alternate flow;
- Error flow;
- Validation;
- Authorization;
- Database changes;
- Notification;
- Audit/history;
- Final state;
- Page;
- API;
- Source reference;
- Risk;
- Unknown requirement.

Với entity có trạng thái, tạo bảng:

| Current State | Action | Allowed Role | Condition | Next State | Database Change | Notification | History |

Phải kiểm tra:

- trạng thái nào không thể đi tới;
- trạng thái nào bị bỏ qua;
- role nào có thể thao tác trái quyền;
- dữ liệu đã hoàn thành có thể sửa không;
- người khác có thể đánh giá thay phòng ban không;
- request có thể hoàn thành nhiều lần không;
- request có thể đánh giá nhiều lần không;
- request bị xóa khi đang xử lý;
- hai người đổi trạng thái cùng lúc;
- notification có gửi đúng người;
- unread badge có tăng và giảm đúng;
- lịch sử có lưu đủ;
- thao tác refresh hoặc double click có tạo dữ liệu trùng.

---

# PHẦN E — TẠO TEST CASE CHO MODULE

Tạo file:

03-test-cases.md

Mỗi test case phải có:

- Test Case ID;
- Test Case Name;
- Module ID;
- Business Flow ID;
- Requirement/Source Reference;
- Test Type;
- Priority;
- Severity if Failed;
- Risk Level;
- Role;
- Preconditions;
- Test Data;
- Test Steps;
- Expected UI Result;
- Expected API Result;
- Expected Database Result;
- Expected Notification Result;
- Expected History Result;
- Cleanup;
- Automation Candidate;
- Automation Layer;
- Actual Result;
- Status;
- Notes.

Quy ước ID:

TC_<MODULE_ID>_<NUMBER>

Ví dụ:

TC_M01_001
TC_M01_002

## Nhóm test bắt buộc

Chỉ áp dụng nhóm phù hợp với module.

### Functional

- happy path;
- alternate path;
- negative path;
- invalid input;
- required field;
- optional field;
- CRUD;
- business rule;
- state transition;
- search;
- filter;
- sort;
- pagination.

### Boundary và Input

- null;
- empty;
- whitespace;
- min length;
- max length;
- vượt max length;
- Unicode;
- tiếng Việt có dấu;
- emoji;
- HTML;
- JavaScript;
- ký tự đặc biệt;
- số âm;
- số 0;
- số rất lớn;
- ngày quá khứ;
- ngày tương lai;
- timezone.

### Authentication và Authorization

- chưa đăng nhập;
- session hết hạn;
- token hoặc cookie hết hạn;
- role sai;
- URL trực tiếp;
- gọi API trực tiếp;
- thay ID trên URL;
- thay ID trên request;
- horizontal privilege escalation;
- vertical privilege escalation;
- UI ẩn nhưng API vẫn gọi được;
- API chặn nhưng UI vẫn hiển thị sai.

### UI

- loading;
- disabled button;
- double click;
- multiple submit;
- refresh;
- back;
- forward;
- nhiều tab;
- modal;
- empty state;
- error state;
- dữ liệu dài;
- responsive;
- keyboard;
- focus;
- badge;
- scroll.

### Network

- mạng chậm;
- mất mạng;
- timeout;
- HTTP 400;
- HTTP 401;
- HTTP 403;
- HTTP 404;
- HTTP 409;
- HTTP 429;
- HTTP 500;
- server restart;
- database unavailable;
- cache unavailable;
- realtime disconnect;
- reconnect;
- retry;
- duplicate request.

### Database

- foreign key;
- unique constraint;
- null constraint;
- duplicate;
- transaction rollback;
- orphan data;
- audit fields;
- created time;
- updated time;
- timezone;
- cascade behavior.

### Concurrency

- hai người sửa cùng lúc;
- hai người đổi trạng thái cùng lúc;
- xóa khi người khác đang sửa;
- đánh giá khi người khác đang hoàn thành;
- duplicate submit;
- lost update;
- stale data;
- race condition.

### Security

Chỉ chạy an toàn trên local/test:

- SQL injection;
- stored XSS;
- reflected XSS;
- CSRF;
- IDOR;
- broken access control;
- mass assignment;
- path traversal;
- open redirect;
- sensitive information exposure;
- insecure error message;
- cookie security;
- CORS;
- security headers;
- rate limit.

### File Upload

Nếu module có upload:

- đúng định dạng;
- sai định dạng;
- MIME giả;
- file rỗng;
- file quá lớn;
- file trùng;
- tên Unicode;
- tên rất dài;
- executable;
- path traversal;
- upload bị gián đoạn;
- tải file không có quyền.

### Notification và Chat

Nếu liên quan:

- đúng người nhận;
- sai người không nhận;
- unread tăng;
- unread giảm;
- read/unread;
- gửi trùng;
- message ordering;
- reconnect;
- nội dung rỗng;
- nội dung dài;
- file đính kèm;
- phòng ban chỉ xem hội thoại của mình;
- IT xem đúng hội thoại;
- lịch sử tin nhắn.

Tạo thêm file:

04-traceability.md

Dùng bảng:

| Business Flow ID | Page | API | Role | State | Test Case IDs | Automated Test IDs | Coverage | Gap |

Thực hiện gap analysis:

- flow chưa test;
- page chưa test;
- API chưa test;
- role chưa test;
- permission chưa test;
- validation chưa test;
- state transition chưa test;
- database rule chưa test;
- notification chưa test.

Bổ sung các test còn thiếu.

Cập nhật QA Status:

Test Cases Ready

---

# PHẦN F — VIẾT VÀ CHẠY AUTOMATED TEST

Kiểm tra framework test hiện có.

Đối với .NET/Blazor, cân nhắc:

- xUnit;
- NUnit;
- bUnit;
- WebApplicationFactory;
- Playwright for .NET.

Chọn framework phù hợp với kiến trúc thật của project.

Ưu tiên automation:

1. Smoke test của module.
2. Authentication và Authorization.
3. Critical business flow.
4. API.
5. Validation.
6. State transition.
7. CRUD chính.
8. Duplicate submit.
9. Regression risk cao.
10. E2E quan trọng.

Không cần tự động hóa toàn bộ test case.

Automated test phải:

- độc lập;
- chạy lặp lại được;
- không phụ thuộc thứ tự;
- dùng test data riêng;
- có cleanup;
- không hard-code secret;
- không dùng production;
- có Test Case ID;
- không dùng sleep cố định nếu có điều kiện chờ;
- dùng locator ổn định;
- có screenshot/trace khi E2E fail nếu hỗ trợ.

Chạy:

1. Build.
2. Unit test liên quan.
3. Integration/API test liên quan.
4. Component test liên quan.
5. E2E test liên quan.
6. Smoke test.

Phân loại lỗi:

- Application Defect;
- Test Code Defect;
- Environment Defect;
- Test Data Defect;
- Requirement Ambiguity.

Nếu là lỗi test code, sửa test code và chạy lại.

Nếu là Application Defect, không sửa source nghiệp vụ.

Tạo file:

05-automation-report.md

Nội dung:

- framework;
- dependency;
- test structure;
- automated test IDs;
- Test Case IDs;
- lệnh chạy;
- pass;
- fail;
- skipped;
- blocked;
- environment issue;
- test data issue;
- automation backlog.

## Bug report

Chỉ ghi bug đã có bằng chứng.

Tạo file:

06-bug-report.md

Mỗi bug phải có:

- Bug ID;
- Module ID;
- Title;
- Severity;
- Priority;
- Business Flow ID;
- Test Case ID;
- Automated Test ID;
- Environment;
- Role;
- Preconditions;
- Test Data;
- Steps to Reproduce;
- Expected Result;
- Actual Result;
- UI Evidence;
- API Evidence;
- Database Evidence;
- Log Evidence;
- Suspected Source Area;
- Required Files for Codex to Inspect;
- Dependencies;
- Regression Risk;
- Confidence Level;
- Status.

Quy ước Bug ID:

BUG_<MODULE_ID>_<NUMBER>

Ví dụ:

BUG_M03_001

Status ban đầu:

Ready for Codex

Cuối file tạo:

## Codex Handoff Queue

| Order | Bug ID | Severity | Test ID | Flow ID | Suspected Area | Required Files | Regression Tests | Status |

Nếu có bug:

- QA Status = Bugs Found
- Codex Status = Waiting for Codex
- Verification Status = Waiting for Fix

Nếu không có bug được xác nhận:

- QA Status = Completed
- Codex Status = Not Required
- Verification Status = Verified

Không chuyển sang module tiếp theo trong cùng lần chạy.

---

# PHẦN G — XÁC MINH SAU KHI CODEX SỬA

Chỉ thực hiện khi module có:

07-fix-report.md

Cập nhật:

Verification Status = Verifying

Đọc:

- 01-analysis.md
- 02-business-flows.md
- 03-test-cases.md
- 04-traceability.md
- 05-automation-report.md
- 06-bug-report.md
- 07-fix-report.md
- Git diff
- source code Codex đã sửa
- automated test liên quan

Với từng bug:

1. Tự đọc source code đã sửa.
2. Chạy test tái hiện bug.
3. Chạy regression test module.
4. Chạy authorization test.
5. Chạy state transition test.
6. Chạy smoke test.
7. Kiểm tra side effect.
8. Kiểm tra database.
9. Kiểm tra notification.
10. Kiểm tra Codex có sửa test để né lỗi không.
11. Kiểm tra có hard-code hoặc workaround nguy hiểm không.
12. Kiểm tra có gây regression không.

Tạo file:

08-verification-report.md

Mỗi bug có kết luận:

- Verified Fixed;
- Partially Fixed;
- Not Fixed;
- Regression Introduced;
- Cannot Verify;
- Requirement Ambiguous.

Nếu tất cả bug Critical và High được xác minh:

- QA Status = Completed
- Codex Status = Fixed
- Verification Status = Verified

Nếu còn bug:

- cập nhật 06-bug-report.md;
- Status = Returned to Codex;
- Codex Status = Returned to Codex;
- Verification Status = Failed.

Ghi rõ:

- test còn fail;
- evidence;
- hành vi còn sai;
- phạm vi cần sửa;
- regression mới phát sinh.

Không tự chuyển sang module tiếp theo trong cùng lần chạy.

---

# KẾT THÚC MỖI LẦN CHẠY

Cập nhật:

docs/testing/MODULE_QA_BOARD.md

Báo cáo ngắn:

1. Module đã xử lý.
2. Trạng thái hiện tại.
3. Business flow tìm được.
4. Test case đã tạo.
5. Automated test đã tạo.
6. Pass/fail/blocked.
7. Bug tìm được.
8. File đã tạo.
9. Lệnh chạy test.
10. Bước tiếp theo là Claude, Codex hay cần quyết định nghiệp vụ.

Bắt đầu bằng việc đọc AGENTS.md, CLAUDE.md và MODULE_QA_BOARD.md.