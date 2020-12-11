using System;
using System.Diagnostics;

namespace Common
{
    /*
   using (new DisposableStopwatch(t => Console.WriteLine("{0} elapsed", t))) {
        // do stuff that I want to measure
      }
   */
    public class DisposableStopwatch : IDisposable
    {
        private readonly Stopwatch _sw;
        private readonly Action<TimeSpan> _f;

        public DisposableStopwatch(Action<TimeSpan> f)
        {
            _f = f;
            _sw = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _sw.Stop();
            _f(_sw.Elapsed);
        }
    }
}
