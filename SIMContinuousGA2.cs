using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTCTickSim
{
    class SIMContinuousGA2
    {
        public Account2 startContrarianSashine(int from, int to, int num_chrom, int num_generation, int num_islands, int elite_crossover_start, double immigration_rate, int num_box, int search_period, bool write_result)
        {
            var ac = new Account2(new Chrome2(from, to, num_box));
            int last_str_changed_ind = from;
            int last_str_num_trade = 0;
            var chro = new Chrome2(from - search_period, from-1, num_box);
            chro = getOptStrategy(from - search_period, from - 1, num_chrom, num_generation, num_box, num_islands, immigration_rate, elite_crossover_start, 30, 10, 10);
            SIM2 sim = new SIM2();
            Account2 ac_best = sim.startContrarianTrendFollowSashine(from - search_period, from - 1, chro, true, write_result);
            int num_recalc = 0;
            var pre_dd = new DecisionData2();

            for (int i = from; i < to; i++)
            {
                var tdd = Strategy2.contrarianSashine(ac, i, chro, pre_dd, true);
                if (tdd.fired_box_ind >= 0)
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
                if (checkUpdateStrategy(i, last_str_changed_ind, from, last_str_num_trade, ac, ac_best))
                {
                    ac.cancelAllOrders(i);
                    num_recalc++;
                    last_str_changed_ind = i;
                    last_str_num_trade = ac.num_trade;
                    chro = getOptStrategy(i - search_period, i, num_chrom, num_generation, num_box, num_islands, immigration_rate, elite_crossover_start, 30, 10, 10);
                    sim = new SIM2();
                    Form1.Form1Instance.addListBox2("recalc=" + num_recalc);
                    ac_best = sim.startContrarianTrendFollowSashine(i - search_period, i, chro, true, false);
                    ac.takeActionLog(i, "applied new strategy:num trade=" + ac_best.num_trade.ToString() + " :pl per min=" + ac_best.pl_per_min.ToString() + " :win rate=" + ac_best.win_rate.ToString());
                }
                ac.moveToNext(i, tdd.fired_box_ind);
                Form1.Form1Instance.addListBox2(TickData.time[i] + ":total pl=" + Math.Round(ac.total_pl_log.Values.ToList()[ac.total_pl_log.Count - 1], 2) + " :num trade=" + ac.num_trade.ToString());
            }
            ac.lastDayOperation(to, chro, pre_dd.fired_box_ind, write_result);
            return ac;
        }

        private bool checkUpdateStrategy(int i, int last_str_ind, int from, int last_str_num_trade, Account2 ac, Account2 ac_best)
        {
            bool res = false;
            double expected_num_trade = (Convert.ToDouble(ac_best.num_trade) / Convert.ToDouble(ac_best.end_ind - ac_best.start_ind)) * Convert.ToDouble(i - last_str_ind);

            if ((i - last_str_ind) >= 2500)
            {
                double current_pl_per_min = (ac.total_pl_log[i - 1] - ac.total_pl_log[last_str_ind]) / (TickData.time[i - 1] - TickData.time[last_str_ind]).TotalMinutes;
                if (ac_best.pl_per_min * 0.7 >= current_pl_per_min)
                {
                    Form1.Form1Instance.setLabel3("num:" + (i - ac.start_ind).ToString() + " recalc because of pl per min" + ", current pl_per_min=" + current_pl_per_min.ToString());
                    res = true;
                }
            }
            return res;
        }

        private Chrome2 getOptStrategy(int from, int to, int num_chrom, int num_generation, int num_box, int num_island, double immigration_rate, int elite_crossover_genera, double pl_importance, double num_trade_importance, double pl_vola_importance)
        {
            Form1.Form1Instance.setLabel2("Calculating GA");
            var ga = new GAIslandMaster2();
            return ga.startGA(from, to, num_box, num_chrom, num_generation, num_island, immigration_rate, elite_crossover_genera, pl_importance, num_island, pl_vola_importance, false);
        }
    }
}
