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

        public static void initialize()
        {
            time = new List<DateTime>();
            price = new List<double>();
            volume = new List<double>();
        }

        public static void readTickData()
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
                    Form1.Form1Instance.setLabel("Tick Data: from-" + time[0].ToString() + ", to-"+time[time.Count-1].ToString()+", Num="+num.ToString());
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(e.ToString());
                }
            }
        }

        private static DateTime FromUnixTime(long unixTime)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTime).LocalDateTime;
        }
    }
}
