using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTCTickSim
{
    class GA
    {
        public List<Chrome> chromes;
        public List<double> eva;
        public List<int> best_chrom_ind;
        public List<int> selected_chrom;
        public List<Account> ac_list;
        public List<Chrome> new_gene_chromes;


        public void startGA(int num_chrom, int num_generation, int from, int to)
        {
            Form1.Form1Instance.setLabel("Started GA");
            initialize();
            generateRandomeChromes(num_chrom);

            //generation loop
            for (int i = 0; i < num_generation; i++)
            {
                Form1.Form1Instance.setLabel("Generation #" + i.ToString() + " Evaluating");
                evaluateChrom(from, to);

            }
        }

        private void initialize()
        {
            chromes = new List<Chrome>();
            eva = new List<double>();
            best_chrom_ind = new List<int>();
            selected_chrom = new List<int>();
            ac_list = new List<Account>();
            new_gene_chromes = new List<Chrome>();
        }

        private void generateRandomeChromes(int num)
        {
            for (int i = 0; i < num; i++)
            {
                Chrome c = new Chrome();
                chromes.Add(c);
            }
        }

        private void evaluateChrom(int from, int to)
        {
            //do sim for all chromes
            eva = new List<double>();
            ac_list = new List<Account>();
            Parallel.For(0, chromes.Count, i => {
                SIM sim = new SIM();
                ac_list.Add(sim.startContrarianSashine(from, to, chromes[i].Gene_exit_time_sec, chromes[i].Gene_kairi_term, chromes[i].Gene_entry_kairi, chromes[i].Gene_rikaku_percentage, false));
            });

            //make list of pl_per_min, num trade
            var pl_per_min = ac_list
                .Select(x => x.pl_per_min);
            var num_trade = ac_list
                .Select(x => x.num_trade);

            double max_pl_per_min = pl_per_min.Max();
            double min_pl_per_min = pl_per_min.Min();
            double max_num_trade = num_trade.Max();
            double min_num_trade = num_trade.Min();


            //regulate to 0-100 for pl_per_min and num_trade
            double pl_max_var = (max_pl_per_min - min_pl_per_min) / 100.0;
            double num_max_var = (max_num_trade - min_num_trade) / 100.0;
            for (int i = 0; i < ac_list.Count; i++)
            {
                if (ac_list[i].num_trade == 0)
                    eva.Add(0);
                else
                {
                    double eva_pl = (ac_list[i].pl_per_min - min_pl_per_min) / pl_max_var;
                    double eva_num = (ac_list[i].num_trade - min_num_trade) / num_max_var;
                    eva.Add(eva_pl + eva_num * 0.1);
                }
            }

            //check for max eva index
            double max = eva.Max();
            var m = eva.Select((p, i) => new { Content = p, Index = i })
    .Where(ano => ano.Content >= max)
    .Select(ano => ano.Index).ToList();
            best_chrom_ind.Add(m[0]);
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
                double roulette_v = ran.Next(0,Convert.ToInt32(Math.Round(sum*100+1)));
                for(int j=1; j<board.Count; j++)
                {
                    if (0 >= roulette_v && board[j] <= roulette_v)
                        selected_chrom[i] = 0;
                    if (board[j - 1] > roulette_v && board[j] <= roulette_v)
                        selected_chrom[i] = j; ;
                }
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
            for(int i=0; i<new_gene_chromes.Count; i++)
            {
                if(i!=best_chrom_ind[best_chrom_ind.Count-1])
                {
                    if(ran.NextDouble() > )
                }
            }
        }


    }
}
