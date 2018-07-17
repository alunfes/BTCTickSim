﻿using System;
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

        private void doSIm()
        {
            TickData.readTickData();

            SIM s = new SIM();
            s.startContrarianSashine(TickData.price.Count-10000, TickData.price.Count-1, 30, 3, 30, 1000, 500);
        }
    }
}
