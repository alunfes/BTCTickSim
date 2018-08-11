using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTCTickSim
{
    class SIMContinuousGA2
    {
        public Account startContrarianSashine(int from, int to, int num_chrom, int num_generation, int search_period, int pl_check_period, bool write_result)
        {
            Account ac = new Account();
            int last_str_changed_ind = from;
            int last_str_num_trade = 0;
            Chrome chro = new Chrome();
            chro = getOptStrategy(from - search_period, from - 1, num_chrom, num_generation);
            SIM sim = new SIM();
            Account ac_best = sim.startContrarianSashine(from - search_period, from - 1, chro.Gene_exit_time_sec, chro.Gene_kairi_term, chro.Gene_entry_kairi, chro.Gene_rikaku_percentage, false);
            int num_recalc = 0;

            for (int i = from; i < to; i++)
            {
                var tdd = Strategy.contrarianSashine(ac, i, chro.Gene_exit_time_sec, chro.Gene_kairi_term, chro.Gene_entry_kairi, chro.Gene_rikaku_percentage);
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
                if (checkUpdateStrategy(i, last_str_changed_ind, from, pl_check_period, last_str_num_trade, ac, ac_best))
                {
                    ac.cancelAllOrders(i);
                    num_recalc++;
                    last_str_changed_ind = i;
                    last_str_num_trade = ac.num_trade;
                    chro = getOptStrategy(i - search_period, i, num_chrom, num_generation);
                    sim = new SIM();
                    Form1.Form1Instance.addListBox2("recalc=" + num_recalc);
                    ac_best = sim.startContrarianSashine(i - search_period, i, chro.Gene_exit_time_sec, chro.Gene_kairi_term, chro.Gene_entry_kairi, chro.Gene_rikaku_percentage, false);
                    ac.takeActionLog(i, "applied new strategy:num trade=" + ac_best.num_trade.ToString() + " :pl per min=" + ac_best.pl_per_min.ToString() + " :win rate=" + ac_best.win_rate.ToString());
                }
                ac.moveToNext(i);
                Form1.Form1Instance.addListBox2(TickData.time[i] + ":total pl=" + Math.Round(ac.total_pl_log.Values.ToList()[ac.total_pl_log.Count - 1], 2) + " :num trade=" + ac.num_trade.ToString());
            }
            ac.lastDayOperation(to, write_result);
            return ac;
        }

        private bool checkUpdateStrategy(int i, int last_str_ind, int from, int pl_check_perid, int last_str_num_trade, Account ac, Account ac_best)
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

        private Chrome getOptStrategy(int from, int to, int num_chrom, int num_generation)
        {
            Form1.Form1Instance.setLabel2("Calculating GA");
            GA ga = new GA();
            return ga.startGA(num_chrom, num_generation, from, to, false);
        }
    }
}
