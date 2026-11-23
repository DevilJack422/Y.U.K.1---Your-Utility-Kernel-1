# PROJECT PLAN — Y.U.K.1 (Your Utility Kernel 1)

## 1. Tổng quan dự án
- **Tên hiển thị (UI):** **Y.U.K.1** (*Your Utility Kernel 1*)
- **Tên Project / Namespace / File thực thi:** `YUK1` (`YUK1.exe`)
- **Mục tiêu:** Xây dựng Local AI System Agent chạy nền siêu nhẹ trên Windows 11, hướng tới đóng gói và phát hành **Open-Source (MIT License) trên GitHub**.
- **Phần cứng mục tiêu:** HP Pavilion x360 (Intel Core i5-1335U, 16GB RAM, iGPU Intel Iris Xe, 512GB SSD).
- **Công nghệ cốt lõi:**
  - **Framework:** C# / .NET 8 hoặc .NET 9 (WPF), mục tiêu `< 50MB` RAM khi chạy nền.
  - **AI Runtime:** Ollama (tăng tốc Intel IPEX-LLM cho Iris Xe), dùng SLM 3B–7B hỗ trợ Function Calling, tự động giải phóng RAM qua `keep_alive`.
  - **Giao diện & Tương tác:** System Tray + Auto-start (`HKCU\...\Run`, không cần quyền Admin), Global Hotkey (`Alt + Space`), cửa sổ nổi không viền `420 × 600 px` gồm 4 tầng:
    1. **Header:** Tên **Y.U.K.1** + đèn báo trạng thái nạp model vào RAM.
    2. **Dải chỉ số thời gian thực:** CPU / RAM / Network.
    3. **Luồng chat & HITL:** Khung hội thoại kèm thẻ xác nhận hành động (*Human-in-the-loop*).
    4. **Thanh lệnh nhanh:** Các thao tác/câu lệnh thường dùng.

---

## 2. Tiêu chuẩn thiết kế Open-Source (Bắt buộc)
1. **Tách biệt cấu hình (`appsettings.json`):** Không hardcode tên model, URL Ollama (`http://localhost:11434`), `keep_alive`, chu kỳ quét tài nguyên hay ngưỡng cảnh báo trong code C#.
2. **Repo sạch (`.gitignore`):** Loại trừ `bin/`, `obj/`, `.vs/`, file trọng số AI nặng, và các file nhật ký làm việc nội bộ (`PROGRESS_LOG.md`, `TEST_REPORT.log`).
3. **Zero Hardcoded Paths:** Tuyệt đối không dùng đường dẫn cá nhân; luôn dùng `AppContext.BaseDirectory`, `Environment.GetFolderPath(...)` hoặc đường dẫn tương đối.
4. **Onboarding Health Check:** Kiểm tra cổng `localhost:11434` và sự tồn tại của model trong `appsettings.json` lúc khởi chạy; hiển thị hướng dẫn thân thiện trên UI nếu chưa sẵn sàng.

---

## 3. Lộ trình các giai đoạn (Milestones)

### Giai đoạn 1: Khởi tạo & Chuẩn hóa Repo Open-Source
- [x] Kiểm tra môi trường: `.NET 8/9/10 SDK`, VS Code + C# Dev Kit, Ollama (`0.40.16`).
- [x] Khởi tạo project WPF `YUK1` bằng CLI.
- [x] Tạo `.gitignore` chuẩn .NET/WPF + Open-Source.
- [x] Thiết kế và tạo `appsettings.json` (cấu hình Ollama, Monitor, UI/Hotkey).
- [x] Tích hợp `Microsoft.Extensions.Configuration.Json` để đọc cấu hình an toàn.

### Giai đoạn 2: Kiến trúc Chạy Ngầm & Cửa Sổ Nổi
- [ ] Tích hợp System Tray Icon (`H.NotifyIcon.Wpf`) và menu ngữ cảnh (Mở/Ẩn, Khởi động cùng Windows, Thoát).
- [ ] Cài đặt cơ chế tự khởi động qua Registry `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` (không cần Admin).
- [ ] Đăng ký Global Hotkey (`Alt + Space`) bằng Win32 API (`RegisterHotKey` / `UnregisterHotKey`) xử lý đúng vòng đời cửa sổ để tránh leak handle.
- [ ] Dựng giao diện cửa sổ nổi không viền `420 × 600 px` (4 tầng) với hiệu năng nhẹ, tối ưu `< 50MB` RAM khi ẩn.

### Giai đoạn 3: Giám Sát Hệ Thống Thời Gian Thực
- [ ] Xây dựng Service đo CPU, RAM, Network bất đồng bộ (`async/await`, `PeriodicTimer`), không chặn UI Thread.
- [ ] Đọc chu kỳ quét và ngưỡng cảnh báo từ `appsettings.json`.
- [ ] Hiển thị lên Dải chỉ số thời gian thực và đảm bảo giải phóng `PerformanceCounter` / tài nguyên hệ thống đúng chuẩn `IDisposable`.

### Giai đoạn 4: Tích Hợp Ollama & Onboarding Health Check
- [ ] Xây dựng `OllamaService` (HttpClient tái sử dụng, hỗ trợ streaming/non-streaming, truyền `keep_alive` từ config).
- [ ] Cài đặt Health Check khi khởi động: kiểm tra `localhost:11434` (`/api/tags`) và đối chiếu model khai báo trong `appsettings.json`.
- [ ] Hiển thị thẻ hướng dẫn Onboarding trên UI nếu Ollama chưa chạy hoặc thiếu model.
- [ ] Cập nhật đèn báo trạng thái nạp RAM của model trên Header theo trạng thái thực tế (`/api/ps`).

### Giai đoạn 5: Function Calling & Human-in-the-Loop (HITL)
- [ ] Thiết kế JSON Schema cho các tool hệ thống (đọc thông tin hệ thống, liệt kê tiến trình, mở ứng dụng an toàn, v.v.).
- [ ] Xây dựng lớp kiểm duyệt & phân loại rủi ro: mọi hành động thay đổi trạng thái hệ thống (đóng tiến trình, thao tác file, chạy lệnh) bắt buộc đi qua thẻ xác nhận HITL trên UI.
- [ ] Hoàn thiện Luồng chat, Thẻ xác nhận hành động và Thanh lệnh nhanh.

### Giai đoạn 6: Kiểm Thử, Bảo Mật & Phát Hành Open-Source
- [ ] Code Review & Security Audit toàn bộ mã nguồn (chống command injection, không lọt đường dẫn cá nhân, kiểm tra memory leak).
- [ ] Chạy bộ test thực tế trên HP Pavilion x360 và ghi kết quả vào `TEST_REPORT.log`.
- [ ] Viết `README.md` (hướng dẫn cài đặt, kiến trúc, ảnh minh họa), `LICENSE` (MIT), và chuẩn hóa commit sẵn sàng đẩy lên GitHub.
