using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace BTCTickSim
{
    class GAIsland
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

        public double base_total_eva;
        public double base_pl_eva;
        public double base_stability_eva;
        public double base_num_eva;

        public double max_pl_per_min;
        public double min_pl_per_min;
        public double base_pl_per_min;
        public double max_num_trade;
        public double min_num_trade;
        public double base_num_trade;
        public double max_profit_factor;
        public double min_profit_factor;
        public double base_profit_factor;
        public double max_pl_vola;
        public double min_pl_vola;
        public double base_pl_vola;

        public int island_ind;

        public void addAcList(int i, AccountGA ac)
        {
            lock (lockobj)
                ac_list.Add(i, ac);
        }
        
        public void initialize(int islandind)
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

            base_total_eva = 0;
            base_pl_eva = 0;
            base_stability_eva = 0;
            base_num_eva = 0;

            max_pl_per_min = 0;
            min_pl_per_min = 0;
            base_pl_per_min = 0;
            max_num_trade = 0;
            min_num_trade = 0;
            base_num_trade = 0;
            max_profit_factor = 0;
            min_profit_factor = 0;
            base_profit_factor = 0;
            max_pl_vola = 0;
            min_pl_vola = 0;
            base_pl_vola = 0;

            island_ind = islandind;
        }

        public void generateRandomeChromes(int num)
        {
            for (int i = 0; i < num; i++)
            {
                Chrome c = new Chrome();
                chromes.Add(c);
            }
        }
        
        public void evaluateChrom(int from, int to, int num_generation, double pl_per_min_importance, double num_trade_importance, double profit_factor_importance, double pl_vola_importance)
        {
            //do sim for all chromes
            eva = new List<double>();
            ac_list = new Dictionary<int, AccountGA>();
            Parallel.For(0, chromes.Count, i => {
                SIMGA sim = new SIMGA();
                addAcList(i, sim.startContrarianSashine(from, to, chromes[i].Gene_exit_time_sec, chromes[i].Gene_kairi_term, chromes[i].Gene_entry_kairi, chromes[i].Gene_rikaku_percentage));
            });

            //calc for performance eva
            List<AccountGA> aclists = new List<AccountGA>();
            for (int i = 0; i < chromes.Count; i++)
                aclists.Add(ac_list[i]);

            var pl_per_min = aclists.Select(x => x.pl_per_min).ToList();
            var num_trade = aclists.Select(x => x.num_trade_per_hour).ToList();
            var profit_factor = aclists.Select(x => x.profit_factor).ToList();
            var pl_vola = aclists.Select(x => (1.0 / x.total_pl_vola)).ToList();

            if (num_generation == 0)
            {
                min_pl_per_min = pl_per_min.Min();
                max_pl_per_min = (pl_per_min.Max() - min_pl_per_min) * 10000;
                base_pl_per_min = (pl_per_min_importance * max_pl_per_min) / (pl_per_min.Max() - min_pl_per_min);

                min_num_trade = num_trade.Min();
                max_num_trade = (num_trade.Max() - min_num_trade) * 10000;
                base_num_trade = (num_trade_importance * max_num_trade) / (num_trade.Max() - num_trade.Min());

                min_profit_factor = profit_factor.Min();
                max_profit_factor = (profit_factor.Max() - min_profit_factor) * 10000;
                base_profit_factor = (profit_factor_importance * max_profit_factor) / (profit_factor.Max() - profit_factor.Min());

                min_pl_vola = pl_vola.Min();
                max_pl_vola = (pl_vola.Max() - min_pl_vola) * 10000;
                base_pl_vola = (pl_vola_importance * max_pl_vola) / (pl_vola.Max() - pl_vola.Min());
            }

            for (int i = 0; i < aclists.Count; i++)
            {
                double eva_pl_per_min = (pl_per_min[i] >= min_pl_per_min) ? base_pl_per_min * (pl_per_min[i] - min_pl_per_min) / max_pl_per_min : min_pl_per_min;
                double eva_num_trade = (num_trade[i] >= min_num_trade) ? base_num_trade * (num_trade[i] - min_num_trade) / max_num_trade : min_num_trade;
                double eva_profit_factor = (profit_factor[i] >= min_profit_factor) ? base_profit_factor * (profit_factor[i] - min_profit_factor) / max_profit_factor : min_profit_factor;
                double eva_pl_vola = (pl_vola[i] >= min_pl_vola) ? base_pl_vola * (pl_vola[i] - min_pl_vola) / max_pl_vola : min_pl_vola;

                eva.Add(pl_per_min_importance * calcLog(eva_pl_per_min) + num_trade_importance * calcLog(eva_num_trade) + pl_vola_importance * calcLog(eva_pl_vola));
            }

            //check for max eva inde
            double max = eva.Max();
            var m = eva.Select((p, i) => new { Content = p, Index = i })
    .Where(ano => ano.Content >= max)
    .Select(ano => ano.Index).ToList();
            best_chrom_ind.Add(m[0]);
            best_eva_log.Add(eva[m[0]]);
            best_ac_log.Add(aclists[m[0]]);
        }

        public double calcLog(double v)
        {
            return Math.Log(v + 20);
        }


        public void rouletteSelection()
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
            for (int i = 0; i < eva.Count; i++)
            {
                double roulette_v = ran.Next(0, Convert.ToInt32(Math.Truncate(sum) + 1));
                for (int j = 1; j < board.Count; j++)
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
            if (selected_chrom.Count != chromes.Count)
                Debug.WriteLine("kita");
        }

        public void roulettteSelectionNonElite(int current_generaiton, int elite_crossover_start_gen)
        {
            selected_chrom = new List<int>();
            var ran = RandomProvider.getRandom();
            List<double> board = new List<double>();
            double sum = 0;
            if (current_generaiton < elite_crossover_start_gen)
            {
                for (int i = 0; i < eva.Count; i++)
                {
                    if (i != best_chrom_ind[best_chrom_ind.Count - 1])
                    {
                        sum += eva[i];
                        board.Add(sum);
                    }
                }
            }
            else
            {
                for (int i = 0; i < eva.Count; i++)
                {
                    sum += eva[i];
                    board.Add(sum);
                }
            }
            //roulette select
            if (current_generaiton < elite_crossover_start_gen)
            {
                for (int i = 0; i < eva.Count; i++)
                {
                    if (i != best_chrom_ind[best_chrom_ind.Count - 1])
                    {
                        double roulette_v = ran.Next(0, Convert.ToInt32(Math.Truncate(sum) + 1));
                        for (int j = 1; j < board.Count; j++)
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
                    else
                        selected_chrom.Add(best_chrom_ind[best_chrom_ind.Count - 1]);
                }
            }
            else
            {
                for (int i = 0; i < eva.Count; i++)
                {
                    double roulette_v = ran.Next(0, Convert.ToInt32(Math.Truncate(sum) + 1));
                    for (int j = 1; j < board.Count; j++)
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
        }

        public void crossOver()
        {
            new_gene_chromes = new List<Chrome>();

            var ran = RandomProvider.getRandom();
            for (int i = 0; i < chromes.Count; i++)
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


        public void mutation()
        {
            var ran = RandomProvider.getRandom();
            Chrome c = new Chrome();
            for (int i = 0; i < new_gene_chromes.Count; i++)
            {
                if (i != best_chrom_ind[best_chrom_ind.Count - 1])
                {
                    if (ran.NextDouble() > 0.1)
                    {
                        new_gene_chromes[i].Gene_exit_time_sec = (ran.NextDouble() > 0.1) ? c.getExitTimeSecGene() : new_gene_chromes[i].Gene_exit_time_sec;
                        new_gene_chromes[i].Gene_kairi_term = (ran.NextDouble() > 0.1) ? c.getKairiTermGene() : new_gene_chromes[i].Gene_kairi_term;
                        new_gene_chromes[i].Gene_entry_kairi = (ran.NextDouble() > 0.1) ? c.getEntryKairiGene() : new_gene_chromes[i].Gene_entry_kairi;
                        new_gene_chromes[i].Gene_rikaku_percentage = (ran.NextDouble() > 0.1) ? c.getRikakuPercentageGene() : new_gene_chromes[i].Gene_rikaku_percentage;
                    }
                }
            }
        }

        public void goToNextGeneration()
        {
            chromes = new List<Chrome>();
            for (int i = 0; i < new_gene_chromes.Count; i++)
                chromes.Add(new_gene_chromes[i]);
        }
        
    }
}
