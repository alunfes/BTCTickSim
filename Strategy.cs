using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTCTickSim
{
    static class Strategy
    {
        /*************************************************************************************/
        /*************************************************************************************/
        /*************************************************************************************/
        /*************************************************************************************/
        public static DecisionData contrarianSashine(Account ac, int i, double exit_time_sec, double cancel_time_sec, int kairi_term, double entry_kairi, double plus_rikaku, double minus_rikaku)
        {
            DecisionData dd = new DecisionData();
            double max_lot = 1;
            
            double kairi = TickData.price[i] - TickData.price[i - kairi_term];
            if (Math.Abs(kairi) >= entry_kairi)
            {
                dd.position = (kairi > 0) ? "Short" : "Long";
                if (dd.position == "Short" && ac.unexe_position.Contains("Long"))
                    dd.position = "Cancel";
                else if (dd.position == "Long" && ac.unexe_position.Contains("Short"))
                    dd.position = "Cancel";
                else
                {
                    dd.lot = 0.01;
                    dd.price = TickData.price[i];
                }
            }
            
            return dd;
        }
    }
}
