using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace soa
{
    abstract class BeliefQueueWorker
    {
        private Queue<Belief> beliefs;
        private bool alive;
        private Thread thread;

        public BeliefQueueWorker()
        {
            this.beliefs = new Queue<Belief>();
            this.thread = new Thread(run);
            this.alive = false;
        }

        protected abstract void run();

        public bool IsAlive()
        {
            return alive;
        }

        public void Start()
        {
            this.alive = true;
            thread.Start();
        }

        public void Stop()
        {
            lock (beliefs)
            {
                alive = false;
                Monitor.PulseAll(beliefs);
            }

            thread.Join();
        }

        protected void addBelief(Belief belief)
        {
            lock (beliefs)
            {
                beliefs.Enqueue(belief);
                Monitor.PulseAll(beliefs);
            }
        }

        protected Belief popBelief()
        {
            lock (beliefs)
            {
                while (alive && beliefs.Count() == 0)
                {
                    Monitor.Wait(beliefs);
                }

                if (!alive)
                {
                    return null;
                }

                return beliefs.Dequeue();
            }
        }
    }
}
