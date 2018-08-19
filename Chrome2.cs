using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BTCTickSim
{
    class Chrome2
    {
        public int num_box;
        public double[] gene_ceil_vola { get; set; }
        public double[] gene_floor_vola { get; set; }
        public double[] gene_ceil_avevol { get; set; }
        public double[] gene_floor_avevol { get; set; }

        public int[] gene_exit_time_sec { get; set; }
        public double[] gene_entry_kairi { get; set; }
        public double[] gene_rikaku_percentage { get; set; }
        public bool[] gene_kirikae { get; set; }

        public int start_ind;
        public int end_ind;

        public double ceil_vola;
        public double floor_vola;
        public double ceil_avevol;
        public double floor_avevol;

        public Chrome2(int start, int end, int n_box)
        {
            start_ind = start;
            end_ind = end;
            num_box = n_box;
            var vola = TickData.vola_500.GetRange(start, end - start);
            ceil_vola = vola.Max();
            floor_vola = vola.Min();
            var vol = TickData.ave_vol_500.GetRange(start, end - start);
            ceil_avevol = vol.Max();
            floor_avevol = vol.Min();
            

            generateInitialChrome();
        }

        public void generateInitialChrome()
        {
            gene_ceil_vola = new double[num_box];
            gene_floor_vola = new double[num_box];
            gene_ceil_avevol = new double[num_box];
            gene_floor_avevol = new double[num_box];
            gene_exit_time_sec = new int[num_box];
            gene_entry_kairi = new double[num_box];
            gene_rikaku_percentage = new double[num_box];
            gene_kirikae = new bool[num_box];

            for(int i=0; i<num_box; i++)
            {
                gene_floor_vola[i] = getFloorVolaGene();
                gene_ceil_vola[i] = getCeilVolaGene(gene_floor_vola[i]);
                gene_floor_avevol[i] = getFloorAvevolGene();
                gene_ceil_avevol[i] = getCeilAvevolGene(gene_floor_avevol[i]);
                gene_exit_time_sec[i] = getExitTimeSecGene();
                gene_entry_kairi[i] = getEntryKairiGene();
                gene_rikaku_percentage[i] = getRikakuPercentageGene();
                gene_kirikae[i] = getKirikaeGene();
            }
        }

        public double getFloorVolaGene()
        {
            var r = RandomProvider.getRandom();
            return r.Next((int)Math.Truncate(floor_vola), (int)Math.Truncate(ceil_vola * 0.8));
        }
        public double getCeilVolaGene(double floor)
        {
            var r = RandomProvider.getRandom();
            return r.Next((int)Math.Truncate(floor)+10, (int)Math.Truncate(ceil_vola));
        }
        public double getFloorAvevolGene()
        {
            var r = RandomProvider.getRandom();
            return r.Next((int)Math.Truncate(floor_avevol), (int)Math.Truncate(ceil_avevol * 0.8));
        }
        public double getCeilAvevolGene(double floor)
        {
            var r = RandomProvider.getRandom();
            return r.Next((int)Math.Truncate(floor) + 500, (int)Math.Truncate(ceil_avevol));
        }

        public int getExitTimeSecGene()
        {
            var r = RandomProvider.getRandom();
            return r.Next(3, 601);
        }
        public double getEntryKairiGene()
        {
            var r = RandomProvider.getRandom();
            return Convert.ToDouble(r.Next(1, 601)) / 10000.0;
        }
        public double getRikakuPercentageGene()
        {
            var r = RandomProvider.getRandom();
            return Convert.ToDouble(r.Next(1, 501)) / 10000.0;
        }
        public bool getKirikaeGene()
        {
            var r = RandomProvider.getRandom();
            return (r.NextDouble() > 0.5) ? true:  false;
        }

        public void writeChrome()
        {
            using (StreamWriter sw = new StreamWriter("./best chrome2.csv", false, Encoding.Default))
            {
                sw.WriteLine("start ind," + start_ind);
                sw.WriteLine("end ind," + end_ind);
                for(int i=0; i<num_box; i++)
                {
                    sw.WriteLine("gene_floor_vola," + gene_floor_vola[i]);
                    sw.WriteLine("gene_ceil_vola," + gene_ceil_vola[i]);
                    sw.WriteLine("gene_floor_avevol," + gene_floor_avevol[i]);
                    sw.WriteLine("gene_ceil_avevol," + gene_ceil_avevol[i]);
                    sw.WriteLine("gene_exit_time_sec," + gene_exit_time_sec[i]);
                    sw.WriteLine("gene_entry_kairi," + gene_entry_kairi[i]);
                    sw.WriteLine("gene_rikaku_percentage," + gene_rikaku_percentage[i]);
                    sw.WriteLine("gene_kirikae," + gene_kirikae[i]);
                }
            }
        }
    }
}
