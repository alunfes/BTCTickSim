using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BTCTickSim
{
    class Histogram
    {
        public static List<double> calcTickCluster(double cluster_percentage, int start_ind, int num_class)
        {
            var res = new List<double>();
            /*var hist_vola = makeHistogram(TickData.vola_500, start_ind, num_class);
            var hist_makairi = makeHistogram(TickData.makairi_500, start_ind, 1000);
            var hist_avevol = makeHistogram(TickData.ave_vol_500, start_ind, 1000);
            
            var clus_vola = makeCluster(TickData.vola_500, start_ind, hist_vola.hist_class, hist_vola.hist_num, cluster_percentage);
            var clus_makairi = makeCluster(TickData.makairi_500, start_ind, hist_makairi.hist_class, hist_makairi.hist_num, cluster_percentage);
            var clus_avevol = makeCluster(TickData.ave_vol_500, start_ind, hist_avevol.hist_class, hist_avevol.hist_num, cluster_percentage);
            */

            var clus_vola = makeCluster2(TickData.vola_500, start_ind, 3);
            var clus_makairi = makeCluster2(TickData.makairi_500, start_ind,3);
            var clus_avevol = makeCluster2(TickData.ave_vol_500, start_ind, 3);

            var clusters = new List<string>();
            for (int i = 0; i < clus_vola.Count; i++)
            {
                if (clusters.Contains((clus_vola[i] + "," + clus_makairi[i] + "," + clus_avevol[i]).ToString()) == false)
                {
                    clusters.Add((clus_vola[i] + "," + clus_makairi[i] + "," + clus_avevol[i]).ToString());
                }
            }

            var clus_num = new int[clusters.Count];
            for (int i = 0; i < TickData.vola_500.Count; i++)
            {
                for (int j = 0; j < clusters.Count; j++)
                {
                    if (clusters[j] == (clus_vola[i] + "," + clus_makairi[i] + "," + clus_avevol[i]).ToString())
                    {
                        res.Add(j);
                        clus_num[j]++;
                    }
                }
            }
            
            return res;
        }


        public static (List<double> hist_class, int[] hist_num) makeHistogram(List<double> data, int start_ind, int num_class)
        {
            var hist_class = new List<double>();
            var hist_num = new int[num_class];

            double max = -999999;
            for (int i = start_ind; i < data.Count - 1; i++)
                max = (data[i] > max) ? data[i] : max;

            double min = 999999999;
            for (int i = start_ind; i < data.Count - 1; i++)
                min = (data[i] < min) ? data[i] : min;

            //make hist class
            double bar = (max - min) / (double)num_class;
            for (int i = 1; i <= num_class; i++)
                hist_class.Add((bar * i) + min);

            //count hist num
            foreach (var v in data)
            {
                if (v <= hist_class[0])
                    hist_num[0]++;
                else
                {
                    for (int i = 1; i < hist_class.Count; i++)
                    {
                        if (v <= hist_class[i] && v > hist_class[i - 1])
                            hist_num[i]++;
                    }
                }
            }
            return (hist_class, hist_num);
        }

        public static void writeHistogram(List<double> hist_class, int[] hist_num)
        {
            using (StreamWriter sw = new StreamWriter("./histogram.csv", false, Encoding.Default))
            {
                try
                {
                    sw.WriteLine("hist class, hist num");
                    for (int i = 0; i < hist_class.Count; i++)
                    {
                        sw.WriteLine(hist_class[i] + "," + hist_num[i]);
                    }
                }
                catch (Exception e)
                {

                }
            }
        }


        private static double calcTotalVola(List<double> data, int start_ind)
        {
            for (int i = 0; i < start_ind; i++)
            {
                data.RemoveAt(i);
            }
            double ave = data.Average();
            double sum_diff = 0;
            for (int i = 0; i < data.Count; i++)
            {
                sum_diff += Math.Pow(ave - data[i], 2);
            }
            return sum_diff / data.Count;
        }

        //num_splitter should be higher than 1
        private static List<int> makeCluster2(List<double> moto_data, int start_ind, int num_splitter)
        {
            var res = new List<int>();
            var data = moto_data;
            data.Sort();

            var cluster_kijun = new double[num_splitter];
            double kijun = (double)data.Count / (double)(num_splitter + 1);
            for (int i = 0; i < num_splitter; i++)
                cluster_kijun[i] = data[Convert.ToInt32(Math.Truncate((i + 1) * kijun))];

            for (int i = 0; i < moto_data.Count; i++)
            {
                for (int j = 0; j < num_splitter; j++)
                {
                    if (j == 0)
                    {
                        if (moto_data[i] <= cluster_kijun[j])
                        {
                            res.Add(0);
                        }
                    }
                    else
                    {
                        if (moto_data[i] > cluster_kijun[j - 1] && moto_data[i] <= cluster_kijun[j])
                        {
                            res.Add(j);
                        }
                    }
                }
                if (moto_data[i] > cluster_kijun[num_splitter-1])
                {
                    res.Add(num_splitter);
                }
            }

            return res;
        }

        private static List<int> makeCluster(List<double> moto_data, int start_ind, List<double> hist_class, int[] hist_num, double cluster_percentage)
        {
            var res = new List<int>();

            int num_cluster = (1.0 / cluster_percentage - Math.Truncate(1.0 / cluster_percentage) > 0) ? Convert.ToInt32(Math.Truncate(1.0 / cluster_percentage)) + 1 : Convert.ToInt32(Math.Truncate(1.0 / cluster_percentage));

            //calc cluster kijun
            int sum = hist_num.Sum();
            int current_sum = 0;
            int n = 0;
            int n_c = 0;
            var cluster_kijun = new double[num_cluster];
            while (n_c < num_cluster)
            {
                current_sum += hist_num[n];
                if (sum * n_c * cluster_percentage <= current_sum)
                {
                    cluster_kijun[n_c] = hist_class[n];
                    n_c++;
                }
                n++;
            }

            for (int i = 0; i < moto_data.Count; i++)
            {
                for (int j = 0; j < num_cluster; j++)
                {
                    if (j == 0)
                    {
                        if (moto_data[i] <= cluster_kijun[j])
                            res.Add(j);
                    }
                    else
                    {
                        if (moto_data[i] > cluster_kijun[j - 1] && moto_data[i] <= cluster_kijun[j])
                            res.Add(j);
                    }
                }
                if (moto_data[i] > cluster_kijun[num_cluster - 1])
                    res.Add(num_cluster);
            }
            return res;
        }
    }
}
