using System;

namespace Finanzuebersicht.Services
{
    public interface IClock
    {
        DateTime Now { get; }
        DateTime Today { get; }
        DateTime UtcNow { get; }
    }
}
