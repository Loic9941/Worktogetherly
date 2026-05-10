using WorkTogetherly.Application.Interfaces;

namespace WorkTogetherly.Infrastructure.Services;

public class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
