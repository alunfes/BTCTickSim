using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTCTickSim
{
    class DecisionData
    {
        public string position;
        public double price;
        public double lot;

        public DecisionData()
        {
            position = "None";
            price = 0;
            lot = 0;
        }
    }
}
