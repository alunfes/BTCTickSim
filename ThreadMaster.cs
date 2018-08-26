using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace BTCTickSim
{
    class ThreadMaster
    {
        public void start()
        {
            Thread th = new Thread(doSIm);
            th.Start();
        }

        public void startGA()
        {
            Thread th = new Thread(doGA);
            th.Start();
        }

        public void startMultiGA()
        {
            Thread th = new Thread(gaForMultiplePeriod);
            th.Start();
        }

        public void startContiGASIm()
        {
            Thread th = new Thread(doContiGASim);
            th.Start();
        }

        public void startGAIsland()
        {
            Thread th = new Thread(gaIsland);
            th.Start();
        }

        public void startGAIsland2()
        {
            Thread th = new Thread(gaIsland2);
            th.Start();
        }

        private void gaForMultiplePeriod()
        {
            TickData.readTickData(0);

            int start = 5000000;
            int slide = 100000;
            int opt_term = 60000;
            int test_term = 12500;

            using (StreamWriter sw = new StreamWriter("./multi ga.csv", false, Encoding.Default))
            {
                sw.WriteLine("from,to,best pl per min,best total pl,best num trade per hour,best win rate,pl per min,total pl,win rate,total perfoamnce,pl_per_min_P,profit_factor_P,num_trade_per_hour_P");
                for (int i = 0; i < 10; i++)
                {
                    GA ga = new GA();
                    int from = TickData.time.Count - start + (i * slide);
                    int to = TickData.time.Count - start + opt_term + (i * slide);
                    Form1.Form1Instance.setLabel3("#" + i.ToString() + ":doing for " + from.ToString() + " - " + to.ToString());
                    var chro = ga.startGA(20, 50, from, to, false);
                    SIM s = new SIM();
                    var ac = s.startContrarianSashine(from, to, chro.Gene_exit_time_sec, chro.Gene_kairi_term, chro.Gene_entry_kairi, chro.Gene_rikaku_percentage, false);
                    var ac2 = s.startContrarianSashine(to, to + test_term, chro.Gene_exit_time_sec, chro.Gene_kairi_term, chro.Gene_entry_kairi, chro.Gene_rikaku_percentage, false);

                    VerifyChrome vc = new VerifyChrome();
                    var res = vc.verifyBestChrome(chro,from, to);
                    sw.WriteLine(TickData.time[from].ToString() + "," + TickData.time[to].ToString() + "," + ac.pl_per_min.ToString() + "," + ac.total_pl_log.Values.ToList()[ac.total_pl_log.Values.ToList().Count - 1].ToString() + "," + ac.num_trade.ToString() + "," + ac.win_rate.ToString() +
                        "," + ac2.pl_per_min.ToString() + "," + ac2.total_pl_log.Values.ToList()[ac2.total_pl_log.Values.ToList().Count - 1].ToString() + "," + ac2.win_rate.ToString()
                        + ","+  res["total performance"].ToString()+", "+ res["pl_per_min"].ToString()+","+res["profit_factor"].ToString()+","+res["num_trade_per_hour"]);

                    Form1.Form1Instance.addListBox2("#"+i.ToString()+", total performance:"+ res["total performance"].ToString()+ ", pl_per_min_P:" +res["pl_per_min"].ToString()+ ", profit_factor_P:"+ res["profit_factor"].ToString()
                        +", num_trade_per_hour_P:"+res["num_trade_per_hour"].ToString());
                }
            }
            Form1.Form1Instance.setLabel3("Completed all calc");
        }


        private void doGA()
        {
            TickData.readTickData(0);
            GA ga = new GA();
            ga.startGA(20, 30, TickData.time.Count - 6000000, TickData.time.Count - 4000000, true);
        }

        private void doSIm()
        {
            TickData.readTickData(0);
           // s.startContrarianSashine(TickData.price.Count - 6000000, TickData.price.Count - 5000000, chro.Gene_exit_time_sec, chro.Gene_kairi_term, chro.Gene_entry_kairi, chro.Gene_rikaku_percentage, true);

            SIM s = new SIM();
            Chrome chro = new Chrome();
            chro.readBestChromFile();
            s.startContrarianSashine(TickData.price.Count - 4000000, TickData.price.Count - 3000000, chro.Gene_exit_time_sec,chro.Gene_kairi_term, chro.Gene_entry_kairi,chro.Gene_rikaku_percentage, true);
        }

        private void doContiGASim()
        {
            TickData.readTickData(0);
            var sim = new SIMContinuousGA2();
            sim.startContrarianSashine(TickData.price.Count - 5000000, TickData.price.Count - 4800000, 10, 7, 3, 7, 0.1, 3, 200000, true);
        }

        private void gaIsland()
        {
            TickData.readTickData(0);

            GAIslandMaster gim = new GAIslandMaster();
            gim.startGA(15,100,4,0.01, 150, 10,10,10,TickData.price.Count-5000000, TickData.price.Count-4700000,false);
        }

        private void gaIsland2()
        {
            TickData.readTickData(0);

            using (StreamWriter sw = new StreamWriter("./multi ga.csv", false, Encoding.Default))
            {
                int opt_period = 420000;
                int sim_period = 180000;
                int slide = 100000;
                sw.WriteLine("pl per min,num trade,vola,fired[0],fired[1],fired[2],pl per min,num trade,vola,fired[0],fired[1],fired[2]");
                for (int i = 0; i < 30; i++)
                {
                    var gim = new GAIslandMaster2();
                    int from = TickData.price.Count - 6000000 + i*slide;
                    int to = from + opt_period;
                    var chro = gim.startGA(from, to, 3, 15, 15, 3, 0.05, 10, 30, 10, 10, false);
                    var s = new SIM2();
                    var ac = s.startContrarianTrendFollowSashine(from, to, chro, false, false);
                    Form1.Form1Instance.addListBox2(i.ToString()+" - "+"GA period - pl per min=" + Math.Round(ac.pl_per_min, 2) + ",num trade=" + Math.Round(ac.num_trade_per_hour, 2) + ", vola=" + Math.Round(ac.total_pl_vola, 2) + ", fired box num=" + ac.fired_box_ind_num[0] + " : " + ac.fired_box_ind_num[1] + " : " + ac.fired_box_ind_num[2]);
                    

                    s = new SIM2();
                    var ac2 = s.startContrarianTrendFollowSashine(to, to + sim_period, chro, true, false);
                    //var ac2 = s.startContrarianTrendFollowSashine(from, to, chro, true, false);
                    Form1.Form1Instance.addListBox2("SIM period - pl per min=" + Math.Round(ac2.pl_per_min, 2) + ",num trade=" + Math.Round(ac2.num_trade_per_hour, 2) + ", vola=" + Math.Round(ac2.total_pl_vola, 2) + ", fired box num=" + ac2.fired_box_ind_num[0] + " : " + ac2.fired_box_ind_num[1] + " : " + ac2.fired_box_ind_num[2]);

                    sw.WriteLine(Math.Round(ac.pl_per_min, 2) + "," + Math.Round(ac.num_trade_per_hour,6) + "," + Math.Round(ac.total_pl_vola) + "," + ac.fired_box_ind_num[0] + "," + ac.fired_box_ind_num[1] + "," + ac.fired_box_ind_num[2] +
                        ","+ Math.Round(ac2.pl_per_min, 2) + "," + Math.Round(ac2.num_trade_per_hour,6) + "," + Math.Round(ac2.total_pl_vola) + "," + ac2.fired_box_ind_num[0] + "," + ac2.fired_box_ind_num[1] + "," + ac2.fired_box_ind_num[2]);
                }
            }
        }
    }
}
