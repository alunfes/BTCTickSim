using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTCTickSim
{
    class SIM
    {
        public void startContrarianSashine(int from, int to, double exit_time_sec, double cancel_time_sec, int kairi_term, double entry_kairi, double plus_rikaku, double minus_rikaku)
        {
            Account ac = new Account();
            for(int i=from; i<to; i++)
            {
                var tdd = Strategy.contrarianSashine(ac, i, exit_time_sec, cancel_time_sec, kairi_term, entry_kairi, plus_rikaku, minus_rikaku);
                if(tdd.position=="Long" || tdd.position == "Short")
                {
                    ac.entryOrder(i, tdd.position, tdd.price, tdd.lot);
                }
                else if(tdd.position == "Cancel")
                {
                    ac.cancelAllOrders(i);
                }

                ac.moveToNext(i);
                Form1.Form1Instance.setLabel("Sim i="+i.ToString());
            }
            ac.lastDayOperation(to, true);
        }
    }
}
