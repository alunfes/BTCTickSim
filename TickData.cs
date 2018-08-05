using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BTCTickSim
{
    class TickData
    {
        public static List<DateTime> time;
        public static List<double> price;
        public static List<double> volume;

        public static List<double> speed_2500;
        public static List<double> speed_1000;
        public static List<double> speed_500;
        public static List<double> ave_vol_2500;
        public static List<double> ave_vol_1000;
        public static List<double> ave_vol_500;
        public static List<double> vola_2500;
        public static List<double> vola_1000;
        public static List<double> vola_500;

        public static void initialize()
        {
            time = new List<DateTime>();
            price = new List<double>();
            volume = new List<double>();

            speed_2500 = new List<double>();
            speed_1000 = new List<double>();
            speed_500 = new List<double>();
            ave_vol_2500 = new List<double>();
            ave_vol_1000 = new List<double>();
            ave_vol_500 = new List<double>();
            vola_2500 = new List<double>();
            vola_1000 = new List<double>();
            vola_500 = new List<double>();
        }

        public static void readTickData()
        {
            if (TickData.price == null)
            {
                TickData.initialize();

                using (System.IO.StreamReader sr = new System.IO.StreamReader("tick.csv", Encoding.UTF8, false))
                {
                    try
                    {
                        Form1.Form1Instance.setLabel("reading data");
                        int num = 0;
                        foreach (var line in File.ReadLines("tick.csv"))
                        {
                            var e = line.Split(',');
                            TickData.time.Add(FromUnixTime(Convert.ToInt64(e[0])));
                            TickData.price.Add(Convert.ToDouble(e[1]));
                            TickData.volume.Add(Convert.ToDouble(e[2]));
                            num++;
                        }
                        Form1.Form1Instance.setLabel("Tick Data: from-" + time[0].ToString() + ", to-" + time[time.Count - 1].ToString() + ", Num=" + num.ToString());
                    }
                    catch (Exception e)
                    {
                        System.Windows.Forms.MessageBox.Show(e.ToString());
                    }
                }

                calcAveVolAll();
                calcSpeedAll();
                calcVolaAll();
            }
        }

        private static DateTime FromUnixTime(long unixTime)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTime).LocalDateTime;
        }

        private static void calcVolaAll()
        {
            //vola_2500= calcVola(2500);
            //vola_1000 = calcVola(1000);
            vola_500 = calcVola(500);
        }
        private static List<double> calcVola(int term)
        {
            double sum = 0;
            double ave = 0;
            List<double> res = new List<double>();
            for(int k=0; k<term; k++)
            {
                sum += TickData.price[k];
                res.Add(0);
            }
            ave = sum / (double)term;
            res.RemoveAt(res.Count - 1);

            double sum_diff = 0;
            for(int i=0; i<term; i++)
            {
                sum_diff += Math.Pow((TickData.price[i] - ave), 2);
            }
            res.Add(Math.Pow(sum_diff / (double)term, 0.5));

            double new_ave = 0;
            double new_sum_diff = 0;
            for(int i=term; i<TickData.price.Count-1;i++)
            {
                new_ave = ((ave * term) + TickData.price[i] - TickData.price[i - term]) / (double)term;
                new_sum_diff = Math.Pow(TickData.price[i] - new_ave, 2) + sum_diff - Math.Pow(TickData.price[i - term] - ave, 2);
                res.Add(Math.Pow(new_sum_diff / (double)term, 0.5));
                ave = new_ave;
                sum_diff = new_sum_diff;
            }
            return res;
        }

        private static void calcSpeedAll()
        {
            speed_500 = calcSpeed(500);
            //speed_1000 = calcSpeed(1000);
            //speed_2500 = calcSpeed(2500);
        }
        private static List<double> calcSpeed(int term)
        {
            List<double> res = new List<double>();
            for(int i=0; i<term; i++)
            {
                res.Add(0);
            }
            res.RemoveAt(res.Count - 1);

            for(int i=term; i<TickData.price.Count-1; i++)
            {
                res.Add((TickData.time[i] - TickData.time[i - term]).TotalSeconds / (double)term);
            }
            return res;
        }

        private static void calcAveVolAll()
        {
            ave_vol_500 = calcAveVol(500);
            //ave_vol_1000 = calcAveVol(1000);
            //ave_vol_2500 = calcAveVol(2500);
        }
        private static List<double> calcAveVol(int term)
        {
            List<double> res = new List<double>();
            double sum = 0;
            for(int k=0; k<term; k++)
            {
                sum += TickData.price[k] * TickData.volume[k];
                res.Add(0);
            }
            res.RemoveAt(res.Count - 1);

            for(int j=0; j<TickData.price.Count - term - 1; j++)
            {
                res.Add(sum / (double)term);
                sum = sum - (TickData.price[j] * TickData.volume[j]) + (TickData.price[j + term] * TickData.volume[j + term]);
            }
            return res;
        }

    }
}
