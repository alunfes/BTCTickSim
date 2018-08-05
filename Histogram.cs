using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTCTickSim
{
    class Histogram
    {
        public static (List<double> hist_class, int[] hist_num) makeHistogram(List<double> data, int start_ind, int num_class)
        {
            var hist_class = new List<double>();
            var hist_num = new int[num_class];

            double max = 0;
            for (int i = start_ind; i < data.Count - 1; i++)
                max = (data[i] > max) ? data[i] : max;

            double min = 999999999;
            for (int i = start_ind; i < data.Count - 1; i++)
                min = (data[i] < min) ? data[i] : min;

            //make hist class
            double bar = (max - min) / (double)num_class;
            for (int i = 1; i <= num_class; i++)
                hist_class.Add(bar * i);

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
}
}
