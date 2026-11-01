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

## Trả lời câu hỏi

### 1. Vì sao thiết bị không nên gọi trực tiếp MaintenanceService?

Nhiệm vụ của thiết bị là theo dõi trạng thái của chính nó và phát thông báo khi có thay đổi hoặc sự cố. Việc tạo yêu cầu bảo trì thuộc trách nhiệm của `MaintenanceService`. Nếu gọi trực tiếp dịch vụ này, lớp thiết bị sẽ phụ thuộc vào cách xử lý bảo trì và có thể phải sửa khi cách xử lý thay đổi.

Sử dụng event giúp tách hai trách nhiệm đó: thiết bị phát `AlertRaised`, còn `MaintenanceService` đăng ký nhận cảnh báo và chỉ tạo yêu cầu khi mức độ là `Critical`. Các thành phần khác cũng có thể đăng ký để ghi log, hiển thị thông báo hoặc gửi email mà không cần thay đổi mã của thiết bị.

### 2. Khi nào cần một event riêng, khi nào có thể dùng chung một event?

Nên tạo event riêng khi điều cần thông báo có ý nghĩa khác nhau hoặc cần dữ liệu khác nhau. Trong bài này, `StatusChanged` thông báo việc đổi trạng thái nên cần trạng thái cũ và mới; `AlertRaised` thông báo sự cố nên cần nội dung và mức độ cảnh báo. Vì vậy, hai trường hợp được tách thành hai event.

Có thể dùng chung event khi các thông báo cùng mục đích và biểu diễn được bằng cùng cấu trúc dữ liệu. Ví dụ, pin yếu ở máy chiếu và tín hiệu yếu ở micro đều dùng `AlertRaised` với `DeviceAlertEventArgs`. Subscriber phân biệt chúng qua mã thiết bị, nội dung và mức độ cảnh báo, nên không cần tạo event riêng cho từng loại thiết bị.

### 3. Thành phần nào có quyền phát sinh event?

Trong C#, chỉ lớp khai báo event mới được trực tiếp gọi `Invoke` cho event đó. Ở bài này, `TeachingDevice` khai báo `StatusChanged` và `AlertRaised`, đồng thời phát chúng trong các phương thức `OnStatusChanged` và `OnAlertRaised`.

Các lớp dẫn xuất như `ProjectorRemote`, `AirConditionerRemote` và `WirelessMicrophone` có thể yêu cầu phát cảnh báo thông qua phương thức `protected` như `RaiseAlert` hoặc `OnAlertRaised`, nhưng không trực tiếp gọi `Invoke` trên event của lớp cha. Các đối tượng bên ngoài chỉ được đăng ký bằng `+=` và hủy đăng ký bằng `-=`, không được tự phát event.
