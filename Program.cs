/****************/
// 202418944
// Bùi Nhật Minh
/****************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assignment8_MI4090;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        // TC1: Đổi trạng thái và ghi audit.
        try
        {
            var device = new ProjectorRemote("PR001", "Máy chiếu Casio");
            var logger = new DeviceAuditLogger();
            device.StatusChanged += logger.HandleStatusChanged;

            device.ChangeStatus(DeviceStatus.Borrowed);

            if (device.Status != DeviceStatus.Borrowed || logger.Entries.Count != 1)
                throw new Exception("Trạng thái hoặc audit không đúng.");

            Console.WriteLine("PASS TC1");
        }
        catch (Exception ex)
        {
            Environment.ExitCode = 1;
            Console.WriteLine($"FAIL TC1: {ex.Message}");
        }

        // TC2: Giữ nguyên trạng thái không phát event.
        try
        {
            var device = new ProjectorRemote("PR001", "Máy chiếu Casio");
            int count = 0;
            device.StatusChanged += (sender, e) => count++;

            device.ChangeStatus(DeviceStatus.Available);

            if (count != 0)
                throw new Exception("Phát event dù trạng thái không đổi.");

            Console.WriteLine("PASS TC2");
        }
        catch (Exception ex)
        {
            Environment.ExitCode = 1;
            Console.WriteLine($"FAIL TC2: {ex.Message}");
        }

        // TC3: Pin máy chiếu cảnh báo đúng, không lặp.
        try
        {
            var device = new ProjectorRemote("PR001", "Máy chiếu Casio");
            var alerts = new List<AlertSeverity>();
            device.AlertRaised += (sender, e) => alerts.Add(e.Severity);

            device.UpdateBatteryLevel(19);
            device.UpdateBatteryLevel(18);
            device.UpdateBatteryLevel(0);
            device.UpdateBatteryLevel(0);

            if (!alerts.SequenceEqual(new[]
                { AlertSeverity.Warning, AlertSeverity.Critical }))
                throw new Exception("Sai mức độ hoặc số lần cảnh báo.");

            Console.WriteLine("PASS TC3");
        }
        catch (Exception ex)
        {
            Environment.ExitCode = 1;
            Console.WriteLine($"FAIL TC3: {ex.Message}");
        }

        // TC4: Pin và tín hiệu micro.
        try
        {
            var device = new WirelessMicrophone("WM001", "Micro không dây");
            var alerts = new List<AlertSeverity>();
            device.AlertRaised += (sender, e) => alerts.Add(e.Severity);

            device.UpdateBatteryLevel(29);
            device.UpdateSignalQuality(39);
            device.UpdateSignalQuality(0);

            if (!alerts.SequenceEqual(new[]
                {
                    AlertSeverity.Warning,
                    AlertSeverity.Warning,
                    AlertSeverity.Critical
                }))
                throw new Exception("Cảnh báo micro không đúng.");

            Console.WriteLine("PASS TC4");
        }
        catch (Exception ex)
        {
            Environment.ExitCode = 1;
            Console.WriteLine($"FAIL TC4: {ex.Message}");
        }

        // TC5: Điều hòa không cảnh báo lặp khi vẫn mất phản hồi.
        try
        {
            var device = new AirConditionerRemote("AC001", "Điều hòa Daikin", "D5-102");
            var alerts = new List<AlertSeverity>();
            device.AlertRaised += (sender, e) => alerts.Add(e.Severity);

            device.UpdateResponseState(false);
            device.UpdateResponseState(false);
            device.UpdateResponseState(true);

            if (!device.IsResponsive
                || !alerts.SequenceEqual(new[] { AlertSeverity.Critical }))
                throw new Exception("Phải phục hồi và chỉ cảnh báo một lần.");

            Console.WriteLine("PASS TC5");
        }
        catch (Exception ex)
        {
            Environment.ExitCode = 1;
            Console.WriteLine($"FAIL TC5: {ex.Message}");
        }

        // TC6: Pin ngoài khoảng phải ném đúng ngoại lệ.
        try
        {
            var device = new ProjectorRemote("PR001", "Máy chiếu Casio");
            bool caught = false;

            try
            {
                device.UpdateBatteryLevel(-1);
            }
            catch (InvalidDeviceReadingException ex)
            {
                caught = ex.DeviceId == "PR001" && ex.InvalidValue == -1;
            }

            if (!caught || device.BatteryLevel != 100)
                throw new Exception("Thiếu ngoại lệ đúng hoặc pin đã bị đổi.");

            Console.WriteLine("PASS TC6");
        }
        catch (Exception ex)
        {
            Environment.ExitCode = 1;
            Console.WriteLine($"FAIL TC6: {ex.Message}");
        }

        // TC7: Đăng ký không trùng và hủy đúng handler.
        try
        {
            var device = new ProjectorRemote("PR001", "Máy chiếu Casio");
            var maintenance = new MaintenanceService();

            using var coordinator = new DeviceMonitoringCoordinator(
                new[] { device },
                new ConsoleNotificationService(),
                new DeviceAuditLogger(),
                maintenance);

            coordinator.Subscribe();
            coordinator.Subscribe();
            device.UpdateBatteryLevel(0);

            if (maintenance.Requests.Count != 1)
                throw new Exception("Đăng ký bị thiếu hoặc trùng.");

            coordinator.Unsubscribe();
            device.UpdateBatteryLevel(100);
            device.UpdateBatteryLevel(0);

            if (maintenance.Requests.Count != 1)
                throw new Exception("Vẫn nhận cảnh báo sau khi hủy.");

            Console.WriteLine("PASS TC7");
        }
        catch (Exception ex)
        {
            Environment.ExitCode = 1;
            Console.WriteLine($"FAIL TC7: {ex.Message}");
        }
        // TC8: Delegate chỉ xử lý Critical
        try
        {
            var device = new ProjectorRemote("PR001", "Máy chiếu Casio");
            int count = 0;

            var subscriber = new FilteredAlertSubscriber(
                alert => alert.Severity == AlertSeverity.Critical,
                alert => count++);

            device.AlertRaised += subscriber.HandleAlert;

            device.UpdateBatteryLevel(19); // Warning: bỏ qua
            if (count != 0)
                throw new Exception("Bộ lọc xử lý cả Warning.");

            device.UpdateBatteryLevel(0); // Critical: xử lý
            if (count != 1)
                throw new Exception("Bộ lọc không xử lý đúng Critical.");

            Console.WriteLine("PASS TC8");
        }
        catch (Exception ex)
        {
            Environment.ExitCode = 1;
            Console.WriteLine($"FAIL TC8: {ex.Message}");
        }

        // TC9: Dispose tự hủy đăng ký
        try
        {
            var device = new ProjectorRemote("PR001", "Máy chiếu Casio");
            var logger = new DeviceAuditLogger();
            var maintenance = new MaintenanceService();

            using (var coordinator = new DeviceMonitoringCoordinator(
                new[] { device },
                new ConsoleNotificationService(),
                logger,
                maintenance))
            {
                coordinator.Subscribe();
                device.ChangeStatus(DeviceStatus.Borrowed);
                device.UpdateBatteryLevel(0);

                if (logger.Entries.Count != 1 || maintenance.Requests.Count != 1)
                    throw new Exception("Đăng ký chưa hoạt động.");
            }

            device.ChangeStatus(DeviceStatus.Available);
            device.UpdateBatteryLevel(100);
            device.UpdateBatteryLevel(0);

            if (logger.Entries.Count != 1 || maintenance.Requests.Count != 1)
                throw new Exception("Sau Dispose vẫn ghi log hoặc tạo bảo trì.");

            Console.WriteLine("PASS TC9");
        }
        catch (Exception ex)
        {
            Environment.ExitCode = 1;
            Console.WriteLine($"FAIL TC9: {ex.Message}");
        }
    }
}

// Danh mục trạng thái và mức độ cảnh báo.
public enum DeviceStatus
{
    Available,
    Borrowed,
    UnderMaintenance
}

public enum AlertSeverity
{
    Information,
    Warning,
    Critical
}

public class DeviceStatusChangedEventArgs : EventArgs
{
    public string DeviceId { get; }
    public DeviceStatus OldStatus { get; }
    public DeviceStatus NewStatus { get; }
    public DateTime ChangedAt { get; }

    public DeviceStatusChangedEventArgs(string deviceId, DeviceStatus oldStatus,
        DeviceStatus newStatus, DateTime changedAt)
    {
        DeviceId = deviceId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        ChangedAt = changedAt;
    }
}

public class DeviceAlertEventArgs : EventArgs
{
    public string DeviceId { get; }
    public string Message { get; }
    public AlertSeverity Severity { get; }
    public DateTime OccurredAt { get; }

    public DeviceAlertEventArgs(string deviceId, string message,
        AlertSeverity severity, DateTime occurredAt)
    {
        DeviceId = deviceId;
        Message = message;
        Severity = severity;
        OccurredAt = occurredAt;
    }
}

public class InvalidDeviceReadingException : Exception
{
    public string DeviceId { get; }
    public int InvalidValue { get; }

    public InvalidDeviceReadingException(string deviceId, int invalidValue)
        : base($"Thiết bị {deviceId}: giá trị {invalidValue} phải nằm trong khoảng 0–100.")
    {
        DeviceId = deviceId;
        InvalidValue = invalidValue;
    }
}

// Publisher chỉ phát sự kiện, không phụ thuộc vào các dịch vụ xử lý.
public abstract class TeachingDevice
{
    public string Id { get; }
    public string Name { get; }
    public DeviceStatus Status { get; private set; }

    public event EventHandler<DeviceStatusChangedEventArgs>? StatusChanged;
    public event EventHandler<DeviceAlertEventArgs>? AlertRaised;

    protected TeachingDevice(string id, string name)
    {
        Id = id;
        Name = name;
        Status = DeviceStatus.Available;
    }

    public void ChangeStatus(DeviceStatus newStatus)
    {
        if (newStatus == Status)
            return;

        DeviceStatus oldStatus = Status;
        Status = newStatus;
        OnStatusChanged(new DeviceStatusChangedEventArgs(
            Id, oldStatus, newStatus, DateTime.Now));
    }

    protected void RaiseAlert(string message, AlertSeverity severity)
    {
        OnAlertRaised(new DeviceAlertEventArgs(Id, message, severity, DateTime.Now));
    }

    protected virtual void OnStatusChanged(DeviceStatusChangedEventArgs e)
    {
        StatusChanged?.Invoke(this, e);
    }

    protected virtual void OnAlertRaised(DeviceAlertEventArgs e)
    {
        AlertRaised?.Invoke(this, e);
    }

    protected void ValidateReading(int value)
    {
        if (value < 0 || value > 100)
            throw new InvalidDeviceReadingException(Id, value);
    }

    // 0: Critical; dưới ngưỡng: Warning; còn lại: Information (vùng an toàn).
    protected AlertSeverity GetReadingSeverity(int value, int warningThreshold)
    {
        if (value == 0)
            return AlertSeverity.Critical;
        if (value < warningThreshold)
            return AlertSeverity.Warning;
        return AlertSeverity.Information;
    }
}

public class ProjectorRemote : TeachingDevice
{
    public int BatteryLevel { get; private set; } = 100;

    public ProjectorRemote(string id, string name) : base(id, name) { }

    public void UpdateBatteryLevel(int newLevel)
    {
        ValidateReading(newLevel);
        var oldSeverity = GetReadingSeverity(BatteryLevel, 20);
        var newSeverity = GetReadingSeverity(newLevel, 20);
        BatteryLevel = newLevel;

        if (newSeverity != oldSeverity && newSeverity != AlertSeverity.Information)
            RaiseAlert($"Pin máy chiếu còn {BatteryLevel}%.", newSeverity);
    }
}

public class AirConditionerRemote : TeachingDevice
{
    public string AssignedRoom { get; }
    public bool IsResponsive { get; private set; } = true;

    public AirConditionerRemote(string id, string name, string assignedRoom)
        : base(id, name)
    {
        AssignedRoom = assignedRoom;
    }

    public void UpdateResponseState(bool isResponsive)
    {
        if (isResponsive == IsResponsive)
            return;

        IsResponsive = isResponsive;
        if (!IsResponsive)
            RaiseAlert($"Điều hòa phòng {AssignedRoom} không phản hồi.", AlertSeverity.Critical);
    }
}

public class WirelessMicrophone : TeachingDevice
{
    public int BatteryLevel { get; private set; } = 100;
    public int SignalQuality { get; private set; } = 100;

    public WirelessMicrophone(string id, string name) : base(id, name) { }

    public void UpdateBatteryLevel(int newLevel)
    {
        ValidateReading(newLevel);
        var oldSeverity = GetReadingSeverity(BatteryLevel, 30);
        var newSeverity = GetReadingSeverity(newLevel, 30);
        BatteryLevel = newLevel;

        if (newSeverity != oldSeverity && newSeverity != AlertSeverity.Information)
            RaiseAlert($"Pin micro còn {BatteryLevel}%.", newSeverity);
    }

    public void UpdateSignalQuality(int newQuality)
    {
        ValidateReading(newQuality);
        var oldSeverity = GetReadingSeverity(SignalQuality, 40);
        var newSeverity = GetReadingSeverity(newQuality, 40);
        SignalQuality = newQuality;

        if (newSeverity != oldSeverity && newSeverity != AlertSeverity.Information)
            RaiseAlert($"Chất lượng tín hiệu micro còn {SignalQuality}%.", newSeverity);
    }
}

public class ConsoleNotificationService
{
    public void HandleAlert(object? sender, DeviceAlertEventArgs e)
    {
        Console.WriteLine($"[{e.Severity}] {e.DeviceId}: {e.Message}");
    }
}

public class DeviceAuditLogger
{
    private readonly List<string> entries = new();
    public IReadOnlyList<string> Entries => entries.AsReadOnly();

    public void HandleStatusChanged(object? sender, DeviceStatusChangedEventArgs e)
    {
        entries.Add($"{e.ChangedAt}: {e.DeviceId}, {e.OldStatus} -> {e.NewStatus}");
    }
}

public class MaintenanceService
{
    private readonly List<string> requests = new();
    public IReadOnlyList<string> Requests => requests.AsReadOnly();

    public void HandleCriticalAlert(object? sender, DeviceAlertEventArgs e)
    {
        if (e.Severity != AlertSeverity.Critical)
            return;

        requests.Add($"Maintenance request for {e.DeviceId}: {e.Message}");
    }
}

public delegate bool AlertFilter(DeviceAlertEventArgs alert);

public class FilteredAlertSubscriber
{
    private readonly AlertFilter filter;
    private readonly Action<DeviceAlertEventArgs> action;

    public FilteredAlertSubscriber(AlertFilter filter, Action<DeviceAlertEventArgs> action)
    {
        this.filter = filter;
        this.action = action;
    }

    public void HandleAlert(object? sender, DeviceAlertEventArgs e)
    {
        if (filter(e))
            action(e);
    }
}

public class DeviceMonitoringCoordinator : IDisposable
{
    private readonly List<TeachingDevice> devices;
    private readonly ConsoleNotificationService notificationService;
    private readonly DeviceAuditLogger auditLogger;
    private readonly MaintenanceService maintenanceService;
    private bool isSubscribed;
    private bool isDisposed;

    public DeviceMonitoringCoordinator(IEnumerable<TeachingDevice> devices,
        ConsoleNotificationService notificationService, DeviceAuditLogger auditLogger,
        MaintenanceService maintenanceService)
    {
        this.devices = devices.Distinct().ToList();
        this.notificationService = notificationService;
        this.auditLogger = auditLogger;
        this.maintenanceService = maintenanceService;
    }

    public void Subscribe()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DeviceMonitoringCoordinator));
        if (isSubscribed)
            return;

        foreach (var device in devices)
        {
            device.StatusChanged += auditLogger.HandleStatusChanged;
            device.AlertRaised += notificationService.HandleAlert;
            device.AlertRaised += maintenanceService.HandleCriticalAlert;
        }
        isSubscribed = true;
    }

    public void Unsubscribe()
    {
        if (!isSubscribed)
            return;

        foreach (var device in devices)
        {
            device.StatusChanged -= auditLogger.HandleStatusChanged;
            device.AlertRaised -= notificationService.HandleAlert;
            device.AlertRaised -= maintenanceService.HandleCriticalAlert;
        }
        isSubscribed = false;
    }

    public void Dispose()
    {
        Unsubscribe();
        isDisposed = true;
    }
}
