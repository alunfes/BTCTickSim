using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BTCTickSim
{
    /*(double exit_time_sec, int kairi_term, double entry_kairi, double rikaku_percentage)
     * exit_time_sec        [int 3-600]
     * kairi_term           [int 3-600]
     * entry_kairi          [double 0.0005 - 0.015]
     * rikaku_percentage    [double 0.0005 - 0.015]
     * */
    class Chrome
    {
        public int Gene_exit_time_sec { get; set; }
        public int Gene_kairi_term { get; set; }
        public double Gene_entry_kairi { get; set; }
        public double Gene_rikaku_percentage { get; set; }

        public Chrome()
        {
            generateInitialChrome();
        }

        public void generateInitialChrome()
        {
            var r = RandomProvider.getRandom();
            Gene_exit_time_sec = r.Next(3, 601);
            Gene_kairi_term = r.Next(3, 601);
            Gene_entry_kairi = Convert.ToDouble(r.Next(5, 151)) / 10000.0;
            Gene_rikaku_percentage = Convert.ToDouble(r.Next(5, 151)) / 10000.0;
        }

        public int getExitTimeSecGene()
        {
            var r = RandomProvider.getRandom();
            return r.Next(3, 601);
        }
        public int getKairiTermGene()
        {
            var r = RandomProvider.getRandom();
            return r.Next(3, 601);
        }
        public double getEntryKairiGene()
        {
            var r = RandomProvider.getRandom();
            return Convert.ToDouble(r.Next(5, 151)) / 10000.0;
        }
        public double getRikakuPercentageGene()
        {
            var r = RandomProvider.getRandom();
            return Convert.ToDouble(r.Next(5, 151)) / 10000.0;
        }

        public void readBestChromFile()
        {
            using (StreamReader sr = new StreamReader("./best chrome gene.csv", Encoding.Default))
            {
                for (int i = 0; i < 3; i++)
                    sr.ReadLine();
                string line = sr.ReadLine();
                var ele = line.Split(',');
                Gene_exit_time_sec = Convert.ToInt32( ele[0]);
                Gene_kairi_term = Convert.ToInt32(ele[1]);
                Gene_entry_kairi = Convert.ToDouble(ele[2]);
                Gene_rikaku_percentage = Convert.ToDouble(ele[3]);
            }
        }
    }
}
