using POSApi.Domain.Common;

namespace POSApi.Domain.Entities;

public class MobileDevice : AggregateRoot
{
    public string DeviceId { get; private set; } = string.Empty;
    public string DeviceName { get; private set; } = string.Empty;
    public DeviceType Type { get; private set; }
    public string Platform { get; private set; } = string.Empty;
    public string PlatformVersion { get; private set; } = string.Empty;
    public string AppVersion { get; private set; } = string.Empty;
    public Guid? UserId { get; private set; }
    public User? User { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime LastSyncDate { get; private set; }
    public string? PushNotificationToken { get; private set; }
    public Dictionary<string, string> DeviceInfo { get; private set; } = new();
    public MobileDeviceStatus Status { get; private set; }

    private MobileDevice() { } // For EF Core

    public MobileDevice(string deviceId, string deviceName, DeviceType type, string platform, string platformVersion, string appVersion)
    {
        DeviceId = deviceId;
        DeviceName = deviceName;
        Type = type;
        Platform = platform;
        PlatformVersion = platformVersion;
        AppVersion = appVersion;
        IsActive = true;
        Status = MobileDeviceStatus.Active;
        LastSyncDate = DateTime.UtcNow;
    }

    public void RegisterUser(Guid userId)
    {
        UserId = userId;
        SetUpdatedAt();
    }

    public void UpdatePushToken(string pushToken)
    {
        PushNotificationToken = pushToken;
        SetUpdatedAt();
    }

    public void UpdateLastSync()
    {
        LastSyncDate = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void UpdateDeviceInfo(Dictionary<string, string> deviceInfo)
    {
        DeviceInfo = deviceInfo;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        IsActive = false;
        Status = MobileDeviceStatus.Inactive;
        SetUpdatedAt();
    }
}

public class MobileSession : AggregateRoot
{
    public Guid DeviceId { get; private set; }
    public MobileDevice Device { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public DateTime StartTime { get; private set; }
    public DateTime? EndTime { get; private set; }
    public string SessionToken { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public Dictionary<string, object> SessionData { get; private set; } = new();

    private MobileSession() { } // For EF Core

    public MobileSession(Guid deviceId, Guid userId, string sessionToken)
    {
        DeviceId = deviceId;
        UserId = userId;
        SessionToken = sessionToken;
        StartTime = DateTime.UtcNow;
        IsActive = true;
    }

    public void EndSession()
    {
        EndTime = DateTime.UtcNow;
        IsActive = false;
        SetUpdatedAt();
    }

    public void UpdateSessionData(string key, object value)
    {
        SessionData[key] = value;
        SetUpdatedAt();
    }

    public TimeSpan Duration => (EndTime ?? DateTime.UtcNow) - StartTime;
}

public class MobileApiLog : BaseEntity
{
    public Guid? DeviceId { get; private set; }
    public MobileDevice? Device { get; private set; }
    public Guid? UserId { get; private set; }
    public User? User { get; private set; }
    public string Endpoint { get; private set; } = string.Empty;
    public string HttpMethod { get; private set; } = string.Empty;
    public int StatusCode { get; private set; }
    public TimeSpan ResponseTime { get; private set; }
    public DateTime RequestTime { get; private set; }
    public string? ErrorMessage { get; private set; }
    public Dictionary<string, string> RequestHeaders { get; private set; } = new();
    public string? RequestBody { get; private set; }
    public string? ResponseBody { get; private set; }

    private MobileApiLog() { } // For EF Core

    public MobileApiLog(string endpoint, string httpMethod, Guid? deviceId = null, Guid? userId = null)
    {
        Endpoint = endpoint;
        HttpMethod = httpMethod;
        DeviceId = deviceId;
        UserId = userId;
        RequestTime = DateTime.UtcNow;
    }

    public void SetResponse(int statusCode, TimeSpan responseTime, string? responseBody = null, string? errorMessage = null)
    {
        StatusCode = statusCode;
        ResponseTime = responseTime;
        ResponseBody = responseBody;
        ErrorMessage = errorMessage;
    }

    public void SetRequestData(Dictionary<string, string> headers, string? requestBody = null)
    {
        RequestHeaders = headers;
        RequestBody = requestBody;
    }
}

public class OfflineTransaction : AggregateRoot
{
    public Guid DeviceId { get; private set; }
    public MobileDevice Device { get; private set; } = null!;
    public string TransactionData { get; private set; } = string.Empty;
    public DateTime CreatedOfflineAt { get; private set; }
    public DateTime? SyncedAt { get; private set; }
    public OfflineTransactionStatus Status { get; private set; }
    public string? SyncError { get; private set; }
    public int RetryCount { get; private set; }
    public Guid? OnlineTransactionId { get; private set; }

    private OfflineTransaction() { } // For EF Core

    public OfflineTransaction(Guid deviceId, string transactionData)
    {
        DeviceId = deviceId;
        TransactionData = transactionData;
        CreatedOfflineAt = DateTime.UtcNow;
        Status = OfflineTransactionStatus.Pending;
        RetryCount = 0;
    }

    public void MarkAsSynced(Guid onlineTransactionId)
    {
        Status = OfflineTransactionStatus.Synced;
        SyncedAt = DateTime.UtcNow;
        OnlineTransactionId = onlineTransactionId;
        SetUpdatedAt();
    }

    public void MarkAsFailed(string error)
    {
        Status = OfflineTransactionStatus.Failed;
        SyncError = error;
        RetryCount++;
        SetUpdatedAt();
    }

    public void Retry()
    {
        if (RetryCount < 3)
        {
            Status = OfflineTransactionStatus.Pending;
            SyncError = null;
            SetUpdatedAt();
        }
    }
}

public enum DeviceType
{
    Phone,
    Tablet,
    MobileTerminal,
    Scanner,
    Kiosk
}

public enum MobileDeviceStatus
{
    Active,
    Inactive,
    Blocked,
    Maintenance
}

public enum OfflineTransactionStatus
{
    Pending,
    Synced,
    Failed,
    Cancelled
}