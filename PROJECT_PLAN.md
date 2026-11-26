# KẾ HOẠCH DỰ ÁN: Y.U.K.1 (Your Utility Kernel 1)

> **Mục tiêu:** Xây dựng Desktop Companion Widget mã nguồn mở trên Windows 11 (.NET WPF), tích hợp giám sát phần cứng thời gian thực và trợ lý AI Local qua Ollama.
> **Phần cứng mục tiêu:** HP Pavilion x360 (Intel Core i5-1335U, 16GB RAM, Intel Iris Xe, 512GB SSD).
> **Nguyên tắc phân tách:**
> - **Luồng 1 (UI/UX):** Cung cấp mã XAML hoàn chỉnh (`Resources/`, `Views/`).
> - **Luồng 2 (Logic, Thuật toán & Cấu hình):** Chỉ cung cấp tài liệu đặc tả kỹ thuật (Class/Interface, Properties, Method Signatures, Workflow, Validation Rules) để lập trình viên tự triển khai, sau đó Review & Test.

---

## GIAI ĐOẠN 1: KHỞI TẠO NỀN TẢNG & HỆ THỐNG GIAO DIỆN (BƯỚC 0 - 1)
- **Luồng 1 (UI/UX):**
  - Thiết lập `Resources/Themes/YukiTheme.xaml` (Yuki Theme v1.0 - Midnight Lavender: bảng màu, brush, typography, corner radius, shadow/glow styles).
  - Thiết lập `App.xaml` (nạp `YukiTheme.xaml` vào `Application.Resources`, cấu hình `ShutdownMode="OnExplicitShutdown"` để chạy nền System Tray).
- **Luồng 2 (Logic & Cấu hình):**
  - Khởi tạo project WPF và cài đặt các gói NuGet (`H.NotifyIcon.Wpf`, `NHotkey.Wpf`, `Hardware.Info`, `Microsoft.Extensions.Configuration.Json`, `Microsoft.Extensions.Configuration.Binder`).
  - Tạo `.gitignore` chuẩn .NET Open-Source (loại trừ `bin/`, `obj/`, `.vs/`, `PROGRESS_LOG.md`, `TEST_REPORT.log`).
  - Thiết kế `appsettings.json` (cấu hình Ollama URL, Model, `keep_alive`, chu kỳ polling phần cứng, phím tắt, Allowlist lệnh hệ thống).
  - Xây dựng `AppSettings` model và `ConfigurationService` đọc cấu hình an toàn.

## GIAI ĐOẠN 2: KHUNG WIDGET, SYSTEM TRAY & PHÍM TẮT TOÀN CỤC
- **Luồng 1 (UI/UX):**
  - Thiết kế `Views/MainWindow.xaml`: cửa sổ không viền (`WindowStyle="None"`, `AllowsTransparency="True"`, `Topmost="True"`), bố cục 3 phần chính (Gương mặt LED, Lưới 6 chỉ số, Thanh nhập liệu) + Nút Toggle Lịch sử Chat ở góc trên phải.
- **Luồng 2 (Logic):**
  - `TrayIconService`: Quản lý biểu tượng System Tray qua `H.NotifyIcon.Wpf` (hiện/ẩn Widget, thoát ứng dụng).
  - `HotkeyService`: Đăng ký phím tắt toàn cục `Alt + Space` qua `NHotkey.Wpf` (không dùng Win32 P/Invoke thô).
  - `StartupService`: Quản lý tùy chọn tự khởi động cùng Windows (Registry `CurrentUser\...\Run` hoặc Task Scheduler an toàn).
  - Kéo thả Widget mượt mà trên Desktop và lưu vị trí hiển thị.

## GIAI ĐOẠN 3: GƯƠNG MẶT AI PIXEL-MATRIX & THEO DÕI CON TRỎ CHUỘT
- **Luồng 1 (UI/UX):**
  - Xây dựng UserControl `Views/Controls/PixelFaceControl.xaml`: Khung tròn nền đen sâu (`#000000` / `#0B0F17`), lưới pixel LED rời rạc (`UniformGrid` / `ItemsControl` với các ô bo nhẹ và hiệu ứng phát sáng).
