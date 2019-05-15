using Common.Base;
using Interface;
using Interface.DataFormat;
using Interface.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ScalKing.Windows;
using Interface.DataModel;

namespace ScalKing.Controls
{
    class TradeControlViewModel : BaseViewModel
    {
        #region | Enum |
        public enum LogType
        {
            체결, 매수전송, 매도전송
        }
        #endregion

        #region | Properties |
        public override string _id
        {
            get
            {
                return string.Format("{0}({1})", this.AccountModel.StName, this.AccountModel.StCode);
            }
        }

        public TradeStateModel TradeStateModel { get; set; }

        private int purchaseUnfinishedCount;
        public int PurchaseUnfinishedCount
        {
            get { return purchaseUnfinishedCount; }
            set
            {
                purchaseUnfinishedCount = value;
                OnPropertyChanged("PurchaseUnfinishedCount");
            }
        }

        private int sellingUnfinishedCount;
        public int SellingUnfinishedCount
        {
            get { return sellingUnfinishedCount; }
            set
            {
                sellingUnfinishedCount = value;
                OnPropertyChanged("SellingUnfinishedCount");
            }
        }

        private string ruleId;
        public string RuleId
        {
            get { return ruleId; }
            set
            {
                ruleId = value;
                OnPropertyChanged("RuleId");
            }
        }

        private string logMessage;
        public string LogMessage
        {
            get { return logMessage; }
            set
            {
                logMessage = value;
                OnPropertyChanged("LogMessage");
            }
        }

        public AccountModel AccountModel { get; set; }

        private StockCurrentPrice stockCurrentPrice;
        //현재가 정보
        public StockCurrentPrice StockCurrentPrice
        {
            get
            {
                return this.stockCurrentPrice;
            }
            set
            {
                this.stockCurrentPrice = value;
                OnPropertyChanged("StockCurrentPrice");
            }
        }
        #endregion

        #region | Private |
        private KHControlViewModel kHControlViewModel;
        private TradeManagerControlViewModel tradeManagerControlViewModel;
        private Thread tradeThread;
        private AutoResetEvent resetEvent;
        #endregion

        #region | Command |
        private MyCommand close;
        public MyCommand Close
        {
            get
            {
                return close ?? (close = new MyCommand(para =>
                {
                    RemoveThis();
                }));
            }
        }

        private MyCommand runTrade;
        public MyCommand RunTrade
        {
            get
            {
                return runTrade ?? (runTrade = new MyCommand(para =>
                {
                    RunTradeAction(Convert.ToString(para));
                }));
            }
        }
        #endregion

