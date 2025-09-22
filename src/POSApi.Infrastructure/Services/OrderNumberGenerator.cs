namespace POSApi.Infrastructure.Services;

public interface IOrderNumberGenerator
{
    string GenerateOrderNumber();
}

public class OrderNumberGenerator : IOrderNumberGenerator
{
    public string GenerateOrderNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = new Random().Next(1000, 9999);
        return $"ORD-{timestamp}-{random}";
    }
}