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
        /*entry when kairi > entry_kairi
         * cancel all orders, stop price tracing order when opposite position entry sign was flagged
         * place rikaku order when 0.05 is executed and sign is not changed
         * exit when rikaku price was hit
         * lot is always 0.05
         * 
        /*************************************************************************************/
        public static DecisionData contrarianSashine(Account ac, int i, double exit_time_sec, double cancel_time_sec, int kairi_term, double entry_kairi, double rikaku_price)
        {
            DecisionData dd = new DecisionData();
            
            double kairi = TickData.price[i] - TickData.price[i - kairi_term];


            if()

            /***********************************/


            if (kairi >= entry_kairi)
            {
                if(ac.holding_position == "None" && ac.price_tracing_order_flg==false)
                {
                    dd.lot = 0.05;
                    dd.position = "Short";
                    dd.price_tracing_order = true;
                }
                else if (ac.holding_position == "Short" && ac.unexe_position.Count == 0) // place rikaku order
                {
                    dd.lot = 0.05;
                    dd.position = "Long";
                    dd.price = ac.ave_holding_price - rikaku_price;
                    dd.price_tracing_order = false;
                }
            }
            else if(kairi <= -entry_kairi)
            {
                if (ac.holding_position == "None" && ac.price_tracing_order_flg == false)
                {
                    dd.lot = 0.05;
                    dd.position = "Long";
                    dd.price_tracing_order = true;
                }
                else if (ac.holding_position == "Long" && ac.unexe_position.Count == 0) // place rikaku order
                {
                    dd.lot = 0.05;
                    dd.position = "Short";
                    dd.price = ac.ave_holding_price + rikaku_price;
                    dd.price_tracing_order = false;
                }
            }
            return dd;
        }
    }
}
