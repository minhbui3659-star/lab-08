# Lab 08 – Delegate & Event

- Họ tên: Bùi Nhật Minh
- MSSV: 202418944

Toàn bộ mã C# nằm trong `Program.cs`: các thiết bị, event, delegate lọc,
subscriber, lớp điều phối, ngoại lệ và 9 ca kiểm tra theo file mẫu.

## Chạy trên VS Code

1. Cài .NET SDK 8 và extension C# của Microsoft (hoặc C# Dev Kit).
2. Mở thư mục `lab-08` bằng VS Code.
3. Mở Terminal trong thư mục này và chạy:

```sh
dotnet run --project Assignment8-MI4090.csproj
```

Chương trình tự chạy TC1–TC9, in `PASS` hoặc `FAIL`. Các dòng cảnh báo
xuất hiện trong các ca kiểm tra là kết quả dự kiến. Nếu có ca thất bại,
chương trình trả mã thoát 1.

`Program.cs` chứa toàn bộ logic; `.csproj` chỉ cấu hình ứng dụng console .NET 8.
Không cần cài thêm thư viện NuGet.

## Thiết kế

- Publisher: `TeachingDevice` và ba lớp thiết bị dẫn xuất.
- Subscriber: `ConsoleNotificationService`, `DeviceAuditLogger`,
  `MaintenanceService`, `FilteredAlertSubscriber`.
- `StatusChanged` mang trạng thái cũ/mới và thời điểm thay đổi.
- `AlertRaised` mang mã thiết bị, nội dung, mức độ và thời điểm cảnh báo.
- `AlertFilter` và `Action<DeviceAlertEventArgs>` minh họa delegate/callback.
- `DeviceMonitoringCoordinator` quản lý đăng ký, hủy đăng ký và `Dispose`.

Pin và tín hiệu ban đầu là 100%, điều hòa ban đầu có phản hồi.
Vùng Critical là 0; Warning là 1–19 cho máy chiếu, 1–29 cho pin micro,
1–39 cho tín hiệu micro. Các giá trị còn lại thuộc vùng an toàn.
Chỉ phát cảnh báo khi chuyển sang một vùng Warning/Critical khác vùng cũ.
Phục hồi về vùng an toàn không phát cảnh báo; giảm trở lại sẽ cảnh báo lại.
Pin và tín hiệu micro được theo dõi độc lập. Giá trị ngoài 0–100 gây
`InvalidDeviceReadingException` trước khi dữ liệu bị thay đổi.

### Vì sao thiết bị không gọi trực tiếp MaintenanceService?

Thiết bị chỉ phát hiện và công bố sự cố. Subscriber quyết định ghi log,
hiển thị hay tạo yêu cầu bảo trì. Nhờ vậy có thể thêm subscriber mà không
sửa lớp thiết bị.

### Khi nào tách event, khi nào dùng chung?

Tách khi ý nghĩa hoặc dữ liệu khác nhau, như đổi trạng thái và cảnh báo.
Dùng chung khi cùng ý nghĩa và cấu trúc dữ liệu, như cảnh báo pin và tín hiệu.

### Thành phần nào có quyền phát sinh event?

`TeachingDevice` gọi `Invoke` trong các phương thức `On...` protected virtual.
Lớp dẫn xuất phát qua các phương thức protected. Mã bên ngoài chỉ đăng ký
và hủy đăng ký bằng `+=` và `-=`.
