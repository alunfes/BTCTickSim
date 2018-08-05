using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTCTickSim
{
    class VerifyChrome
    {
        public Dictionary<string, double> verifyBestChromeFromFile()
        {
            Dictionary<string, double> res = new Dictionary<string, double>();
            Chrome chro = new Chrome();
            chro.readBestChromFile();

            SIM sim_ga = new SIM();
            var ac_ga = sim_ga.startContrarianSashine(chro.start_ind, chro.end_ind, chro.Gene_exit_time_sec, chro.Gene_kairi_term, chro.Gene_entry_kairi, chro.Gene_rikaku_percentage, false);

            SIM sim = new SIM();
            var ac = sim.startContrarianSashine(chro.end_ind, chro.end_ind + 180000, chro.Gene_exit_time_sec, chro.Gene_kairi_term, chro.Gene_entry_kairi, chro.Gene_rikaku_percentage, false);

            res.Add("pl_per_min", ac.pl_per_min / ac_ga.pl_per_min);
            res.Add("profit_factor", ac.profit_factor / ac_ga.profit_factor);
            res.Add("num_trade_per_hour", ac.num_trade_per_hour / ac_ga.num_trade_per_hour);
            res.Add("total performance", res.Values.ToList().Sum());

            return res;
        }


        public Dictionary<string, double> verifyBestChrome(Chrome chro, int ga_from, int ga_to)
        {
            Dictionary<string, double> res = new Dictionary<string, double>();
            
            SIM sim_ga = new SIM();
            var ac_ga = sim_ga.startContrarianSashine(ga_from, ga_to, chro.Gene_exit_time_sec, chro.Gene_kairi_term, chro.Gene_entry_kairi, chro.Gene_rikaku_percentage, false);

            SIM sim = new SIM();
            var ac = sim.startContrarianSashine(ga_to, ga_to + 180000, chro.Gene_exit_time_sec, chro.Gene_kairi_term, chro.Gene_entry_kairi, chro.Gene_rikaku_percentage, false);

            res.Add("pl_per_min", ac.pl_per_min / ac_ga.pl_per_min);
            res.Add("profit_factor", ac.profit_factor / ac_ga.profit_factor);
            res.Add("num_trade_per_hour", ac.num_trade_per_hour / ac_ga.num_trade_per_hour);
            res.Add("total performance", res.Values.ToList().Sum());

            return res;
        }
    }
}
