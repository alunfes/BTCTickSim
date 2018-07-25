using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace BTCTickSim
{
    class GA
    {
        public List<Chrome> chromes;
        public List<double> eva;
        public List<int> selected_chrom;    
        public Dictionary<int, AccountGA> ac_list;
        public List<Chrome> new_gene_chromes;
        private object lockobj = new object();

        public List<double> best_eva_log;
        public List<int> best_chrom_ind;
        public List<AccountGA> best_ac_log;

        private int start_i;
        private int end_i;

        private Stopwatch sw = new Stopwatch();
        public TimeSpan estimated_remaining_time;

        public void addAcList(int i, AccountGA ac)
        {
            lock (lockobj)
                ac_list.Add(i, ac);
        }


        public Chrome startGA(int num_chrom, int num_generation, int from, int to, bool writelog)
        {
            //Form1.Form1Instance.initializeListBox();
            start_i = from;
            end_i = to;

            Form1.Form1Instance.setLabel("GA - from:"+TickData.time[from].ToString() + " to:"+TickData.time[to].ToString());
            
            initialize();
            generateRandomeChromes(num_chrom);

            //generation loop
            double sec = 0;
            for (int i = 0; i < num_generation; i++)
            {
                sw.Start();

                evaluateChrom(from, to, i);
                rouletteSelection();
                crossOver();
                mutation();
                goToNextGeneration();

                Form1.Form1Instance.addListBox("#"+i.ToString()+": eva="+best_eva_log[best_eva_log.Count-1].ToString()+", pl per min="+best_ac_log[best_ac_log.Count-1].pl_per_min.ToString());

                sw.Stop();
                sec += sw.Elapsed.TotalSeconds;
                estimated_remaining_time = new TimeSpan(0, 0, Convert.ToInt32(Convert.ToDouble(num_generation - Convert.ToDouble(i+1)) * (sec / (Convert.ToDouble(i+1)))));
                Form1.Form1Instance.setLabel2("Estimated Remaining Time to Complete=" + estimated_remaining_time.ToString());
            }
            Form1.Form1Instance.setLabel2("GA calculation was completed, writing log..");
            if (writelog) writeLog();
            Form1.Form1Instance.setLabel2("Completed GA");

            return chromes[best_chrom_ind[best_chrom_ind.Count - 1]];
        }

        private void initialize()
        {
            chromes = new List<Chrome>();
            eva = new List<double>();
            best_chrom_ind = new List<int>();
            selected_chrom = new List<int>();
            ac_list = new Dictionary<int, AccountGA>();
            new_gene_chromes = new List<Chrome>();
            best_ac_log = new List<AccountGA>();
            best_eva_log = new List<double>();
            estimated_remaining_time = new TimeSpan();
        }

        private void generateRandomeChromes(int num)
        {
            for (int i = 0; i < num; i++)
            {
                Chrome c = new Chrome();
                chromes.Add(c);
            }
        }

        private void evaluateChrom(int from, int to, int n_g)
        {
            //do sim for all chromes
            eva = new List<double>();
            ac_list = new Dictionary<int, AccountGA>();
            Parallel.For(0, chromes.Count, i => {
                SIMGA sim = new SIMGA();
                addAcList(i, sim.startContrarianSashine(from, to, chromes[i].Gene_exit_time_sec, chromes[i].Gene_kairi_term, chromes[i].Gene_entry_kairi, chromes[i].Gene_rikaku_percentage));
            });

            //make list of pl_per_min, num trade
            List<AccountGA> lists = new List<AccountGA>();
            for (int i = 0; i < chromes.Count; i++)
                lists.Add(ac_list[i]);
            var pl_per_min = lists
                .Select(x => x.pl_per_min);
            //var num_trade = lists
            //    .Select(x => x.num_trade);

            double max_pl_per_min = pl_per_min.Max();
            double min_pl_per_min = pl_per_min.Min();
            //double max_num_trade = num_trade.Max();
            //double min_num_trade = num_trade.Min();


            //regulate to 0-100 for pl_per_min and num_trade
            double pl_max_var = (max_pl_per_min - min_pl_per_min) / 100.0;
            //double num_max_var = (max_num_trade - min_num_trade) / 100.0;
            for (int i = 0; i < lists.Count; i++)
            {
                if (lists[i].cum_pl <= 0)
                    eva.Add(0);
                else
                {
                    eva.Add(lists[i].pl_per_min * 1440);
                    //double eva_num = (ac_list[i].num_trade - min_num_trade) / num_max_var;
                }
            }

            //check for max eva index
            double max = eva.Max();
            var m = eva.Select((p, i) => new { Content = p, Index = i })
    .Where(ano => ano.Content >= max)
    .Select(ano => ano.Index).ToList();
            best_chrom_ind.Add(m[0]);
            best_eva_log.Add(eva[m[0]]);
            best_ac_log.Add(lists[m[0]]);

            //display info
            Form1.Form1Instance.setLabel2("pl per min="+ac_list[m[0]].pl_per_min.ToString()+", num trade="+ ac_list[m[0]].num_trade.ToString()+", cum pl="+ ac_list[m[0]].cum_pl.ToString());
        }

        private void rouletteSelection()
        {
            selected_chrom = new List<int>();
            var ran = RandomProvider.getRandom();

            //make roulette board
            List<double> board = new List<double>();
            double sum = 0;
            for (int i = 0; i < eva.Count; i++)
            {
                sum += eva[i];
                board.Add(sum);
            }

            //roulette select
            for(int i=0; i<eva.Count; i++)
            {
                double roulette_v = ran.Next(0,Convert.ToInt32(Math.Round(sum+1)));
                for(int j=1; j<board.Count; j++)
                {
                    if (board[j - 1] < roulette_v && board[j] >= roulette_v)
                    {
                        selected_chrom.Add(j);
                        break;
                    }
                }
                if (0 <= roulette_v && board[0] >= roulette_v)
                    selected_chrom.Add(0);
            }
        }

        private void crossOver()
        {
            new_gene_chromes = new List<Chrome>();

            var ran = RandomProvider.getRandom();
            for(int i=0; i<chromes.Count; i++)
            {
                if (i != best_chrom_ind[best_chrom_ind.Count - 1])
                {
                    var c = new Chrome();
                    c.Gene_exit_time_sec = (ran.NextDouble() > 0.5) ? chromes[selected_chrom[i]].Gene_exit_time_sec : chromes[i].Gene_exit_time_sec;
                    c.Gene_kairi_term = (ran.NextDouble() > 0.5) ? chromes[selected_chrom[i]].Gene_kairi_term : chromes[i].Gene_kairi_term;
                    c.Gene_entry_kairi = (ran.NextDouble() > 0.5) ? chromes[selected_chrom[i]].Gene_entry_kairi : chromes[i].Gene_entry_kairi;
                    c.Gene_rikaku_percentage = (ran.NextDouble() > 0.5) ? chromes[selected_chrom[i]].Gene_rikaku_percentage : chromes[i].Gene_rikaku_percentage;
                    new_gene_chromes.Add(c);
                }
                else
                    new_gene_chromes.Add(chromes[i]);
            }
        }

        private void mutation()
        {
            var ran = RandomProvider.getRandom();
            Chrome c = new Chrome();
            for(int i=0; i<new_gene_chromes.Count; i++)
            {
                if(i!=best_chrom_ind[best_chrom_ind.Count-1])
                {
                    if(ran.NextDouble() > 0.1)
                    {
                        new_gene_chromes[i].Gene_exit_time_sec = (ran.NextDouble() > 0.1) ? c.getExitTimeSecGene() : new_gene_chromes[i].Gene_exit_time_sec;
                        new_gene_chromes[i].Gene_kairi_term = (ran.NextDouble() > 0.1) ? c.getKairiTermGene() : new_gene_chromes[i].Gene_kairi_term;
                        new_gene_chromes[i].Gene_entry_kairi = (ran.NextDouble() > 0.1) ? c.getEntryKairiGene() : new_gene_chromes[i].Gene_entry_kairi;
                        new_gene_chromes[i].Gene_rikaku_percentage = (ran.NextDouble() > 0.1) ? c.getRikakuPercentageGene() : new_gene_chromes[i].Gene_rikaku_percentage;
                    }
                }
            }
        }

        private void goToNextGeneration()
        {
            chromes = new List<Chrome>();
            for (int i = 0; i < new_gene_chromes.Count; i++)
                chromes.Add(new_gene_chromes[i]);
        }

        private void writeLog()
        {
            string path = "./ga log.csv";
            using (StreamWriter sw = new StreamWriter(path, false, Encoding.Default))
            {
                sw.WriteLine("From="+TickData.time[start_i].ToString() + " To="+TickData.time[end_i].ToString());
                sw.WriteLine("Generation,best chrom ind,Eva,best cum pl, best pl per min,best num trade");
                for(int i=0; i<best_eva_log.Count;i++)
                {
                    sw.WriteLine(i.ToString()+","+best_chrom_ind[i].ToString()+","+best_eva_log[i].ToString()+","+best_ac_log[i].cum_pl.ToString()
                        +","+ best_ac_log[i].pl_per_min.ToString()+","+ best_ac_log[i].num_trade.ToString());
                }
            }

            string path2 = "./best chrome gene.csv";
            using (StreamWriter sw = new StreamWriter(path2, false, Encoding.Default))
            {
                sw.WriteLine("From=" + TickData.time[start_i].ToString() + " To=" + TickData.time[end_i].ToString());
                sw.WriteLine("From Index="+start_i.ToString()+" To Index="+end_i.ToString());

                var c = chromes[best_chrom_ind[best_chrom_ind.Count - 1]];
                sw.WriteLine("Exit Time Sec,Kairi Term,Entry Kairi,Rikaku");
                sw.WriteLine(c.Gene_exit_time_sec.ToString()+","+c.Gene_kairi_term.ToString()+","+c.Gene_entry_kairi.ToString()+","+c.Gene_rikaku_percentage.ToString());
            }
        }
    }
}
