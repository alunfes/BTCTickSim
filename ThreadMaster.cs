using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace BTCTickSim
{
    class ThreadMaster
    {
        public void start()
        {
            Thread th = new Thread(doSIm);
            th.Start();
        }

        public void startGA()
        {
            Thread th = new Thread(doGA);
            th.Start();
        }

        private void doGA()
        {
            TickData.readTickData();
            GA ga = new GA();
            ga.startGA(100, 100, TickData.time.Count - 50000, TickData.time.Count - 100);
        }

        private void doSIm()
        {
            TickData.readTickData();

            SIM s = new SIM();
            s.startContrarianSashine(TickData.price.Count-10000, TickData.price.Count-100, 30, 60, 0.001, 0.0005, true);
        }
    }
}
