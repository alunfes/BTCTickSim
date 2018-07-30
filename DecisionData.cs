using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTCTickSim
{
    class DecisionData
    {
        public string position;//Long, Short, Cancel, Cancel_All, Cancel_PriceTracingOrder, Exit_All
        public int cancel_index;
        public bool price_tracing_order;
        public double price;
        public double lot;

        public DecisionData()
        {
            position = "";
            cancel_index = -1;
            price_tracing_order = false;
            price = 0;
            lot = 0;
        }
    }
}