- **Luồng 2 (Logic & Thuật toán):**
  - `IPixelExpressionEngine` / `PixelExpressionEngine`: Định nghĩa ma trận bit (ví dụ 16x16 hoặc 12x12) cho các trạng thái (`Idle` - đôi mắt viên nén dọc, `Thinking`, `Happy`, `Warning`, `Error`).
  - Thuật toán **Eye-Tracking**: Lấy tọa độ chuột toàn màn hình (qua WPF/System API hợp lệ, không P/Invoke thô nếu tránh được hoặc qua `System.Windows.Forms.Control.MousePosition`), tính vector hướng từ tâm Widget đến con trỏ chuột, chuẩn hóa và giới hạn biên dịch chuyển (`offsetX`, `offsetY`) cho cụm pixel mắt.
  - Bộ đếm nháy mắt (Blink animation timer) tự nhiên ở trạng thái `Idle`.

## GIAI ĐOẠN 4: GIÁM SÁT TÀI NGUYÊN HỆ THỐNG 6 Ô (< 50MB RAM NỀN)
- **Luồng 1 (UI/UX):**
  - Thiết kế 6 thẻ thông số (2 hàng × 3 cột) trong `MainWindow.xaml` / `ResourceCardControl.xaml`: `CPU`, `RAM`, `Memory`, `GPU`, `Net Up`, `Net Down`.
- **Luồng 2 (Logic & Hiệu năng):**
  - `IHardwareMonitorService` / `HardwareMonitorService`: Thu thập dữ liệu định kỳ (mặc định 1.5s–2s khi Widget hiện, tạm dừng/giảm tần suất khi Widget ẩn để giữ RAM `< 50MB`).
  - Xử lý bất đồng bộ (`Task.Run` / `PeriodicTimer`), tránh cấp phát bộ nhớ thừa (GC pressure), hỗ trợ đọc GPU Intel Iris Xe và tốc độ Network (`BytesSent`/`BytesReceived` delta theo giây).

## GIAI ĐOẠN 5: THANH NHẬP LIỆU, POPOVER HỒI ĐÁP & LỊCH SỬ CHAT
- **Luồng 1 (UI/UX):**
  - `Views/ResponsePopoverWindow.xaml` (hoặc Popup): Khung nổi `AI Response` bo góc, hiệu ứng glow nhẹ, hỗ trợ hiển thị văn bản trả lời và thẻ xác nhận hành động (Human-in-the-loop).
  - `Views/Controls/ChatHistoryOverlay.xaml`: Bảng lịch sử hội thoại ẩn/hiện khi bấm nút góc trên phải.
- **Luồng 2 (Logic):**
  - **Thuật toán định vị Popover thông minh:** Tính toán `WorkingArea` của màn hình chứa Widget; kiểm tra khoảng trống bên phải Widget, nếu không đủ rộng thì tự động đặt sát cạnh trái (và ngược lại).
  - **Cơ chế tự đóng:** `DispatcherTimer` 10 giây tự ẩn Popover; sự kiện click bên trong Popover đóng ngay lập tức (ngoại trừ click vào nút `Confirm` / `Cancel` của thẻ xác nhận hành động).
  - `ChatHistoryService`: Lưu trữ lịch sử phiên làm việc trong bộ nhớ (giới hạn số lượng tin nhắn để tiết kiệm RAM).

## GIAI ĐOẠN 6: OLLAMA LOCAL AI, FUNCTION CALLING & BẢO MẬT
- **Luồng 1 (UI/UX):**
  - Trạng thái kết nối Ollama (chỉ báo nhỏ hoặc thông báo trên Popover/PixelFace) và giao diện thẻ xác nhận lệnh hệ thống.
- **Luồng 2 (Logic & Bảo mật):**
  - `IOllamaClient` / `OllamaClient`: Giao tiếp REST API `http://localhost:11434`, kiểm tra kết nối lúc khởi động (Startup Health Check), cấu hình `keep_alive` để Ollama tự giải phóng RAM sau khi dùng.
  - `IToolRegistry` & `CommandSecurityGuard`: Cơ chế **Allowlist** (chỉ cho phép các hành động an toàn được khai báo trước) + **Human-in-the-loop** (bắt buộc người dùng bấm xác nhận trên Popover trước khi thực thi bất kỳ lệnh thay đổi hệ thống nào).

## GIAI ĐOẠN 7: KIỂM THỬ, TỐI ƯU & CHUẨN HÓA OPEN-SOURCE
- Đo đạc RAM nền (`< 50MB`), CPU usage, thời gian phản hồi Popover và độ trễ phím tắt `Alt + Space`.
- Viết Unit Test cho các thuật toán lõi (Popover positioning, Eye-tracking vector, Allowlist validation).
- Hoàn thiện `README.md` và `LICENSE` cho GitHub.
