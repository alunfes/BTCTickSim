﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BTCTickSim
{
    class Account2
    {
        private double order_time_lag = 1.0;

        public int start_ind;
        public int end_ind;

        public int[] fired_box_ind_num;
        public int num_box;

        public double pl;
        public double cum_pl;
        public int num_trade;
        public double win_rate;
        public double ave_pl;
        public double pl_per_min;
        public double pl_vola;
        public double total_pl_vola;
        public double profit_factor;
        public double sharp_ratio;
        public List<double> quarter_performance;
        public double num_trade_per_hour;
        public double[] cum_pl_fired_box;

        public List<double> unexe_price;
        public List<double> unexe_lot;
        public List<string> unexe_position; //Long, Short
        public List<DateTime> unexe_time;
        public List<int> unexe_i;
        public List<bool> unexe_cancel;

        public double price_tracing_order_target_lot;
        public string price_tracing_order_position;
        public DateTime price_tracing_order_dt;
        public int price_tracing_order_i;
        public bool price_tracing_order_flg;

        public bool cancel_all_orders;
        public DateTime cancel_all_order_time;
        public int cancel_all_order_i;

        public double ave_holding_price;
        public double ave_holding_lot;
        public DateTime last_entry_time;
        public int last_entry_i;
        public string holding_position; //Long, Short, None

        //log
        public Dictionary<int, double> total_pl_log;
        private Dictionary<int, double> pl_log;
        private Dictionary<int, double> cum_pl_log;
        private Dictionary<int, string> position_log;
        private Dictionary<int, double> holding_price_log;
        private Dictionary<int, double> lot_log;
        private Dictionary<string, string> action_log2;
        private int action_log_num;

        public Account2(Chrome2 chro)
        {
            start_ind = 999999999;
            end_ind = 0;
            this.num_box = chro.num_box;
            fired_box_ind_num = new int[num_box];
            cum_pl_fired_box = new double[num_box];

            pl = 0;
            cum_pl = 0;
            num_trade = 0;
            win_rate = 0;
            ave_pl = 0;
            pl_per_min = 0;
            pl_vola = 0;
            total_pl_vola = 0;
            profit_factor = 0;
            quarter_performance = new List<double>();
            num_trade_per_hour = 0;
            sharp_ratio = 0;

            pl_log = new Dictionary<int, double>();
            cum_pl_log = new Dictionary<int, double>();
            position_log = new Dictionary<int, string>();
            lot_log = new Dictionary<int, double>();
            holding_price_log = new Dictionary<int, double>();
            action_log2 = new Dictionary<string, string>();
            total_pl_log = new Dictionary<int, double>();
            action_log_num = 0;

            initializeUnexeData();
            initializeCancelAllData();
            initializeHoldingData();
            initializePriceTracingOrder();
        }

        private void initializeUnexeData()
        {
            unexe_lot = new List<double>();
            unexe_position = new List<string>();
            unexe_price = new List<double>();
            unexe_time = new List<DateTime>();
            unexe_i = new List<int>();
            unexe_cancel = new List<bool>();
        }

        private void initializeCancelAllData()
        {
            cancel_all_orders = false;
            cancel_all_order_time = new DateTime();
            cancel_all_order_i = -1;
        }

        private void initializeHoldingData()
        {
            ave_holding_lot = new double();
            ave_holding_price = new double();
            holding_position = "None";
            last_entry_time = new DateTime();
            last_entry_i = 0;
        }
        private void initializePriceTracingOrder()
        {
            price_tracing_order_target_lot = 0;
            price_tracing_order_position = "None";
            price_tracing_order_dt = new DateTime();
            price_tracing_order_i = -1;
            price_tracing_order_flg = false;
        }

        private void removeUnexeInd(int unexe_ind)
        {
            unexe_position.RemoveAt(unexe_ind);
            unexe_price.RemoveAt(unexe_ind);
            unexe_lot.RemoveAt(unexe_ind);
            unexe_time.RemoveAt(unexe_ind);
            unexe_i.RemoveAt(unexe_ind);
            unexe_cancel.RemoveAt(unexe_ind);
        }


        public void moveToNext(int i, int fired_box_ind)
        {
            checkExecution(i, fired_box_ind);
            checkCancel(i);
            updatePriceTracingOrder(i);
            pl = calcPL(i);
            pl_log.Add(i, pl);
            total_pl_log.Add(i, pl + cum_pl);
            if (cum_pl_log.ContainsKey(i) == false) cum_pl_log.Add(i, cum_pl);
            position_log.Add(i, holding_position);
            lot_log.Add(i, ave_holding_lot);
            holding_price_log.Add(i, ave_holding_price);
            start_ind = (i < start_ind) ? i : start_ind;
            action_log_num = 0;
        }

        public void lastDayOperation(int i, Chrome2 chro, int fired_box_ind, bool writelog)
        {
            checkExecution(i, fired_box_ind);
            checkCancel(i);
            pl = calcPL(i);
            pl_log.Add(i, pl);
            position_log.Add(i, holding_position);
            lot_log.Add(i, ave_holding_lot);
            total_pl_log.Add(i, pl + cum_pl);

            ave_pl = cum_pl / (double)num_trade;
            win_rate = win_rate / (double)num_trade;
            end_ind = i;
            num_trade_per_hour = Convert.ToDouble(num_trade) / (TickData.time[end_ind] - TickData.time[start_ind]).TotalHours;
            pl_per_min = (num_trade_per_hour > 0) ? total_pl_log.Values.ToList()[total_pl_log.Values.ToList().Count - 1] / Convert.ToDouble((TickData.time[end_ind] - TickData.time[start_ind]).TotalMinutes) : 0;
            sharp_ratio = (pl_vola > 1) ? total_pl_log[total_pl_log.Count - 1] / pl_vola : 0;

            profit_factor = calcProfitFactor();
            calcQuarterPerformance(4);
            pl_vola = calcPLVolatility();
            total_pl_vola = calcTotalPLVola();

            for (int j = 0; j < fired_box_ind_num.Length; j++)
                fired_box_ind_num[j] = chro.box_fired_num[j];
            
            if (writelog)
                writeLog2();
        }

        public void takeActionLog(int i, string v)
        {
            action_log2.Add(i.ToString() + "-" + action_log_num.ToString(), v);
            action_log_num++;
        }

        public double calcPL(int i)
        {
            return (holding_position == "Long") ? (TickData.price[i] - ave_holding_price) * ave_holding_lot :
                (ave_holding_price - TickData.price[i]) * ave_holding_lot;
        }

        private double calcProfitFactor()
        {
            if (num_trade > 0)
            {
                var list = cum_pl_log.Values.ToList();
                List<double> pl = new List<double>();
                for (int i = 1; i < list.Count; i++)
                {
                    if (list[i] - list[i - 1] != 0)
                        pl.Add(list[i] - list[i - 1]);
                }

                double plus = 0;
                double minus = 0;
                foreach (var v in pl)
                {
                    if (v > 0)
                        plus += v;
                    else
                        minus += v;
                }
                return (minus != 0) ? plus / (-minus) : 0;
            }
            else
                return 0;
        }

        private double calcPLVolatility()
        {
            if (num_trade > 5)
            {
                var list = cum_pl_log.Values.ToList();
                List<double> pl = new List<double>();
                for (int i = 1; i < list.Count; i++)
                {
                    if (list[i] - list[i - 1] != 0)
                        pl.Add(list[i] - list[i - 1]);
                }

                if (pl.Count > 5)
                {
                    double ave = pl.Average();
                    double sum_diff = 0;
                    foreach (var v in pl)
                        sum_diff += Math.Pow(ave - v, 2);
                    return Math.Pow(sum_diff / (double)num_trade, 0.5);
                }
                else
                    return 9999;
            }
            else
                return 9999;
        }

        private double calcTotalPLVola()
        {
            if (num_trade >= 3)
            {
                var list = total_pl_log.Values.ToList();
                List<double> pl = new List<double>();
                for (int i = 1; i < list.Count; i++)
                {
                    if (list[i] - list[i - 1] != 0)
                        pl.Add(list[i] - list[i - 1]);
                }
                double ave = pl.Average();
                double sum_diff = 0;
                foreach (var v in pl)
                    sum_diff += Math.Pow(ave - v, 2);
                return Math.Pow(sum_diff / (double)num_trade, 0.5);
            }
            else
                return 9999;
        }

        private void calcQuarterPerformance(int numq)
        {
            var list = total_pl_log.Values.ToList();
            double num_tick_q = (double)list.Count / (double)numq;

            int start = 0;
            int end = (int)Math.Truncate(num_tick_q);
            for (int i = 0; i < numq - 1; i++)
            {
                quarter_performance.Add(list[end] - list[start]);
                start = end;
                end += (int)Math.Truncate(num_tick_q);
            }
            quarter_performance.Add(list[list.Count - 1] - list[list.Count - 1 - (int)Math.Truncate(num_tick_q)]);
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
                unexe_cancel.Add(false);
                takeActionLog(i, "Entry Order for " + position + " @" + price.ToString() + " x " + lot.ToString());
            }
        }

        public void exitAllOrder(int i)
        {
            string position = (holding_position == "Long") ? "Short" : "Long";
            entryPriceTracingOrder(i, position, ave_holding_lot);
            takeActionLog(i, "Exit all");
        }


        /********************************************************************
         * cancelall orders when entry to price tracing order
         * do nothing when date - dt < timelag
         * do nothing when cancelling order
         * cancel all orders when date - dt >= timelag && unexe.Count >0
         * entry with update price and minus from target_lot when date - dt >= timelag && unexe.Count ==0
         * 
         * 
         * ******************************************************************/
        public void entryPriceTracingOrder(int i, string position, double target_lot)
        {
            price_tracing_order_dt = TickData.time[i];
            price_tracing_order_position = position;
            price_tracing_order_i = i;
            price_tracing_order_flg = true;
            price_tracing_order_target_lot = target_lot;
            cancelAllOrders(i);
            takeActionLog(i, "Started Price Tracing Order for " + position + " x" + target_lot.ToString());
        }


        private void updatePriceTracingOrder(int i)
        {
            if (unexe_position.Count == 0 && price_tracing_order_target_lot == 0) //finished price tracing order
            {
                initializePriceTracingOrder();
            }
            else if (price_tracing_order_flg && cancel_all_orders == false)
            {
                if (unexe_position.Count == 0)
                {
                    price_tracing_order_dt = TickData.time[i];
                    double opt_lot = calcLotForPriceTracingOrder(i);
                    entryOrder(i, price_tracing_order_position, calcPriceForPriceTracingOrder(i), opt_lot);
                    price_tracing_order_target_lot -= opt_lot; //temporary minus order lot from target_lot
                    price_tracing_order_i = i;
                }
                else
                {
                    if ((TickData.time[i] - price_tracing_order_dt).TotalSeconds >= order_time_lag * 2)
                        cancelAllOrders(i);
                }
            }
        }

        private double calcPriceForPriceTracingOrder(int i)
        {
            double res = 0;
            if (price_tracing_order_position == "Long")
            {
                res = Math.Min(TickData.price[i], TickData.price[i - 1]) + Math.Abs(TickData.price[i] - TickData.price[i - 1]) * 0.2;
            }
            else if (price_tracing_order_position == "Short")
            {
                res = Math.Max(TickData.price[i], TickData.price[i - 1]) - Math.Abs(TickData.price[i] - TickData.price[i - 1]) * 0.2;
            }
            return Math.Round(res);
        }

        private double calcLotForPriceTracingOrder(int i)
        {
            return (price_tracing_order_target_lot > 0.05) ? 0.05 : price_tracing_order_target_lot;
        }

        public void cancelPriceTracingOrder(int i)
        {
            cancelAllOrders(i);
            initializePriceTracingOrder();
            takeActionLog(i, "Stopped Price Tracing Order");
        }

        public void cancelAllOrders(int i)
        {
            if (unexe_position.Count > 0)
            {
                cancel_all_orders = true;
                cancel_all_order_time = TickData.time[i];
                cancel_all_order_i = i;
                takeActionLog(i, "Cancelling All Orders");
            }
        }

        public void cancelOrder(int i, int index)
        {
            unexe_cancel[index] = true;
            takeActionLog(i, "Cancelling order #" + index.ToString());
        }

        private void checkExecution(int i, int fired_box_ind)//cancel中でも約定判定すべき
        {
            for (int j = 0; j < unexe_position.Count; j++)
            {
                if ((TickData.time[i] - unexe_time[j]).TotalSeconds >= order_time_lag)
                {
                    if (unexe_position[j] == "Long")
                    {
                        if (TickData.price[i] <= unexe_price[j])
                            execute(i, j, fired_box_ind);
                    }
                    else if (unexe_position[j] == "Short")
                    {
                        if (TickData.price[i] >= unexe_price[j])
                            execute(i, j, fired_box_ind);
                    }
                }
            }
        }

        private void checkCancel(int i)
        {
            if (cancel_all_orders)
            {
                if ((TickData.time[i] - cancel_all_order_time).TotalSeconds >= order_time_lag)
                    executeCancelAllOrders(i);
            }
            else
            {
                string cancelled_index = "";
                for (int j = 0; j < unexe_cancel.Count; j++)
                {
                    if (unexe_cancel[j] && (TickData.time[i] - unexe_time[j]).TotalSeconds >= order_time_lag)
                    {
                        removeUnexeInd(j);
                        cancelled_index += j.ToString() + ",";
                    }
                }
                if (cancelled_index != "")
                    takeActionLog(i, "Cancelled orders #" + cancelled_index);
            }
        }

        private void execute(int i, int unexe_ind, int fired_box_ind)//unexe_ind : index of executed unexecuted order
        {
            double lot = (TickData.volume[i] >= unexe_lot[unexe_ind]) ? unexe_lot[unexe_ind] : TickData.volume[i]; // executable lot

            if (holding_position == "None")
            {
                holding_position = unexe_position[unexe_ind];
                ave_holding_lot += lot;
                ave_holding_price = unexe_price[unexe_ind];
                last_entry_time = TickData.time[i];
                last_entry_i = i;
            }
            else if (holding_position == "Long")
            {
                if (unexe_position[unexe_ind] == "Long")
                {
                    ave_holding_price = ((ave_holding_price * ave_holding_lot) + (unexe_price[unexe_ind] * lot)) / (ave_holding_lot + lot);
                    ave_holding_lot += lot;
                    holding_position = "Long";
                    last_entry_i = i;
                    last_entry_time = TickData.time[i];
                }
                else if (unexe_position[unexe_ind] == "Short")
                {
                    if (lot < ave_holding_lot)
                    {
                        ave_holding_lot -= lot;
                        updateCumPL(i, unexe_ind, unexe_price[unexe_ind], lot, fired_box_ind);
                    }
                    else if (lot > ave_holding_lot)
                    {
                        ave_holding_price = unexe_price[unexe_ind];
                        ave_holding_lot = lot - ave_holding_lot;
                        holding_position = "Short";
                        last_entry_i = i;
                        last_entry_time = TickData.time[i];
                        updateCumPL(i, unexe_ind, unexe_price[unexe_ind], lot, fired_box_ind);
                    }
                    else if (unexe_lot[unexe_ind] == ave_holding_lot)
                    {
                        updateCumPL(i, unexe_ind, unexe_price[unexe_ind], lot, fired_box_ind);
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
                        holding_position = "Short";
                        ave_holding_lot -= lot;
                        updateCumPL(i, unexe_ind, unexe_price[unexe_ind], lot, fired_box_ind);
                    }
                    else if (lot > ave_holding_lot)
                    {
                        ave_holding_price = unexe_price[unexe_ind];
                        ave_holding_lot = lot - ave_holding_lot;
                        holding_position = "Long";
                        last_entry_i = i;
                        last_entry_time = TickData.time[i];
                        updateCumPL(i, unexe_ind, unexe_price[unexe_ind], lot, fired_box_ind);
                    }
                    else if (lot == ave_holding_lot)
                    {
                        updateCumPL(i, unexe_ind, unexe_price[unexe_ind], lot, fired_box_ind);
                        initializeHoldingData();
                    }
                }
                else if (unexe_position[unexe_ind] == "Short")
                {
                    ave_holding_price = ((ave_holding_price * ave_holding_lot) + (unexe_price[unexe_ind] * lot)) / (ave_holding_lot + lot);
                    ave_holding_lot += lot;
                    holding_position = "Short";
                    last_entry_i = i;
                    last_entry_time = TickData.time[i];
                }
            }
            takeActionLog(i, "Executed " + unexe_position[unexe_ind] + " @" + unexe_price[unexe_ind].ToString() + " x " + lot.ToString());

            if (lot >= unexe_lot[unexe_ind])
                removeUnexeInd(unexe_ind);
            else
                unexe_lot[unexe_ind] -= lot;


        }


        private void executeCancel(int i, int index)
        {
            if (price_tracing_order_flg)
                price_tracing_order_target_lot += unexe_lot[index];
            removeUnexeInd(index);
            takeActionLog(i, "Cancelled order #" + index.ToString());
        }

        private void executeCancelAllOrders(int i)
        {
            if (price_tracing_order_flg)
            {
                foreach (double v in unexe_lot)
                    price_tracing_order_target_lot += v;
            }
            initializeUnexeData();
            initializeCancelAllData();
            takeActionLog(i, "Cancelled all orders");
        }

        private void updateCumPL(int i, int unexe_ind, double price, double lot, int fired_box_ind)
        {
            double pl = 0;
            if (holding_position == "Long")
            {
                if (unexe_position[unexe_ind] == "Short")
                {
                    pl = (price - ave_holding_price) * lot;
                }
            }
            else if (holding_position == "Short")
            {
                if (unexe_position[unexe_ind] == "Long")
                {
                    pl = (ave_holding_price - price) * lot;
                }
            }

            if (pl == 0)
                pl = 0;

            num_trade++;
            if (pl > 0)
                win_rate++;
            cum_pl += pl;
            cum_pl_log.Add(i, cum_pl);

            if (fired_box_ind >= 0)
                cum_pl_fired_box[fired_box_ind] += pl;
        }

        public void writeLog2()
        {
            string path = @"./sim_result.csv";
            using (StreamWriter sw = new StreamWriter(path, false, Encoding.Default))
            {
                sw.WriteLine("num trade," + num_trade.ToString());
                sw.WriteLine("cum pl," + cum_pl.ToString());
                sw.WriteLine("pl per min," + pl_per_min.ToString());
                sw.WriteLine("win rate," + win_rate.ToString());
                sw.WriteLine("ave pl," + ave_pl.ToString());
                sw.WriteLine("num trade per hour," + num_trade_per_hour.ToString());
                sw.WriteLine("pl vola," + pl_vola.ToString());
                sw.WriteLine("total pl vola," + total_pl_vola.ToString());
                sw.WriteLine("profit_factor," + profit_factor.ToString());
                sw.WriteLine("quarter performance," + quarter_performance.Select(x => x.ToString() + ","));
                sw.WriteLine("i,DateTime,Tick,Size,pl,cum pl,total pl,position,ave holding price,holding lot,action log");

                for (int i = start_ind; i <= end_ind; i++)
                {
                    string line = i.ToString() + "," + TickData.time[i].ToString() + "," + TickData.price[i] + "," + TickData.volume[i] + ",";

                    line += pl_log.ContainsKey(i) ? pl_log[i] + "," : " ,";
                    line += cum_pl_log.ContainsKey(i) ? cum_pl_log[i] + "," : " ,";
                    line += total_pl_log.ContainsKey(i) ? total_pl_log[i] + "," : " ,";
                    line += position_log.ContainsKey(i) ? position_log[i] + "," : " ,";
                    line += holding_price_log.ContainsKey(i) ? holding_price_log[i] + "," : " ,";
                    line += lot_log.ContainsKey(i) ? lot_log[i] + "," : " ,";

                    int n = 0;
                    while (action_log2.ContainsKey(i.ToString() + "-" + n.ToString()))
                    {
                        line += action_log2[i.ToString() + "-" + n.ToString()] + ",";
                        //line += (n > 0) ? Environment.NewLine : "";
                        n++;
                    }
                    sw.WriteLine(line);
                }
            }
            Form1.Form1Instance.setLabel("Completed write sim result");
        }
    }
}
