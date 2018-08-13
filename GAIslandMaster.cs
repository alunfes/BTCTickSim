using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTCTickSim
{
    class GAIslandMaster
    {
        public Dictionary<int, GAIsland> islands;
        public Dictionary<int, Chrome> best_chrome;
        public Dictionary<int, AccountGA> best_ac;


        public Chrome startGA(int num_chrom, int num_generation, int num_island, double immig_rate, int elite_crossover_generation, double pl_per_min_importance, double num_trade_importance, double pl_vola_importance, int from, int to, bool writelog)
        {
            Form1.Form1Instance.setLabel("GA - from:" + TickData.time[from].ToString() + " to:" + TickData.time[to].ToString());

            initialize(num_chrom, num_island);
            generateIsland(num_chrom, num_island);

            for (int i = 0; i < num_generation; i++)
            {
                for (int j = 0; j < num_island; j++)
                {
                    islands[j].evaluateChrom(from, to, i, pl_per_min_importance, num_trade_importance, 0, pl_vola_importance);
                    islands[j].roulettteSelectionNonElite(i, elite_crossover_generation);
                    islands[j].crossOver();
                    islands[j].mutation();
                    islands[j].goToNextGeneration();
                }
                immigration(immig_rate, num_island, num_chrom);
                checkBestChrome(i);
                Form1.Form1Instance.addListBox("#"+i.ToString()+", pl per min="+Math.Round(best_ac[best_ac.Count-1].pl_per_min,2).ToString()+", num trade per hour="+ Math.Round(best_ac[best_ac.Count - 1].num_trade_per_hour,2).ToString()
                    +", total pl vola="+ Math.Round(best_ac[best_ac.Count - 1].total_pl_vola,2).ToString());
            }
            return best_chrome[num_generation-1];
        }

        private void initialize(int num_chro, int num_island)
        {
            islands = new Dictionary<int, GAIsland>();
            best_chrome = new Dictionary<int, Chrome>();
            best_ac = new Dictionary<int, AccountGA>();
        }

        private void generateIsland(int num_chro, int num_island)
        {
            for (int i = 0; i < num_island; i++)
            {
                var ga = new GAIsland();
                ga.initialize(i);
                ga.generateRandomeChromes(num_chro);
                islands.Add(i, ga);
            }
        }

        private void immigration(double immigration_rate, int num_island, int num_chrome)
        {
            var r = RandomProvider.getRandom();
            for(int i=0; i<num_island; i++)
            {
                for(int j=0; j<num_chrome; j++)
                {
                    if(r.Next() < immigration_rate)
                    {
                        int selected_island = r.Next(0, num_island);
                        int selected_chro = -1;
                        do
                            selected_chro = r.Next(0, num_chrome);
                        while (selected_chro != islands[selected_island].best_chrom_ind[islands[selected_island].best_chrom_ind.Count - 1]);

                        islands[selected_island].chromes[selected_chro].Gene_exit_time_sec = (r.Next() > 0.5) ? islands[i].chromes[j].Gene_exit_time_sec : islands[selected_island].chromes[selected_chro].Gene_exit_time_sec;
                        islands[selected_island].chromes[selected_chro].Gene_kairi_term = (r.Next() > 0.5) ? islands[i].chromes[j].Gene_kairi_term : islands[selected_island].chromes[selected_chro].Gene_kairi_term;
                        islands[selected_island].chromes[selected_chro].Gene_entry_kairi = (r.Next() > 0.5) ? islands[i].chromes[j].Gene_entry_kairi : islands[selected_island].chromes[selected_chro].Gene_entry_kairi;
                        islands[selected_island].chromes[selected_chro].Gene_rikaku_percentage = (r.Next() > 0.5) ? islands[i].chromes[j].Gene_rikaku_percentage : islands[selected_island].chromes[selected_chro].Gene_rikaku_percentage;
                    }
                }
            }
        }

        private void checkBestChrome(int generation)
        {
            double max_pl_per_min = -9999;
            int max_ind = -1;
            for (int i=0; i< islands.Count; i++)
            {
                if(max_pl_per_min < islands[i].best_ac_log[islands[i].best_ac_log.Count - 1].pl_per_min)
                {
                    max_pl_per_min = islands[i].best_ac_log[islands[i].best_ac_log.Count - 1].pl_per_min;
                    max_ind = i;
                }
            }
            best_ac.Add(generation, islands[max_ind].best_ac_log[islands[max_ind].best_ac_log.Count - 1]);
        }
    }
}
