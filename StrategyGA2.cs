using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTCTickSim
{
    class StrategyGA2
    {
        public static DecisionData2 contrarianSashine(AccountGA2 ac, int i, Chrome2 chro, DecisionData2 pre_dd)
        {
            DecisionData2 dd = new DecisionData2();

            double kairi = (TickData.price[i] - TickData.makairi_500[i]) / TickData.makairi_500[i];

            var entry_signs = new int[chro.num_box]; //1:Long, -1:Short
            string entry_sign = "";
            int selected_box = -1;
            for (int j = 0; j < chro.num_box; j++)
            {
                if (chro.gene_ceil_vola[j] >= TickData.vola_500[i] && chro.gene_floor_vola[j] <= TickData.vola_500[i] &&
                    chro.gene_ceil_avevol[j] >= TickData.ave_vol_500[i] && chro.gene_floor_avevol[j] <= TickData.ave_vol_500[i])
                {
                    if (kairi >= chro.gene_entry_kairi[j])
                        entry_signs[j] = (chro.gene_kirikae[j]) ? -1 : 1;
                    else if (kairi <= chro.gene_entry_kairi[j])
                        entry_signs[j] = (chro.gene_kirikae[j]) ? 1 : -1;
                }
            }
            

            bool flg = true;
            string s = "";
            if (pre_dd.fired_box_ind >= 0)
            {
                if (entry_signs[pre_dd.fired_box_ind] == 1)
                    s = "Long";
                else if (entry_signs[pre_dd.fired_box_ind] == -1)
                    s = "Short";

                if (s == pre_dd.position)
                {
                    selected_box = pre_dd.fired_box_ind;
                    entry_sign = s;
                    flg = false;
                }
            }
            if (flg)
            {
                var r = RandomProvider.getRandom();
                if (entry_signs.Sum() > 0)
                {
                    entry_sign = "Long";
                    var selections = entry_signs.Select((p, ind) => new { Content = p, Index = ind })
                        .Where(ano => ano.Content == 1)
                        .Select(ano => ano.Index).ToList();
                    selected_box = selections[r.Next(0, selections.Count)];
                }
                else if (entry_signs.Sum() < 0)
                {
                    entry_sign = "Short";
                    var selections = entry_signs.Select((p, ind) => new { Content = p, Index = ind })
                        .Where(ano => ano.Content == -1)
                        .Select(ano => ano.Index).ToList();
                    selected_box = selections[r.Next(0, selections.Count)];
                }
                if (selected_box >= 0)
                    entry_sign = (entry_signs[selected_box] == 1) ? "Long" : "Short";
            }



            if (entry_sign != "")
            {
                if (ac.holding_position == "None" && ac.price_tracing_order_flg == false)
                {
                    dd = makeDDForEntryPriceTracingOrder(i, entry_sign, true, 0.05, selected_box);
                }
                else if (ac.holding_position != "None" && entry_sign != ac.holding_position && ac.price_tracing_order_flg && ac.cancel_all_orders == false)
                {
                    dd = makeDDForEntryPriceTracingOrder(i, "Cancel_PriceTracingOrder", false, 0, selected_box);
                }
                else if (ac.holding_position != "None" && entry_sign != ac.holding_position && ac.price_tracing_order_flg == false && ac.cancel_all_orders == false)
                {
                    dd = makeDDForEntryPriceTracingOrder(i, entry_sign, true, ac.ave_holding_lot + 0.05, selected_box);
                }
                else if (entry_sign == ac.holding_position && ac.unexe_position.Count == 0)
                {
                    dd.position = (entry_sign == "Long") ? "Short" : "Long";
                    dd.cancel_index = -1;
                    dd.price_tracing_order = false;
                    dd.price = (ac.holding_position == "Long") ? Math.Round(ac.ave_holding_price * (1 + chro.gene_rikaku_percentage[selected_box])) : Math.Round(ac.ave_holding_price * (1 - chro.gene_rikaku_percentage[selected_box]));
                    dd.lot = ac.ave_holding_lot;
                }
                else if ((TickData.time[i] - ac.last_entry_time).TotalSeconds >= chro.gene_exit_time_sec[selected_box])
                {
                    dd = makeDDForEntryPriceTracingOrder(i, (ac.holding_position == "Long") ? "Short" : "Long", true, ac.ave_holding_lot, selected_box);
                }
                /*else if (entry_sign == ac.holding_position && ac.unexe_position.Count == 0 && (TickData.time[i] - ac.last_entry_time).TotalSeconds >= chro.gene_exit_time_sec[selected_box])
                {
                    dd = makeDDForEntryPriceTracingOrder(i, (ac.holding_position == "Long") ? "Short" : "Long", true, ac.ave_holding_lot, selected_box);
                }*/
            }
            else if (ac.holding_position != "None")
            {
                // dd.position = "Exit_All";
            }

            //if(dd.fired_box_ind!=-1) chro.box_fired_num[dd.fired_box_ind]++;   ->Count in SIM class

            return dd;
        }

        private static DecisionData2 makeDDForEntryPriceTracingOrder(int i, string order_position, bool flg, double lot, int fired_ind)
        {
            DecisionData2 dd = new DecisionData2();
            dd.position = order_position;
            dd.cancel_index = -1;
            dd.price_tracing_order = flg;
            dd.price = 0;
            dd.lot = lot;
            dd.fired_box_ind = fired_ind;
            return dd;
        }
    }
}
