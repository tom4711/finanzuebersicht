using System;

namespace Finanzuebersicht.Core.Services
{
    public class SystemClock : IClock
    {
        public DateTime Now => DateTime.Now;
        public DateTime Today => DateTime.Today;
        public DateTime UtcNow => DateTime.UtcNow;

        // Global instance for code that hasn't been DI-updated yet.
        public static IClock Instance { get; set; } = new SystemClock();
    }
}
