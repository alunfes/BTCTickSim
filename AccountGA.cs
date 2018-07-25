using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTCTickSim
{
    class AccountGA
    {
        private double order_time_lag = 1.0;

        private int start_ind;
        private int end_ind;

        public double pl;
        public double cum_pl;
        public int num_trade;
        public double win_rate;
        public double ave_pl;
        public double pl_per_min;

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
        

        public AccountGA()
        {
            start_ind = 999999999;
            end_ind = 0;

            pl = 0;
            cum_pl = 0;
            num_trade = 0;
            win_rate = 0;
            ave_pl = 0;
            pl_per_min = 0;

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

        
        public void moveToNext(int i)
        {
            checkExecution(i);
            checkCancel(i);
            updatePriceTracingOrder(i);
            pl = calcPL(i);
            start_ind = (i < start_ind) ? i : start_ind;
        }

        public void lastDayOperation(int i)
        {
            checkExecution(i);
            checkCancel(i);
            pl = calcPL(i);
            ave_pl = cum_pl / (double)num_trade;
            win_rate = win_rate / (double)num_trade;
            end_ind = i;
            pl_per_min = cum_pl / Convert.ToDouble((TickData.time[end_ind] - TickData.time[start_ind]).Minutes);
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
                unexe_cancel.Add(false);
            }
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
                    if ((TickData.time[i] - price_tracing_order_dt).Seconds >= order_time_lag * 2)
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
        }

        public void cancelAllOrders(int i)
        {
            if (unexe_position.Count > 0)
            {
                cancel_all_orders = true;
                cancel_all_order_time = TickData.time[i];
                cancel_all_order_i = i;
            }
        }

        public void cancelOrder(int i, int index)
        {
            unexe_cancel[index] = true;
        }

        private void checkExecution(int i)//cancel中でも約定判定すべき
        {
            for (int j = 0; j < unexe_position.Count; j++)
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

        private void checkCancel(int i)
        {
            if (cancel_all_orders)
            {
                if ((TickData.time[i] - cancel_all_order_time).Seconds >= order_time_lag)
                    executeCancelAllOrders(i);
            }
            else
            {
                string cancelled_index = "";
                for (int j = 0; j < unexe_cancel.Count; j++)
                {
                    if (unexe_cancel[j] && (TickData.time[i] - unexe_time[j]).Seconds >= order_time_lag)
                    {
                        removeUnexeInd(j);
                        cancelled_index += j.ToString() + ",";
                    }
                }
            }
        }

        private void execute(int i, int unexe_ind)
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
                        holding_position = "Short";
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
                    ave_holding_lot += lot;
                    holding_position = "Short";
                    last_entry_i = i;
                    last_entry_time = TickData.time[i];
                }
            }

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
        }

        private void updateCumPL(int i, int unexe_ind, double price, double lot)
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
            num_trade++;
            if (pl > 0)
                win_rate++;
            cum_pl += pl;
        }
    }
}
