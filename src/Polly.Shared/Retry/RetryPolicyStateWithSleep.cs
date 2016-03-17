using System;
using System.Collections.Generic;
using Polly.Utilities;

namespace Polly.Retry
{
    internal partial class RetryPolicyStateWithSleep : IRetryPolicyState
    {
        private int _errorCount;
        private readonly Action<Exception, TimeSpan, Context, int> _onRetry;
        private readonly Context _context;
        private readonly IEnumerator<TimeSpan> _sleepDurationsEnumerator;

        public RetryPolicyStateWithSleep(IEnumerable<TimeSpan> sleepDurations, Action<Exception, TimeSpan, Context, int> onRetry, Context context)
        {
            _onRetry = onRetry;
            _context = context;
            _sleepDurationsEnumerator = sleepDurations.GetEnumerator();
        }

        public RetryPolicyStateWithSleep(IEnumerable<TimeSpan> sleepDurations, Action<Exception, TimeSpan, Context> onRetry, Context context) :
            this(sleepDurations, (exception, span, nestedContext, retryCount) => onRetry(exception, span, nestedContext), context)
        {
        }

        public RetryPolicyStateWithSleep(IEnumerable<TimeSpan> sleepDurations, Action<Exception, TimeSpan, int> onRetry) :
            this(sleepDurations, (exception, span, context, retryCount) => onRetry(exception, span, retryCount), null)
        {
        }

        public RetryPolicyStateWithSleep(IEnumerable<TimeSpan> sleepDurations, Action<Exception, TimeSpan> onRetry) :
            this(sleepDurations, (exception, span, context) => onRetry(exception, span), null)
        {
        }

        public bool CanRetry(Exception ex)
        {
            if (!_sleepDurationsEnumerator.MoveNext()) return false;

            if (_errorCount < int.MaxValue)
            {
                _errorCount += 1;
            }           

            var currentTimeSpan = _sleepDurationsEnumerator.Current;
            _onRetry(ex, currentTimeSpan, _context, _errorCount);
                
            SystemClock.Sleep(currentTimeSpan);

            return true;
        }
    }
}