using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTCTickSim
{
    class SIM
    {

        public Account startContrarianSashine(int from, int to, double exit_time_sec, int kairi_term, double entry_kairi, double rikaku, bool write_result)
        {
            Account ac = new Account();
            for (int i = from; i < to; i++)
            {
                var tdd = Strategy.contrarianSashine(ac, i, exit_time_sec, kairi_term, entry_kairi, rikaku);
                if (tdd.price_tracing_order)
                {
                    if (tdd.position == "Long" || tdd.position == "Short")
                    {
                        ac.entryPriceTracingOrder(i, tdd.position, tdd.lot);
                    }
                }
                else
                {
                    if (tdd.position == "Cancel" && tdd.cancel_index >= 0)
                    {
                        ac.cancelOrder(i, tdd.cancel_index);
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
            ac.lastDayOperation(to, write_result);
            return ac;
        }
    }
}
