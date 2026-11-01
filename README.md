# Bài tập tuần 8 – Delegate và Event

- Họ tên: Bùi Nhật Minh
- MSSV: 202418944

### 1. Vì sao thiết bị không nên gọi trực tiếp MaintenanceService?

Thiết bị có nhiệm vụ theo dõi trạng thái, phát hiện sự cố và phát thông báo. Còn việc tạo yêu cầu bảo trì do `MaintenanceService` xử lý. Nếu thiết bị gọi trực tiếp dịch vụ này thì hai lớp sẽ phụ thuộc vào nhau, gây khó khăn khi thay đổi hoặc bổ sung chức năng.

Vì vậy, thiết bị phát event `AlertRaised`, còn dịch vụ bảo trì đăng ký nhận event và xử lý các cảnh báo `Critical`. Sau này có thể thêm chức năng gửi email hoặc hiển thị thông báo mà không phải sửa lớp thiết bị.

### 2. Khi nào cần một event riêng, khi nào có thể dùng chung một event?

Cần tách event khi các sự việc có ý nghĩa khác nhau hoặc cần truyền dữ liệu khác nhau. Chẳng hạn, `StatusChanged` cần trạng thái cũ và mới, còn `AlertRaised` cần nội dung và mức độ cảnh báo nên được tách riêng.

Có thể dùng chung event khi các thông báo cùng mục đích và cùng dạng dữ liệu. Ví dụ, cảnh báo pin yếu của máy chiếu và tín hiệu yếu của micro đều dùng `AlertRaised`; thông tin cụ thể được truyền qua `DeviceAlertEventArgs`.

### 3. Thành phần nào có quyền phát sinh event?

Lớp khai báo event có quyền trực tiếp gọi `Invoke`. Trong bài này, `TeachingDevice` khai báo hai event `StatusChanged` và `AlertRaised`, rồi phát chúng qua `OnStatusChanged` và `OnAlertRaised`.

Các lớp con phát sự kiện thông qua các phương thức `protected` của lớp cha, như `RaiseAlert`, chứ không trực tiếp gọi `Invoke` trên event của lớp cha. Các đối tượng bên ngoài chỉ được đăng ký hoặc hủy đăng ký bằng `+=` và `-=`, không được tự phát event.

## Cách chạy

Cài .NET SDK 8, mở thư mục bài tập trong VS Code và chạy lệnh sau trong Terminal:

```sh
dotnet run --project Assignment8-MI4090.csproj
```

Mã nguồn nằm trong `Program.cs`. Chương trình gồm 9 ca kiểm tra, hiển thị kết quả `PASS` hoặc `FAIL` cho từng ca.
