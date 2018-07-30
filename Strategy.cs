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
        public static DecisionData contrarianSashine(Account ac, int i, double exit_time_sec, int kairi_term, double entry_kairi, double rikaku_percentage)
        {
            DecisionData dd = new DecisionData();
            
            double kairi = (TickData.price[i] - TickData.price[i - kairi_term])/TickData.price[i-kairi_term];

            string entry_sign = "";
            if (kairi >= entry_kairi)
                entry_sign = "Short";
            else if (kairi <= -entry_kairi)
                entry_sign = "Long";

            if(entry_sign !="")
            {
                if (ac.holding_position == "None" && ac.price_tracing_order_flg == false)
                {
                    dd = makeDDForEntryPriceTracingOrder(i, entry_sign, true, 0.05);
                }
                else if (ac.holding_position != "None" && entry_sign != ac.holding_position && ac.price_tracing_order_flg && ac.cancel_all_orders == false)
                {
                    dd = makeDDForEntryPriceTracingOrder(i, "Cancel_PriceTracingOrder", false, 0);
                }
                else if (ac.holding_position != "None" && entry_sign != ac.holding_position && ac.price_tracing_order_flg == false && ac.cancel_all_orders == false)
                {
                    dd = makeDDForEntryPriceTracingOrder(i, entry_sign, true, ac.ave_holding_lot + 0.05);
                }
                else if(entry_sign == ac.holding_position && ac.unexe_position.Count == 0)
                {
                    dd.position = (entry_sign == "Long") ? "Short" : "Long";
                    dd.cancel_index = -1;
                    dd.price_tracing_order = false;
                    dd.price = (ac.holding_position == "Long") ? Math.Round(ac.ave_holding_price * (1 + rikaku_percentage)) : Math.Round(ac.ave_holding_price * (1 - rikaku_percentage));
                    dd.lot = ac.ave_holding_lot;
                }
                else if(entry_sign == ac.holding_position && ac.unexe_position.Count == 0 && (TickData.time[i] - ac.last_entry_time).TotalSeconds >= exit_time_sec)
                {
                    dd = makeDDForEntryPriceTracingOrder(i, (ac.holding_position == "Long") ? "Short" : "Long", true, ac.ave_holding_lot);
                }
            }
            else if (ac.holding_position != "None")
            {
                dd.position = "Exit_All";
            }

            return dd;
        }

        private static DecisionData makeDDForEntryPriceTracingOrder(int i, string order_position, bool flg, double lot)
        {
            DecisionData dd = new DecisionData();
            dd.position = order_position;
            dd.cancel_index = -1;
            dd.price_tracing_order = flg;
            dd.price = 0;
            dd.lot = lot;
            return dd;
        }
    }
}
