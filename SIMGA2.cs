﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTCTickSim
{
    class SIMGA2
    {
        public AccountGA2 startContrarianTrendFollowSashine(int from ,int to, Chrome2 chro)
        {
            var ac = new AccountGA2(chro);
            var pre_dd = new DecisionData2();
            for(int i=from; i<to; i++)
            {
                var tdd = StrategyGA2.contrarianSashine(ac, i, chro, pre_dd);
                pre_dd = tdd;

                if(tdd.position == "Exit_All")
                {
                    ac.exitAllOrder(i);
                }
                else if(tdd.price_tracing_order)
                {
                    if(tdd.position == "Long" || tdd.position == "Short")
                    {
                        ac.entryPriceTracingOrder(i, tdd.position, tdd.lot);
                    }
                    else if(tdd.position == "Cancel_All")
                    {
                        ac.cancelAllOrders(i);
                    }
                    else if(tdd.position == "Cancel_PriceTracingOrder")
                    {
                        ac.cancelPriceTracingOrder(i);
                    }
                    else if(tdd.position == "Long" || tdd.position == "Short")
                    {
                        ac.entryOrder(i, tdd.position, tdd.price, tdd.lot);
                    }
                }
                ac.moveToNext(i, tdd.fired_box_ind);
            }
            ac.lastDayOperation(to, chro, pre_dd.fired_box_ind);
            return ac;
        }
    }
}
