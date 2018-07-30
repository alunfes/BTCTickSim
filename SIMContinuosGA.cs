using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTCTickSim
{
    class SIMContinuosGA
    {
        public Account startContrarianSashine(int from, int to, int num_chrom, int num_generation, bool write_result)
        {
            Account ac = new Account();
            int last_str_changed_ind = 0;
            Chrome chro = new Chrome();
            chro = getOptStrategy(from - 1000000, from - 1, num_chrom, num_generation);
            SIM sim = new SIM();
            Account ac_best = sim.startContrarianSashine(from - 1000000, from - 1, chro.Gene_exit_time_sec, chro.Gene_kairi_term, chro.Gene_entry_kairi, chro.Gene_rikaku_percentage, false);
            int num_recalc = 0;

            for (int i = from; i < to; i++)
            {
                var tdd = Strategy.contrarianSashine(ac, i, chro.Gene_exit_time_sec, chro.Gene_kairi_term, chro.Gene_entry_kairi, chro.Gene_rikaku_percentage);
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
                Form1.Form1Instance.addListBox2(TickData.time[i]+":total pl="+ac.total_pl_log.Values.ToList()[ac.total_pl_log.Count-1]);

                if (checkUpdateStrategy(i,last_str_changed_ind, from, ac, ac_best))
                {
                    num_recalc++;
                    last_str_changed_ind = i;
                    chro = getOptStrategy(i - 1000000, i, num_chrom, num_generation);
                    sim = new SIM();
                    Form1.Form1Instance.addListBox2("recalc=" + num_recalc);
                    ac_best = sim.startContrarianSashine(i - 1000000, i, chro.Gene_exit_time_sec, chro.Gene_kairi_term, chro.Gene_entry_kairi, chro.Gene_rikaku_percentage, false);
                    ac.takeActionLog(i, "applied new strategy");
                }
            }
            ac.lastDayOperation(to, write_result);
            return ac;
        }

        private bool checkUpdateStrategy(int i, int last_str_ind, int from, Account ac, Account ac_best)
        {
            bool res = false;
            if (i - last_str_ind -from> 15000 || ac.num_trade > 0)
            {
                //var expected_num_trade = Convert.ToDouble(ac_best.num_trade / (ac_best.end_ind - ac_best.start_ind)) * (i - last_str_ind);
                if (ac_best.pl_per_min * 0.5 >= ac.pl_per_min)
                {
                    Form1.Form1Instance.setLabel3("i:" + i.ToString() + " recalc because of pl per min" + ", pl_per_min=" + ac.pl_per_min.ToString());
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
