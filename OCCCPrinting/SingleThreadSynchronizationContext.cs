using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OCCCPrinting
{
    //https://blogs.msdn.microsoft.com/pfxteam/2012/01/20/await-synchronizationcontext-and-console-apps/
    //https://stackoverflow.com/questions/40249169/async-await-and-threading/40249260
    public sealed class SingleThreadSynchronizationContext : SynchronizationContext
    {

        private readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object>>
            m_queue = new BlockingCollection<KeyValuePair<SendOrPostCallback, object>>();
        public override void Post(SendOrPostCallback d, object state)
        {
            m_queue.Add(new KeyValuePair<SendOrPostCallback, object>(d, state));
        }

        public void RunOnCurrentThread()
        {
            KeyValuePair<SendOrPostCallback, object> workItem;
            while (m_queue.TryTake(out workItem, Timeout.Infinite))
                workItem.Key(workItem.Value);
        }

        public void Complete()
        {
            m_queue.CompleteAdding();
        }
    }
}
