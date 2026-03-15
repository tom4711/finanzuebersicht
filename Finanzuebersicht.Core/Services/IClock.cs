using System;

namespace Finanzuebersicht.Core.Services
{
    public interface IClock
    {
        DateTime Now { get; }
        DateTime Today { get; }
        DateTime UtcNow { get; }
    }
}
