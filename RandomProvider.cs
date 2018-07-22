using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTCTickSim
{
    class RandomProvider
    {
        private static int seed;
        public static Random getRandom()
        {
            var s = Environment.TickCount;
            Random r = new Random(s + seed++);
            return r;
        }
    }
}
