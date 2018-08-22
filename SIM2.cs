using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTCTickSim
{
    class SIM2
    {
        public AccountGA2 startContrarianTrendFollowSashine(int from, int to, Chrome2 chro)
        {
            var ac = new AccountGA2(chro.num_box);
            var pre_dd = new DecisionData2();
            for (int i = from; i < to; i++)
            {
                var tdd = StrategyGA2.contrarianSashine(ac, i, chro, pre_dd);
                if(tdd.fired_box_ind>=0)
                    chro.box_fired_num[tdd.fired_box_ind]++;
                pre_dd = tdd;

                if (tdd.position == "Exit_All")
                {
                    ac.exitAllOrder(i);
                }
                else if (tdd.price_tracing_order)
                {
                    if (tdd.position == "Long" || tdd.position == "Short")
                    {
                        ac.entryPriceTracingOrder(i, tdd.position, tdd.lot);
                    }
                    else if (tdd.position == "Cancel_All")
                    {
                        ac.cancelAllOrders(i);
                    }
                    else if (tdd.position == "Cancel_PriceTracingOrder")
                    {
                        ac.cancelPriceTracingOrder(i);
                    }
                    else if (tdd.position == "Long" || tdd.position == "Short")
                    {
                        ac.entryOrder(i, tdd.position, tdd.price, tdd.lot);
                    }
                }
                ac.moveToNext(i);
            }
            ac.lastDayOperation(to, chro);
            return ac;
        }
    }
}
