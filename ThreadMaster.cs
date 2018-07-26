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

        private void gaForMultiplePeriod()
        {
            TickData.readTickData();
            
            using (StreamWriter sw = new StreamWriter("./multi ga.csv", false, Encoding.Default))
            {
                sw.WriteLine("from,to,best pl per min,best cum pl,best num trade,best win rate,pl per min,cum pl,num trade,win rate");
                for (int i = 0; i < 10; i++)
                {
                    GA ga = new GA();
                    int from = TickData.time.Count - 10000000 + (i * 500000);
                    int to = TickData.time.Count - 9500000 + (i * 500000);
                    Form1.Form1Instance.setLabel3("#"+i.ToString()+":doing for " + from.ToString() + " - " + to.ToString());
                    var chro = ga.startGA(20, 25, from, to, false);
                    SIM s = new SIM();
                    var ac =  s.startContrarianSashine(from, to, chro.Gene_exit_time_sec, chro.Gene_kairi_term, chro.Gene_entry_kairi, chro.Gene_rikaku_percentage, false);
                    var ac2 = s.startContrarianSashine(to, to+100000, chro.Gene_exit_time_sec, chro.Gene_kairi_term, chro.Gene_entry_kairi, chro.Gene_rikaku_percentage, false);


                    sw.WriteLine(from.ToString()+","+to.ToString()+","+ac.pl_per_min.ToString()+","+ac.cum_pl.ToString()+","+ac.num_trade.ToString()+","+ac.win_rate.ToString()+
                        ","+ac2.pl_per_min.ToString()+","+ac2.cum_pl.ToString()+","+ac2.num_trade.ToString()+","+ac2.win_rate.ToString());
                }
            }
            Form1.Form1Instance.setLabel3("Completed all calc");
        }


        private void doGA()
        {
            TickData.readTickData();
            GA ga = new GA();
            ga.startGA(50, 10, TickData.time.Count - 10000000, TickData.time.Count - 8000000, true);
        }

        private void doSIm()
        {
            TickData.readTickData();

            SIM s = new SIM();
            s.startContrarianSashine(TickData.price.Count-10000, TickData.price.Count-100, 30, 60, 0.001, 0.0005, true);
        }
    }
}
