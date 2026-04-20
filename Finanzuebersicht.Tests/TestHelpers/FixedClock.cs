using System;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Tests.TestHelpers
{
    /// <summary>
    /// Deterministic clock for tests. Allows setting/advancing time.
    /// </summary>
    public class FixedClock : IClock
    {
        private DateTime now;

        public FixedClock(DateTime now)
        {
            this.now = now;
        }

        public DateTime Now => now;

        public DateTime UtcNow => now.ToUniversalTime();

        public DateTime Today => now.Date;

        /// <summary>
        /// Advance the fixed time by the given timespan.
        /// </summary>
        public void Advance(TimeSpan span) => now = now.Add(span);

        /// <summary>
        /// Replace the current fixed time.
        /// </summary>
        public void Set(DateTime newNow) => now = newNow;
    }
}
