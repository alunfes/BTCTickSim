using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BTCTickSim
{
    class GAIsland2
    {
        public List<Chrome2> chromes;
        public List<double> eva;
        public List<int> selected_chrom;
        public Dictionary<int, AccountGA2> ac_list;
        public List<Chrome2> new_gene_chromes;
        private object lockobj = new object();

        public List<double> best_eva_log;
        public List<int> best_chrom_ind;
        public List<AccountGA2> best_ac_log;

        private int start_i;
        private int end_i;
        

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

        public int from;
        public int to;
        public int num_box;
        

        public void addAcList(int i, AccountGA2 ac)
        {
            lock (lockobj)
                ac_list.Add(i, ac);
        }

        public void initialize(int islandind)
        {
            chromes = new List<Chrome2>();
            eva = new List<double>();
            best_chrom_ind = new List<int>();
            selected_chrom = new List<int>();
            ac_list = new Dictionary<int, AccountGA2>();
            new_gene_chromes = new List<Chrome2>();
            best_ac_log = new List<AccountGA2>();
            best_eva_log = new List<double>();

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

        public void generateRandomeChromes(int num, int from, int to, int num_box)
        {
            this.from = from;
            this.to = to;
            this.num_box = num_box;
            for (int i = 0; i < num; i++)
            {
                var c = new Chrome2(from, to, num_box);
                chromes.Add(c);
            }
        }

        public void evaluateChrom(int num_generation, double pl_per_min_importance, double num_trade_importance, double profit_factor_importance, double pl_vola_importance)
        {
            //do sim for all chromes
            eva = new List<double>();
            ac_list = new Dictionary<int, AccountGA2>();
            Parallel.For(0, chromes.Count, i => {
                SIMGA2 sim = new SIMGA2();
                addAcList(i, sim.startContrarianTrendFollowSashine(from, to, chromes[i]));
            });


            //calc for performance eva
            var aclists = new List<AccountGA2>();
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

                eva.Add(pl_per_min_importance * calcPowerFunction(2, eva_pl_per_min) + num_trade_importance * calcLN(eva_num_trade) + pl_vola_importance * calcLinerFunction(1, eva_pl_vola));
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

        public double calcLN(double v)
        {
            return (v > 0) ? Math.Log(v) : 0;
        }
        public double calcLinerFunction(double a, double x)
        {
            return a * x;
        }
        public double calcPowerFunction(double a, double x)
        {
            return Math.Pow(x, a);
        }

        public void roulettteSelectionNonElite(int current_generaiton, int elite_crossover_start_gen)
        {
            selected_chrom = new List<int>();
            var ran = RandomProvider.getRandom();
            List<double> board = new List<double>();
            double sum = 0;
            double[] eva_copy = eva.ToArray();
            eva_copy[best_chrom_ind[best_chrom_ind.Count - 1]] = 0;

            for (int i = 0; i < eva_copy.Length; i++)
            {
                sum += eva_copy[i];
                board.Add(sum);
            }

            //roulette select
            for (int i = 0; i < eva_copy.Length; i++)
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
            selected_chrom[best_chrom_ind[best_chrom_ind.Count - 1]] = best_chrom_ind[best_chrom_ind.Count - 1];
        }

        public void crossOver()
        {
            new_gene_chromes = new List<Chrome2>();

            var ran = RandomProvider.getRandom();
            for (int i = 0; i < chromes.Count; i++)
            {
                var c = new Chrome2(from, to, num_box);
                c.box_fired_num = chromes[i].box_fired_num;
                for(int j=0; j<c.num_box; j++)
                {
                    if((ran.NextDouble() > 0.5))
                    {
                        c.gene_floor_vola[j] = chromes[selected_chrom[i]].gene_floor_vola[j];
                        c.gene_ceil_vola[j] = chromes[selected_chrom[i]].gene_ceil_vola[j];
                    }
                    else
                    {
                        c.gene_floor_vola[j] = chromes[i].gene_floor_vola[j];
                        c.gene_ceil_vola[j] = chromes[i].gene_ceil_vola[j];
                    }
                    if ((ran.NextDouble() > 0.5))
                    {
                        c.gene_floor_avevol[j] = chromes[selected_chrom[i]].gene_floor_avevol[j];
                        c.gene_ceil_avevol[j] = chromes[selected_chrom[i]].gene_ceil_avevol[j];
                    }
                    else
                    {
                        c.gene_floor_avevol[j] = chromes[i].gene_floor_avevol[j];
                        c.gene_ceil_avevol[j] = chromes[i].gene_ceil_avevol[j];
                    }
                    c.gene_exit_time_sec[j] = (ran.NextDouble() > 0.5) ? chromes[selected_chrom[i]].gene_exit_time_sec[j] : chromes[i].gene_exit_time_sec[j];
                    c.gene_entry_kairi[j] = (ran.NextDouble() > 0.5) ? chromes[selected_chrom[i]].gene_exit_time_sec[j] : chromes[i].gene_exit_time_sec[j];
                    c.gene_rikaku_percentage[j] = (ran.NextDouble() > 0.5) ? chromes[selected_chrom[i]].gene_rikaku_percentage[j] : chromes[i].gene_rikaku_percentage[j];
                    c.gene_kirikae[j] = (ran.NextDouble() > 0.5) ? chromes[selected_chrom[i]].gene_kirikae[j] : chromes[i].gene_kirikae[j];
                }
                new_gene_chromes.Add(c);
            }
        }


        public void mutation()
        {
            var ran = RandomProvider.getRandom();
            var c = new Chrome2(from, to, num_box);
            for (int i = 0; i < new_gene_chromes.Count; i++)
            {
                if (i != best_chrom_ind[best_chrom_ind.Count - 1])
                {
                    if (ran.NextDouble() > 0.1)
                    {
                        for(int j=0; j<new_gene_chromes[i].num_box; j++)
                        {
                            if(ran.NextDouble() > 0.1)
                            {
                                new_gene_chromes[i].gene_floor_vola[j] = c.getFloorVolaGene();
                                new_gene_chromes[i].gene_ceil_vola[j] = c.getCeilVolaGene(new_gene_chromes[i].gene_floor_vola[j]);
                            }
                            else
                            {
                                new_gene_chromes[i].gene_floor_vola[j] = new_gene_chromes[i].gene_floor_vola[j];
                                new_gene_chromes[i].gene_ceil_vola[j] = new_gene_chromes[i].gene_ceil_vola[j];
                            }
                            if (ran.NextDouble() > 0.1)
                            {
                                new_gene_chromes[i].gene_floor_avevol[j] = c.getFloorAvevolGene();
                                new_gene_chromes[i].gene_ceil_avevol[j] = c.getCeilAvevolGene(new_gene_chromes[i].gene_floor_avevol[j]);
                            }
                            else
                            {
                                new_gene_chromes[i].gene_floor_avevol[j] = new_gene_chromes[i].gene_floor_avevol[j];
                                new_gene_chromes[i].gene_ceil_avevol[j] = new_gene_chromes[i].gene_ceil_avevol[j];
                            }
                            new_gene_chromes[i].gene_exit_time_sec[j] = (ran.NextDouble() > 0.1) ? c.getExitTimeSecGene() : new_gene_chromes[i].gene_exit_time_sec[j];
                            new_gene_chromes[i].gene_rikaku_percentage[j] = (ran.NextDouble() > 0.1) ? c.getRikakuPercentageGene() : new_gene_chromes[i].gene_rikaku_percentage[j];
                            new_gene_chromes[i].gene_kirikae[j] = (ran.NextDouble() > 0.1) ? c.getKirikaeGene() : new_gene_chromes[i].gene_kirikae[j];
                            new_gene_chromes[i].gene_entry_kairi[j] = (ran.NextDouble() > 0.1) ? c.getEntryKairiGene() : new_gene_chromes[i].gene_entry_kairi[j];
                        }
                    }
                }
            }
        }

        public void goToNextGeneration()
        {
            chromes = new List<Chrome2>();
            for (int i = 0; i < new_gene_chromes.Count; i++)
                chromes.Add(new_gene_chromes[i]);
        }

    }
}