        #region | Events |
        void TradeControlViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsRunning":
                    //if (this.IsRunning)
                    //{
                    //    this.State = TradeState.매수;
                    //    this.resetEvent.Set();
                    //}
                    break;
                default:
                    break;
            }
        }

        void kHControlViewModel_ReceiveCurrentPrice(object sender, Interface.StockCurrentPriceArgs e)
        {
            if (e.StockCurrentPrice.StCode != this.AccountModel.StCode)
            {
                return;
            }

            this.StockCurrentPrice = e.StockCurrentPrice;
            this.AccountModel.StName = e.StockCurrentPrice.StName;
        }

        void kHControlViewModel_ReceiveRealCurrentData(object sender, RealCurrentDataArgs e)
        {
            if (e.RealCurrentData.StCode != this.AccountModel.StCode)
            {
                return;
            }

            StockCurrentPrice stockCurrentPriceArgs = new StockCurrentPrice()
            {
                StCode = e.RealCurrentData.StCode,
                //ChangePrice = Convert.ToInt32(e.RealCurrentData.전일대비),
                //ChangeRate = e.RealCurrentData.등락율,
                //CumulativeVolume = Convert.ToInt64(e.RealCurrentData.누적거래량),
                CurrentPrice = e.RealCurrentData.CurrentPrice
            };

            this.StockCurrentPrice = stockCurrentPriceArgs;

            if (this.AccountModel.HaveStockCount > 0)
            {
                //Re Setting
                //평총, 손익, 손익율
                this.AccountModel.EvalPrice = this.AccountModel.HaveStockCount * stockCurrentPriceArgs.CurrentPrice;
                this.AccountModel.GainLoss = this.AccountModel.EvalPrice - this.AccountModel.PurchaseAmount;
                this.AccountModel.GainLossRate = Math.Round((this.AccountModel.GainLoss / this.AccountModel.PurchaseAmount) * 100, 2);
            }
        }

        void kHControlViewModel_ReceiveOrderConcludInfo(object sender, OrderConcludInfoArgs e)
        {
            if (e.OrderConcludInfo.StCode != this.AccountModel.StCode)
            {
                return;
            }

            Console.WriteLine("체결로그");

            if (e.OrderConcludInfo.OrderDivision.Contains("매수"))
            {
                this.PurchaseUnfinishedCount = e.OrderConcludInfo.UnfinishedCount;
                //var findItem = this.PurchaseUnfinishedCollection.FirstOrDefault(x => x.StCode.Equals(e.OrderConcludInfo.StCode));
                //if (findItem != null)
                //{
                //    this.PurchaseUnfinishedCollection.Remove(findItem);
                //}

                //if (e.OrderConcludInfo.UnfinishedCount != 0)
                //{
                //    this.PurchaseUnfinishedCollection.Add(e.OrderConcludInfo);
                //}
            }
            else
            {
                this.SellingUnfinishedCount = e.OrderConcludInfo.UnfinishedCount;
                //var findItem = this.SellingUnfinishedCollection.FirstOrDefault(x => x.StCode.Equals(e.OrderConcludInfo.StCode));
                //if (findItem != null)
                //{
                //    this.SellingUnfinishedCollection.Remove(findItem);
                //}

                //if (e.OrderConcludInfo.UnfinishedCount != 0)
                //{
                //    this.SellingUnfinishedCollection.Add(e.OrderConcludInfo);
                //}
            }

            if (e.OrderConcludInfo.UnfinishedCount == 0 && DateTime.Now.Subtract(e.OrderConcludInfo.OrderConcludTime).TotalMinutes < 10)
            {
                this.kHControlViewModel.GetAccountInfo();
            }

            WriteLog(LogType.체결, string.Format("평균단가: {0}, 체결수량: {1}, 미체결수량: {2}", e.OrderConcludInfo.OrderConcludPrice, e.OrderConcludInfo.OrderCount, e.OrderConcludInfo.UnfinishedCount));
        }

        void kHControlViewModel_ReceiveAccountInfo(object sender, AccountModelArgs e)
        {
            if (e.AccountModel.StCode != this.AccountModel.StCode)
            {
                return;
            }

            this.AccountModel.HaveStockCount = e.AccountModel.HaveStockCount;
            this.AccountModel.GainLossRate = e.AccountModel.GainLossRate;
            this.AccountModel.GainLoss = e.AccountModel.GainLoss;
            this.AccountModel.PurchaseAmount = e.AccountModel.PurchaseAmount;
            this.AccountModel.PurchasePrice = e.AccountModel.PurchasePrice;
            this.AccountModel.EvalPrice = e.AccountModel.EvalPrice;

            if (this.AccountModel.HaveStockCount == 0)
            {
                //RemoveThis();
            }
        }
        #endregion

        #region | Ctor |
        public TradeControlViewModel()
        {

        }
        #endregion

        #region | Method |
        public void Initialize()
        {
            this.TradeStateModel = new TradeStateModel();
            //나중에 여기든 어디든 해서 PubSub 을 통해서 Trade State 갱신~!!

            this.PropertyChanged += TradeControlViewModel_PropertyChanged;

            this.resetEvent = new AutoResetEvent(false);
            this.tradeThread = new Thread(TradeThreadAction);
            this.tradeThread.Start();
        }

        private void InitKHControlEvents()
        {
            this.kHControlViewModel.ReceiveCurrentPrice += kHControlViewModel_ReceiveCurrentPrice;
            this.kHControlViewModel.ReceiveOrderConcludInfo += kHControlViewModel_ReceiveOrderConcludInfo;
            this.kHControlViewModel.ReceiveAccountInfo += kHControlViewModel_ReceiveAccountInfo;
            this.kHControlViewModel.ReceiveRealCurrentData += kHControlViewModel_ReceiveRealCurrentData;

            //CurrentPrice 요청
            //this.kHControlViewModel.GetCurrentPrice(this.RegistStockInfo.StCode);

            this.kHControlViewModel.GetCurrentPrice(this.AccountModel.StCode);
        }

        /// <summary>
        /// Mather Control Init 이후 진행이 됨
        /// </summary>
        /// <param name="kHControlViewModel"></param>
        /// <param name="tradeManagerControlViewModel"></param>
        public void InitMatherControls(KHControlViewModel kHControlViewModel, TradeManagerControlViewModel tradeManagerControlViewModel)
        {
            this.kHControlViewModel = kHControlViewModel;
            this.tradeManagerControlViewModel = tradeManagerControlViewModel;

            Initialize();
            InitKHControlEvents();
        }

        /// <summary>
        /// Mather Control Init 이후 진행이 됨
        /// </summary>
        /// <param name="kHControlViewModel"></param>
        /// <param name="tradeManagerControlViewModel"></param>
        /// <param name="stockInfo"></param>
        public void InitMatherControls(KHControlViewModel kHControlViewModel, TradeManagerControlViewModel tradeManagerControlViewModel, AccountModel accountModel)
        {
            this.AccountModel = accountModel;

            InitMatherControls(kHControlViewModel, tradeManagerControlViewModel);
        }

        private void RunTradeAction(string para)
        {
            OrderModel orderModel = new OrderModel()
            {
                StCode = this.AccountModel.StCode,
                StName = this.AccountModel.StName,
                CurrentPrice = this.StockCurrentPrice.CurrentPrice
            };

            switch (para)
            {
                case "Purcase":
                    {
                        orderModel.HogaCode = HogaCode.지정가;
                        orderModel.OrderType = OrderType.신규매수;
                    }
                    break;
                case "Selling":
                    {
                        orderModel.HogaCode = HogaCode.지정가;
                        orderModel.OrderType = OrderType.신규매도;
                        orderModel.OrderCount = this.AccountModel.HaveStockCount;
                    }
                    break;
                default:
                    break;
            }

            SendOrderWindow window = new SendOrderWindow(orderModel);
            if (window.ShowDialog() == true)
            {
                //주문전송
                this.kHControlViewModel.SendOrder(orderModel.StCode, orderModel.OrderType, orderModel.OrderCount, orderModel.OrderPrice, orderModel.HogaCode, "0");
            }
        }

        private void TradeThreadAction()
        {
            //int refreshTimeCheck = 0;
            //while (true)
            //{
            //    if (!this.IsRunning)
            //    {
            //        this.resetEvent.WaitOne();
            //    }

            //    Thread.Sleep(100);

            //    if (this.State == TradeState.대기 || !this.kHControlViewModel.IsConnected)
            //    {
            //        continue;
            //    }

            //    //1. 해당 Rule ID로 Rule 검색
            //    if (--refreshTimeCheck < 0)
            //    {
            //        //10초마다 갱신 
            //        refreshTimeCheck = 100;
            //        //여길 어케 할꼬? 이젠 안쓰겠지
            //        //this.tradeRuleModel = this.tradeManagerControlViewModel.GetTradeRuleModel(this.RuleId);
            //    }

            //    if (this.tradeRuleModel == null)
            //    {
            //        continue;
            //    }

            //    PuchaseAction();
            //    SellingAction();
            //}
        }

        private void PuchaseAction()
        {
            //if (this.State == TradeState.매도)
            //{
            //    //매도 상태로 간 이후부터는 매수룰 X
            //    return;
            //}

            //var targetSequence = this.Sequence;
            ////다음 구매 시퀀스의 아이템을 찾음
            ////var nextPurchaseRule = this.tradeRuleModel.PurchaseRules.FirstOrDefault(x => x.Sequence.Equals(targetSequence));
            ////if (nextPurchaseRule == null)
            ////{
            ////    return;
            ////}
            ////1. Time Type의 분류
            //if (!TimeTypeCheck(nextPurchaseRule.TimeTypeInfo))
            //{
            //    return;
            //}
            ////2. Rule Type의 분류
            //switch (nextPurchaseRule.RuleTypeInfo)
            //{
            //    case RuleModel.RuleType.Range:
            //        {
            //            var nowRate = targetSequence == 1 ? this.StockCurrentPrice.ChangeRate : this.AccountModel.GainLossRate;
            //            var success = CheckRange(targetSequence, nextPurchaseRule.RuleValue, nowRate);
            //            if (success)
            //            {
            //                //구매액 만큼 매수 전송
            //                //매수 전송 후 Sequnce 상승
            //                //시장가로 할찌, 고오민
            //                SendOrder(nextPurchaseRule);
            //            }
            //        }
            //        break;
            //    case RuleModel.RuleType.Signal:
            //        //현재는 Signal X
            //        return;
            //    default:
            //        break;
            //}
        }

        public static bool CheckRange(int targetSequence, string ruleValue, double nowRate)
        {
            if (string.IsNullOrEmpty(ruleValue))
            {
                return false;
            }

            var range = ruleValue.Split('~');
            var success = false;

            if (range.Length == 2)
            {
                var doubleValues = range.Select(x => Convert.ToDouble(x)).ToList();

                var minRate = Math.Min(doubleValues[0], doubleValues[1]);
                var maxRate = Math.Max(doubleValues[0], doubleValues[1]);

                //var nowRate = targetSequence == 1 ? this.StockCurrentPrice.ChangeRate : this.AccountModel.GainLossRate;

                //보유수량이 0 일 시 오늘의 수익률로 min max 룰 분석
                if (nowRate >= minRate && nowRate <= maxRate)
                {
                    success = true;
                }
            }
            else
            {
                var equalSign = Regex.Match(ruleValue, "[>=<]+").Value;
                var compareRate = Convert.ToDouble(Regex.Match(ruleValue, "[0-9-]+").Value);

                //var nowRate = targetSequence == 1 ? this.StockCurrentPrice.ChangeRate : this.AccountModel.GainLossRate;
                //3 > 4
                switch (equalSign)
                {
                    case ">":
                        if (compareRate > nowRate)
                        {
                            success = true;
                        }
                        break;
                    case ">=":
                        if (compareRate >= nowRate)
                        {
                            success = true;
                        }
                        break;
                    case "<":
                        if (compareRate < nowRate)
                        {
                            success = true;
                        }
                        break;
                    case "<=":
                        if (compareRate <= nowRate)
                        {
                            success = true;
                        }
                        break;
                    default:
                        break;
                }
            }

            return success;
        }

        private void SendOrder(PurchaseRuleInfo purchaseRuleInfo)
        {
            //매수 비중을 통해서 매수
            //현재 보유수량에 따라 비중 결정            
            var orderCount = 0;

            if (purchaseRuleInfo.Price.Contains('%'))
            {
                var percent = Convert.ToDouble(Regex.Match(purchaseRuleInfo.Price, "[0-9]+").Value);
                var purchasePrice = (this.kHControlViewModel.AccountInfo.예수금 / 100) * percent;

                orderCount = Math.Abs((int)purchasePrice / this.StockCurrentPrice.CurrentPrice);
            }
            else
            {
                orderCount = Math.Abs(Convert.ToInt32(purchaseRuleInfo.Price) / this.StockCurrentPrice.CurrentPrice);
            }

            var orderType = OrderType.신규매수;
            var hogaCode = HogaCode.지정가;


            //시장가와 지정가의 고오민
            this.kHControlViewModel.SendOrder(this.StockCurrentPrice.StCode, orderType, orderCount, this.StockCurrentPrice.CurrentPrice, hogaCode, "0");

            //this.Sequence++;

            WriteLog(LogType.매수전송, string.Format("매수수량: {0}, 주문타입: {1}, 호가코드: {2}", orderCount, orderType, hogaCode));
        }

        private void SellingAction()
        {
            //if (this.AccountModel.HaveStockCount == 0)
            //{
            //    //보유수량이 없으니 X요.
            //    return;
            //}

            //var targetSequence = this.State == TradeState.매수 ? 1 : this.Sequence;
            ////다음 구매 시퀀스의 아이템을 찾음
            //var nextSellingRule = this.tradeRuleModel.SellingRules.FirstOrDefault(x => x.Sequence.Equals(targetSequence));
            //if (nextSellingRule == null)
            //{
            //    return;
            //}
            ////1. Time Type의 분류
            //if (!TimeTypeCheck(nextSellingRule.TimeTypeInfo))
            //{
            //    return;
            //}
            ////2. Rule Type의 분류
            //switch (nextSellingRule.RuleTypeInfo)
            //{
            //    case RuleModel.RuleType.Range:
            //        {
            //            var nowRate = targetSequence == 1 ? this.StockCurrentPrice.ChangeRate : this.AccountModel.GainLossRate;
            //            var success = CheckRange(targetSequence, nextSellingRule.RuleValue, nowRate);
            //            var cutSuccess = CheckRange(targetSequence, nextSellingRule.CutRate, nowRate);
            //            if (success || cutSuccess)
            //            {
            //                //구매액 만큼 매수 전송
            //                //매수 전송 후 Sequnce 상승
            //                //시장가로 할찌, 고오민
            //                SendOrder(nextSellingRule);
            //            }
            //        }
            //        break;
            //    case RuleModel.RuleType.Signal:
            //        //현재는 Signal X
            //        return;
            //    default:
            //        break;
            //}
        }

        private void SendOrder(SellingRuleInfo sellingRuleInfo)
        {
            //매수 비중을 통해서 매수
            //현재 보유수량에 따라 비중 결정            
            var orderCount = Convert.ToInt32(this.AccountModel.HaveStockCount / sellingRuleInfo.TradePercent);

            var orderType = OrderType.신규매도;
            var hogaCode = HogaCode.지정가;

            //시장가와 지정가의 고오민
            this.kHControlViewModel.SendOrder(this.StockCurrentPrice.StCode, OrderType.신규매도, orderCount, this.StockCurrentPrice.CurrentPrice, HogaCode.지정가, "0");

            //this.Sequence = this.State == TradeState.매수 ? 2 : this.Sequence + 1;

            WriteLog(LogType.매도전송, string.Format("매수수량: {0}, 주문타입: {1}, 호가코드: {2}", orderCount, orderType, hogaCode));
        }

        private bool TimeTypeCheck(RuleModel.TimeType timeType)
        {
            bool result = false;

            var now = DateTime.Now;

            switch (timeType)
            {
                case RuleModel.TimeType.RealTime:
                    //월~금 이면서 9시 부터 15시 30분 장 중
                    if (now.Hour >= 9 && (now.Hour <= 14 || (now.Hour == 15 && now.Minute <= 30)))
                    {
                        result = true;
                    }
                    break;
                case RuleModel.TimeType.StartTime:
                    if (now.Hour == 9 && now.Minute <= 30)
                    {
                        result = true;
                    }
                    break;
                case RuleModel.TimeType.EndTime:
                    if (now.Hour == 15 && now.Minute <= 30)
                    {
                        result = true;
                    }
                    break;
                default:
                    break;
            }

            return result;
        }

        private void WriteLog(LogType logType, string message)
        {
            this.LogMessage += string.Format("[{0}] {1}\r\n", logType, message);
        }
        #endregion

        #region | Dispose |
        public new void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Parent에 자신을 제거하도록 전송
        /// </summary>
        public void RemoveThis()
        {
            this.tradeManagerControlViewModel.RemoveItem(this);
        }

        protected override void Dispose(bool disposing)
        {
            Console.WriteLine("Trade Control Dispose~!");

            if (!disposed)
            {
                this.tradeThread.Abort();

                disposed = true;
            }
        }
        #endregion
    }
}
