using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BTCTickSim
{
    class Account
    {
        private double order_time_lag = 1.5;

        public double pl;
        public double cum_pl;
        public int num_trade;
        public double win_rate;
        public double ave_pl;

        public List<double> unexe_price;
        public List<double> unexe_lot;
        public List<string> unexe_position; //Long, Short
        public List<DateTime> unexe_time;
        public List<int> unexe_i;

        public bool exit_all_tracing_price;


        public bool cancel_all_orders;
        public DateTime cancel_time;
        public int cancel_i;

        public double ave_holding_price;
        public double ave_holding_lot;
        public DateTime last_entry_time;
        public int last_entry_i;
        public string holding_position; //Long, Short, None

        //log
        private Dictionary<int, double> pl_log;
        private Dictionary<int, double> cum_pl_log;
        private Dictionary<int, string> position_log;
        private Dictionary<int, double> holding_price_log;
        private Dictionary<int, double> lot_log;
        private Dictionary<int, string> action_log;

        public Account()
        {
            pl = 0;
            cum_pl = 0;
            num_trade = 0;
            win_rate = 0;
            ave_pl = 0;

            initializeUnexeData();
            initializeCancelData();
            initializeHoldingData();

            exit_all_tracing_price = false;

            pl_log = new Dictionary<int, double>();
            cum_pl_log = new Dictionary<int, double>();
            position_log = new Dictionary<int, string>();
            lot_log = new Dictionary<int, double>();
            holding_price_log = new Dictionary<int, double>();
            action_log = new Dictionary<int, string>();
        }

        private void initializeUnexeData()
        {
            unexe_lot = new List<double>();
            unexe_position = new List<string>();
            unexe_price = new List<double>();
            unexe_time = new List<DateTime>();
            unexe_i = new List<int>();
        }

        private void initializeCancelData()
        {
            cancel_all_orders = false;
            cancel_time = new DateTime();
            cancel_i = 0;
        }

        private void initializeHoldingData()
        {
            ave_holding_lot = new double();
            ave_holding_price = new double();
            holding_position = "None";
            last_entry_time = new DateTime();
            last_entry_i = 0;
        }

        private void removeUnexeInd(int unexe_ind)
        {
            unexe_position.RemoveAt(unexe_ind);
            unexe_price.RemoveAt(unexe_ind);
            unexe_lot.RemoveAt(unexe_ind);
            unexe_time.RemoveAt(unexe_ind);
            unexe_i.RemoveAt(unexe_ind);
        }

        public void moveToNext(int i)
        {
            checkExecution(i);
            pl = calcPL(i);
            pl_log.Add(i, pl);
            position_log.Add(i, holding_position);
            lot_log.Add(i, ave_holding_lot);
            holding_price_log.Add(i, ave_holding_price);
        }

        public void lastDayOperation(int i, bool writelog)
        {
            checkExecution(i);
            pl = calcPL(i);
            pl_log.Add(i, pl);
            position_log.Add(i, holding_position);
            lot_log.Add(i, ave_holding_lot);
            ave_pl = cum_pl / (double)num_trade;
            win_rate = win_rate / (double)num_trade;
            if (writelog)
                writeLog();
        }

        public double calcPL(int i)
        {
            return (holding_position == "Long") ? (TickData.price[i] - ave_holding_price) * ave_holding_lot :
                (ave_holding_price - TickData.price[i]) * ave_holding_lot;
        }

        public void entryOrder(int i, string position, double price, double lot)
        {
            if (cancel_all_orders == false)
            {
                unexe_position.Add(position);
                unexe_price.Add(price);
                unexe_lot.Add(lot);
                unexe_time.Add(TickData.time[i]);
                unexe_i.Add(i);
                action_log.Add(i, "Placed Order for " + position + " @" + price.ToString()+ " x "+ lot.ToString());
            }
        }

        public void exitAllTracingTickPrice(int i)
        {
            initializeCancelData();
            initializeUnexeData();
            exit_all_tracing_price = true;
        }

        public void cancelAllOrders(int i)
        {
            if (unexe_position.Count > 0)
            {
                cancel_all_orders = true;
                cancel_time = TickData.time[i];
                cancel_i = i;
                action_log.Add(i, "Cancelling All Orders");
            }
        }
        private void checkExecution(int i)
        {
            if (cancel_all_orders == false)
            {
                for (int j = 0; j < unexe_position.Count - 1; j++)
                {
                    if ((TickData.time[i] - unexe_time[j]).Seconds >= order_time_lag)
                    {
                        if (unexe_position[j] == "Long")
                        {
                            if (TickData.price[i] <= unexe_price[j])
                                execute(i, j);
                        }
                        else if (unexe_position[j] == "Short")
                        {
                            if (TickData.price[i] >= unexe_price[j])
                                execute(i, j);
                        }
                    }
                }
            }
            else
            {
                if ((TickData.time[i] - cancel_time).Seconds >= order_time_lag)
                    cancelAllOrders(i);
            }
        }

        private void execute(int i, int unexe_ind)
        {
            double lot = (TickData.volume[i] >= unexe_lot[unexe_ind]) ? unexe_lot[unexe_ind] : TickData.volume[i]; // executable lot
            if (holding_position == "Long")
            {
                if (unexe_position[unexe_ind] == "Long")
                {
                    ave_holding_price = ((ave_holding_price * ave_holding_lot) + (unexe_price[unexe_ind] * lot)) / (ave_holding_lot + lot);
                    ave_holding_lot += lot;
                    last_entry_i = i;
                    last_entry_time = TickData.time[i];
                }
                else if (unexe_position[unexe_ind] == "Short")
                {
                    if (lot < ave_holding_lot)
                    {
                        ave_holding_lot -= lot;
                        updateCumPL(i, unexe_ind, unexe_price[unexe_ind], lot);
                    }
                    else if (lot > ave_holding_lot)
                    {
                        ave_holding_price = unexe_price[unexe_ind];
                        ave_holding_lot = lot - ave_holding_lot;
                        holding_position = "Short";
                        last_entry_i = i;
                        last_entry_time = TickData.time[i];
                        updateCumPL(i, unexe_ind, unexe_price[unexe_ind], lot);
                    }
                    else if (unexe_lot[unexe_ind] == ave_holding_lot)
                    {
                        initializeHoldingData();
                    }

                }
            }
            else if (holding_position == "Short")
            {
                if (unexe_position[unexe_ind] == "Long")
                {
                    if (lot < ave_holding_lot)
                    {
                        ave_holding_lot -= lot;
                        updateCumPL(i, unexe_ind, unexe_price[unexe_ind], lot);
                    }
                    else if (lot > ave_holding_lot)
                    {
                        ave_holding_price = unexe_price[unexe_ind];
                        ave_holding_lot = lot - ave_holding_lot;
                        holding_position = "Long";
                        last_entry_i = i;
                        last_entry_time = TickData.time[i];
                        updateCumPL(i, unexe_ind, unexe_price[unexe_ind], lot);
                    }
                    else if (lot == ave_holding_lot)
                    {
                        updateCumPL(i, unexe_ind, unexe_price[unexe_ind], lot);
                        initializeHoldingData();
                    }
                }
                else if (unexe_position[unexe_ind] == "Short")
                {
                    ave_holding_price = ((ave_holding_price * ave_holding_lot) + (unexe_price[unexe_ind] * lot)) / (ave_holding_lot + lot);
                    ave_holding_lot += unexe_lot[unexe_ind];
                    last_entry_i = i;
                    last_entry_time = TickData.time[i];
                }
            }

            if(lot >= unexe_lot[unexe_ind])
                removeUnexeInd(unexe_ind);
            else
                unexe_lot[unexe_ind] -= lot; 
            action_log.Add(i, "Executed " + unexe_position[unexe_ind] + " @" + unexe_price[unexe_ind].ToString() + " x "+lot.ToString());
        }
        

        private void executeCancel(int i)
        {
            initializeUnexeData();
            initializeCancelData();

            action_log.Add(i, "Cancelled All Orders");
        }

        private void updateCumPL(int i, int unexe_ind, double price, double lot)
        {
            double pl = 0;
            if (holding_position == "Long")
            {
                if (unexe_position[unexe_ind] == "Short")
                {
                    pl = (TickData.price[i] - price) * lot;
                }
            }
            else if (holding_position == "Short")
            {
                if (unexe_position[unexe_ind] == "Long")
                {
                    pl = (price - TickData.price[i]) * lot;
                }
            }
            num_trade++;
            if (pl > 0)
                win_rate++;
            cum_pl += pl;
            cum_pl_log.Add(i, pl);
        }

        public void writeLog()
        {
            string path = @"./sim_result.csv";
            using (StreamWriter sw = new StreamWriter(path, false, Encoding.Default))
            {
                sw.Write("num trade,"+num_trade.ToString());
                sw.Write("cum pl," + cum_pl.ToString());
                sw.Write("win rate," + win_rate.ToString());
                sw.Write("ave pl," + ave_pl.ToString());
                sw.WriteLine("i,DateTime,Tick,Size,pl,cum pl,position,ave holding price,holding lot,action log");

                for (int i = 0; i < TickData.time.Count - 1; i++)
                {
                    string line = i.ToString()+","+TickData.time[i].ToString()+","+TickData.price[i]+","+TickData.volume[i]+",";
                    if (position_log.ContainsKey(i))
                    {
                        line += pl_log[i] + "," + cum_pl_log[i] + "," + position_log[i] + "," + holding_price_log[i] + "," + lot_log[i] + "," + action_log[i];
                        sw.WriteLine(line);
                    }
                }
            }
            Form1.Form1Instance.setLabel("Completed write sim result");
        }
    }
}
