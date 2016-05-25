using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace soa
{
    public abstract class ThreadWorker
    {
        private readonly Thread thread;
        protected readonly CancellationTokenSource cancelled;

        public ThreadWorker()
        {
            this.thread = new Thread(doWork);
            this.cancelled = new CancellationTokenSource();
        }

        protected abstract void doWork();

        public bool isAlive()
        {
            return !cancelled.IsCancellationRequested;
        }

        public void Start()
        {
            thread.Start();
        }

        public void Stop()
        {
            cancelled.Cancel();
            thread.Join();
        }
    }
}
