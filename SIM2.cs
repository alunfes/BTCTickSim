using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTCTickSim
{
    class SIM2
    {

        public Account2 startContrarianTrendFollowSashine(int from, int to, Chrome2 chro, bool no_trade_non_fired_box, bool writelog)
        {
            var ac = new Account2(chro);
            var pre_dd = new DecisionData2();
            for (int i = from; i < to; i++)
            {
                var tdd = Strategy2.contrarianSashine(ac, i, chro, pre_dd, no_trade_non_fired_box);
                if(tdd.fired_box_ind != -1)
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
                ac.moveToNext(i, tdd.fired_box_ind);
            }
            ac.lastDayOperation(to, chro, pre_dd.fired_box_ind, writelog);
            return ac;
        }
    }
}
