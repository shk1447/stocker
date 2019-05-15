using AxKHOpenAPILib;
using Common;
using Interface;
using KiwoomCode;
using Common.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Threading;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Collections.Concurrent;
using Interface.Model;
using Newtonsoft.Json.Linq;
using Interface.DataFormat;
using System.ComponentModel;

namespace ScalKing.Controls
{
    public class KHControlViewModel : BaseViewModel
    {
        #region | Properties |
        private bool isConnected;
        public bool IsConnected
        {
            get
            {
                return isConnected;
            }
            set
            {
                isConnected = value;
                OnPropertyChanged("IsConnected");
            }
        }

        private UserInfo userInfo;
        public UserInfo UserInfo
        {
            get
            {
                return userInfo;
            }

            set
            {
                userInfo = value;
                OnPropertyChanged("UserInfo");
            }
        }

        private string selectedAccount;
        public string SelectedAccount
        {
            get
            {
                return selectedAccount;
            }
            set
            {
                selectedAccount = value;
                OnPropertyChanged("SelectedAccount");
            }
        }

        private AccountInfo accountInfo;
        public AccountInfo AccountInfo
        {
            get
            {
                return accountInfo;
            }

            set
            {
                accountInfo = value;
                OnPropertyChanged("AccountInfo");
            }
        }

        private int totalGainlossPrice;
        public int TotalGainlossPrice
        {
            get
            {
                return totalGainlossPrice;
            }

            set
            {
                totalGainlossPrice = value;
                OnPropertyChanged("TotalGainlossPrice");
            }
        }

        private string sGO2IP;
        public string SGO2IP
        {
            get
            {
                return sGO2IP;
            }
            set
            {
                sGO2IP = value;
                OnPropertyChanged("SGO2IP");
            }
        }

        #endregion

        #region | Private |
        private AxKHOpenAPI khAPI;
        private Dictionary<string, StockTimePrice> continuityCacheTimeData;
        private DispatcherTimer oneSecondTimer;
        private int scrNum = 5000;
        //private ManualResetEvent khAPIWaitEvent;
        private ConcurrentQueue<KeyValuePair<APIRunType, object[]>> apiRunQueue;
        private Thread apiRunThread;

        private static readonly string DefaultScreenCode = "5001";
        private AutoResetEvent apiWaitEvent;
        #endregion

        #region | Command |
        private MyCommand loginCommand;
        public MyCommand LoginCommand
        {
            get
            {
                return loginCommand ?? (loginCommand = new MyCommand(para =>
                {
                    if (this.khAPI == null)
                    {
                        throw new Exception("KH API 설정안됨");
                    }

                    Login();
                }));
            }
        }

        private MyCommand accountRefresh;
        public MyCommand AccountRefresh
        {
            get
            {
                return accountRefresh ?? (accountRefresh = new MyCommand(para =>
                {
                    if (this.khAPI == null)
                    {
                        throw new Exception("KH API 설정안됨");
                    }

                    GetUnfinishedConclud();
                    GetAccountInfo();
                }));
            }
        }

        private MyCommand stopRealTime;
        public MyCommand StopRealTime
        {
            get
            {
                return stopRealTime ?? (stopRealTime = new MyCommand(para =>
                {
                    DisconnectAllRealData();
                }));
            }
        }

        public enum APIRunType
        {
            GetCurrentPrice, GetMultiData, GetTimePrice, Get매물대, GetAccountInfo, SendOrder, GetCondition, SendCondition, GetUnfinishedConclud, GetStrength
        }
        #endregion

        #region | Events |
        void oneSecondTimer_Tick(object sender, EventArgs e)
        {
            //Connection Check
            var connCheckVal = this.IsConnected ? 1 : 0;
            if (connCheckVal != this.khAPI.GetConnectState())
            {
                this.IsConnected = !this.IsConnected;
            }
        }

        private void KhAPI_OnEventConnect(object sender, _DKHOpenAPIEvents_OnEventConnectEvent e)
        {
            Log.Info("[로그인 처리결과] " + Error.GetErrorMessage(e.nErrCode));

            this.IsConnected = e.nErrCode == 0 ? true : false;
        }

        private void KhAPI_OnReceiveChejanData(object sender, _DKHOpenAPIEvents_OnReceiveChejanDataEvent e)
        {
            if (e.sGubun == "0")
            {
                //Log.Info("구분 : 주문체결통보");
                //Log.Info("주문/체결시간 : " + this.khAPI.GetChejanData(908));
                //Log.Info("주문번호 : " + this.khAPI.GetChejanData(9203));
                //Log.Info("종목명 : " + this.khAPI.GetChejanData(302));
                //Log.Info("주문수량 : " + this.khAPI.GetChejanData(900));
                //Log.Info("주문가격 : " + this.khAPI.GetChejanData(901));
                //Log.Info("체결수량 : " + this.khAPI.GetChejanData(911));
                //Log.Info("체결가격 : " + this.khAPI.GetChejanData(910));

                //체결 수량이 있고, 미 체결 수량이 0 일때  - 전부 완료됬을때 기록
                if (!string.IsNullOrEmpty(this.khAPI.GetChejanData(911)))// && Convert.ToInt32(this.khAPI.GetChejanData(902)) == 0)
                {
                    OrderConcludInfo orderConcludInfo = new OrderConcludInfo()
                    {
                        Account = this.khAPI.GetChejanData(9201),
                        OrderNumber = this.khAPI.GetChejanData(9203),
                        StCode = this.khAPI.GetChejanData(9001).Replace("A", ""),
                        OrderStatus = this.khAPI.GetChejanData(913),
                        StName = this.khAPI.GetChejanData(302),
                        OrderCount = Convert.ToInt32(this.khAPI.GetChejanData(900)),
                        OrderPrice = Convert.ToInt32(this.khAPI.GetChejanData(901)),
                        UnfinishedCount = Convert.ToInt32(this.khAPI.GetChejanData(902)),
                        TotalPrice = Convert.ToInt32(this.khAPI.GetChejanData(903)),
                        OrderDivision = this.khAPI.GetChejanData(905).Replace("+", "").Replace("-", ""),
                        TradingDivision = this.khAPI.GetChejanData(906),
                        OrderConcludTime = Convert.ToDateTime(DateTime.Now.ToShortDateString() + " " + this.khAPI.GetChejanData(908).Insert(4, ":").Insert(2, ":")),
                        OrderConcludPrice = Convert.ToInt32(this.khAPI.GetChejanData(910)),
                        OrderConcludCount = Convert.ToInt32(this.khAPI.GetChejanData(911))
                        //CurrentPrice = Convert.ToInt32(this.khAPI.GetChejanData(10)),
                        //HaveStockCount = Convert.ToInt32(this.khAPI.GetChejanData(930)),
                        //TotalPurchasePrice = Convert.ToInt32(this.khAPI.GetChejanData(932)),
                        //GainLossRate = Convert.ToDouble(this.khAPI.GetChejanData(8019))
                    };
                    //orderConcludInfo.PriceAfterFees = CalcPriceAfterFees(orderConcludInfo.TotalPrice);
                    //orderConcludInfo.GainLossPrice = orderConcludInfo.PriceAfterFees - orderConcludInfo.TotalPrice;
                    //orderConcludInfo.GainLossRate = (orderConcludInfo.GainLossPrice / (double)orderConcludInfo.TotalPrice) * 100;

                    OnReceiveOrderConcludInfo(orderConcludInfo);
                }
            }
            else if (e.sGubun == "1")
            {
                Log.Info("구분 : 잔고통보");
            }
            else if (e.sGubun == "3")
            {
                Log.Info("구분 : 특이신호");
            }
        }

        private void KhAPI_OnReceiveMsg(object sender, _DKHOpenAPIEvents_OnReceiveMsgEvent e)
        {
            //Log.Info(Log.조회, "===================================================");
            //Log.Info(string.Format("화면번호:{0} | RQName:{1} | TRCode:{2} | 메세지:{3}", e.sScrNo, e.sRQName, e.sTrCode, e.sMsg));
        }

        private void KhAPI_OnReceiveRealData(object sender, _DKHOpenAPIEvents_OnReceiveRealDataEvent e)
        {
            //Log.Info(string.Format("종목코드 : {0} | RealType : {1} | RealData : {2}", e.sRealKey, e.sRealType, e.sRealData));

            switch (e.sRealType)
            {
                case "주식시세":
                    {
                        Log.Info(string.Format("종목코드 : {0} | 현재가 : {1} | 등락율 : {2} | 누적거래량 : {3} ",
                        e.sRealKey,
                        this.khAPI.GetCommRealData(e.sRealType, 10).Trim(),
                        this.khAPI.GetCommRealData(e.sRealType, 12).Trim(),
                        this.khAPI.GetCommRealData(e.sRealType, 13).Trim()));
                    }
                    break;
                case "주식체결":
                    {
                        //현재가 리얼 데이터이므로 넘기기
                        var data = e.sRealData.Split('\t').Where(x => !string.IsNullOrEmpty(x)).ToArray();

                        if (data.Length < 25)
                        {
                            return;
                        }

                        DateTime time = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, Convert.ToInt32(data[0].Substring(0, 2)), Convert.ToInt32(data[0].Substring(2, 2)), Convert.ToInt32(data[0].Substring(4, 2)));

                        RealCurrentData currentData = new RealCurrentData()
                        {
                            StCode = e.sRealKey.Trim().Replace("A", ""),
                            DateTime = time,
                            UnixTime = Common.Util.GetUnixTimeInt32(time.ToUniversalTime()),
                            CurrentPrice = Math.Abs(Convert.ToInt32(data[1])),
                            전일대비 = Convert.ToInt32(data[2]),
                            등락율 = Convert.ToDouble(data[3]),
                            //매도호가 = data[4],
                            //매수호가 = data[5],
                            거래량 = Math.Abs(Convert.ToInt64(data[6])),
                            //누적거래량 = data[7],
                            //누적거래대금 = data[8],
                            시가 = Convert.ToInt32(data[9]),
                            고가 = Convert.ToInt32(data[10]),
                            저가 = Convert.ToInt32(data[11]),
                            //전일대비기호 = data[12],
                            //전일거래량대비_계약 = data[13],
                            //거래대금증감 = data[14],
                            //전일거래량대비_비율 = data[15],
                            //거래회전율 = data[16],
                            //거래비용 = data[17],
                            //체결강도 = data[18],
                            //시가총액 = data[19],
                            //장구분 = data[20],
                            //KO접근도 = data[21],
                            //상한가발생시간 = data[22],
                            //하한가발생시간 = data[23]
                        };

                        OnReceiveRealCurrentData(currentData);
                    }
                    break;
                case "주식호가잔량":
                    {
                        RealHogaData hogaCurrentData = new RealHogaData()
                        {
                            StCode = e.sRealKey,
                            DateTime = DateTime.Now,
                            매도호가 = new List<int>(),
                            매수호가 = new List<int>(),
                            매도호가수량 = new List<int>(),
                            매수호가수량 = new List<int>(),
                            매도호가총잔량 = Convert.ToInt32(this.khAPI.GetCommRealData(e.sRealType, 121).Trim()),
                            매수호가총잔량 = Convert.ToInt32(this.khAPI.GetCommRealData(e.sRealType, 125).Trim())
                        };

                        for (int i = 1; i <= 10; i++)
                        {
                            hogaCurrentData.매도호가.Add(Convert.ToInt32(this.khAPI.GetCommRealData(e.sRealType, i + 40).Trim()));
                            hogaCurrentData.매수호가.Add(Convert.ToInt32(this.khAPI.GetCommRealData(e.sRealType, i + 50).Trim()));
                            hogaCurrentData.매도호가수량.Add(Convert.ToInt32(this.khAPI.GetCommRealData(e.sRealType, i + 60).Trim()));
                            hogaCurrentData.매수호가수량.Add(Convert.ToInt32(this.khAPI.GetCommRealData(e.sRealType, i + 70).Trim()));
                        }

                        //매도호가 , 41 to 50
                        //매수호가 , 51 to 60
                        //매도호가수량 , 61 to 70
                        //매수호가수량 , 71 to 80
                        OnReceiveRealHogaData(hogaCurrentData);
                    }
                    break;
            }

            //실시간데이터 분석 및 적용 
        }

        private void KhAPI_OnReceiveTrData(object sender, _DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            var rqName = e.sRQName.Split('_');

            StockTimePrice.TimeType receiveTimeType;

            var stCode = rqName.Length > 1 ? rqName[1] : string.Empty;

            if (Enum.TryParse<StockTimePrice.TimeType>(rqName[0], out receiveTimeType))
            {
                //timeType.ToString(), stockCode, range, date, endDate
                var range = rqName[2];
                var date = rqName[3];
                var endDate = rqName[4];

                StockTimePrice timePrice = this.continuityCacheTimeData.ContainsKey(stCode) ? this.continuityCacheTimeData[stCode] : new StockTimePrice()
                {
                    Name = rqName[0],
                    StCode = stCode,
                    Type = receiveTimeType,
                    PriceList = new List<TimePrice>()
                };

                var isSend = false;

                switch (receiveTimeType)
                {
                    case StockTimePrice.TimeType.Tick:
                    case StockTimePrice.TimeType.Minute:
                    case StockTimePrice.TimeType.Day:
                        {
                            //연속 데이터 조오회
                            isSend = GetStockTimePrice(e.sTrCode, rqName[0], stCode, receiveTimeType, timePrice, date, endDate);
                        }
                        break;
                    case StockTimePrice.TimeType.Week:
                        break;
                    case StockTimePrice.TimeType.Month:
                        break;
                    case StockTimePrice.TimeType.DayTrend:
                        {
                            isSend = GetStockTimePrice(e.sTrCode, rqName[0], stCode, receiveTimeType, timePrice, date, endDate);
                        }
                        break;
                    case StockTimePrice.TimeType.Multy:
                        break;
                }

                if (e.sPrevNext == "2" && !isSend)//&& receiveTimeType != StockTimePrice.TimeType.Day)
                {
                    //연속조회
                    if (!this.continuityCacheTimeData.ContainsKey(stCode))
                    {
                        this.continuityCacheTimeData.Add(stCode, timePrice);
                    }

                    GetTimePrice(receiveTimeType, stCode, range, date, endDate, e.sScrNo, 2);
                }
                else
                {
                    if (this.continuityCacheTimeData.ContainsKey(stCode))
                    {
                        this.continuityCacheTimeData.Remove(stCode);
                    }

                    //Reverse 해서 전송
                    timePrice.PriceList.Reverse();

                    OnReceiveTimePrice(timePrice);
                }
            }
            else
            {
                switch (rqName[0])
                {
                    case "계좌조회":
                        {
                            int nCnt = this.khAPI.GetRepeatCnt(e.sTrCode, e.sRQName);
                            if (nCnt == 0)
                            {
                                this.AccountInfo = CreateInstance<AccountInfo>(e.sTrCode, e.sRQName, 0);
                            }
                            else
                            {
                                this.AccountInfo = CreateInstance<AccountInfo>(e.sTrCode, e.sRQName, 0);

                                for (int i = 0; i < nCnt; i++)
                                {
                                    OnReceiveAccountInfo(new AccountModel()
                                    {
                                        CurrentPrice = Int32.Parse(this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, i, "현재가").Trim()),
                                        EvalPrice = Int32.Parse(this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, i, "평가금액").Trim()),
                                        GainLoss = double.Parse(this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, i, "손익금액").Trim()),
                                        HaveStockCount = Int32.Parse(this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, i, "보유수량").Trim()),
                                        GainLossRate = double.Parse(this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, i, "손익율").Trim()),
                                        PurchaseAmount = Int32.Parse(this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, i, "매입금액").Trim()),
                                        PurchasePrice = double.Parse(this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, i, "평균단가").Trim()),
                                        StCode = this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, i, "종목코드").Trim().Replace("A", ""),
                                        StName = this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, i, "종목명").Trim()
                                    });
                                }
                            }
                        }
                        break;
                    case "주식주문":
                        {
                            string s원주문번호 = this.khAPI.GetCommData(e.sTrCode, "", 0, "").Trim();

                            long n원주문번호 = 0;
                            bool canConvert = long.TryParse(s원주문번호, out n원주문번호);

                            if (canConvert == true)
                            {
                                //주문성공
                                //scheduleIndex Log or Dic cache 저장
                                //var scheduleId = Convert.ToInt32(rqName[2]);
                                //var orderType = rqName[3];

                                //var item = Communicator.RedisClient.Instance.GetScheduleStockItem(scheduleId, stCode);
                                //if (item == null)
                                //{
                                //    //매수면 Set 매도면 Remove
                                //    Communicator.RedisClient.Instance.SetScheduleStockItem(new ScheduleStockItem()
                                //    {
                                //        ScheduleId = scheduleId,
                                //        StCode = stCode,
                                //        TradeHistory = new List<TradeHistory>()
                                //    });
                                //}
                                //else
                                //{
                                //    //한번 구매한거는 오늘 더 이상 구매하지 않음                                    
                                //    //매일 아침 실행할때 IsSelling 여부가 True인것들 삭제

                                //    var orderTypeEnum = (OrderType)Enum.Parse(typeof(OrderType), orderType);
                                //    if (orderTypeEnum == OrderType.신규매도)
                                //    {
                                //        item.IsSelling = true;
                                //        Communicator.RedisClient.Instance.SetScheduleStockItem(item);
                                //        //Communicator.RedisClient.Instance.RemoveScheduleStockItem(scheduleId, stCode);
                                //    }
                                //}
                            }
                            //txt원주문번호.Text = s원주문번호;
                            else
                            {
                                Log.Info("잘못된 원주문번호 입니다, " + e.sRQName);
                            }
                        }
                        break;
                    case "체결강도":
                        {
                            int nCnt = this.khAPI.GetRepeatCnt(e.sTrCode, e.sRQName);

                            for (int i = nCnt - 1; i >= 0; i--)
                            {
                            }
                        }
                        break;
                    case "주식기본정보":
                        {
                            int nCnt = this.khAPI.GetRepeatCnt(e.sTrCode, e.sRQName);

                            OnReceiveCurrentPrice(new StockCurrentPrice()
                            {
                                StCode = rqName[1],
                                StName = this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, 0, "종목명").Trim(),
                                CurrentPrice = Int32.Parse(this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, 0, "현재가").Trim()),
                                ChangeRate = Convert.ToDouble(this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, 0, "등락율").Trim()),
                                ChangePrice = Int32.Parse(this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, 0, "전일대비").Trim()),
                                CumulativeVolume = Int32.Parse(this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, 0, "거래량").Trim()),
                            });
                        }
                        break;
                    case "거래량급증요청":
                        {
                            int nCnt = this.khAPI.GetRepeatCnt(e.sTrCode, e.sRQName);
                            for (int i = 0; i < nCnt; i++)
                            {
                                var s = this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, i, "종목명").Trim();
                                var a = this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, i, "급증률").Trim();

                            }
                        }
                        break;
                    case "주식복수정보":
                        {
                            List<StockCurrentPrice> multiStock = new List<StockCurrentPrice>();

                            int nCnt = this.khAPI.GetRepeatCnt(e.sTrCode, e.sRQName);

                            for (int i = nCnt - 1; i >= 0; i--)
                            {
                                multiStock.Add(new StockCurrentPrice()
                                {
                                    StCode = this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, i, "종목코드").Trim(),
                                    StName = this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, i, "종목명").Trim(),
                                    CurrentPrice = Math.Abs(Int32.Parse(this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, i, "현재가").Trim())),
                                    ChangeRate = Convert.ToDouble(this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, i, "등락율").Trim()),
                                    CumulativeVolume = Int32.Parse(this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, i, "거래량").Trim())
                                });
                            }

                            OnReceiveConditionMultiStock(multiStock);
                        }
                        break;
                    case "매물대":
                        {
                            int nCnt = this.khAPI.GetRepeatCnt(e.sTrCode, e.sRQName);
                            for (int i = 0; i < nCnt; i++)
                            {
                                var s = this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, i, "종가").Trim();
                                var a = this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, i, "거래량합").Trim();

                            }
                        }
                        break;
                    case "미체결정보":
                        {
                            int nCnt = this.khAPI.GetRepeatCnt(e.sTrCode, e.sRQName);
                            for (int i = 0; i < nCnt; i++)
                            {
                                OrderConcludInfo orderConcludInfo = new OrderConcludInfo()
                                {
                                    Account = this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, i, "계좌번호").Trim(),
                                    OrderNumber = this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, i, "주문번호").Trim(),
                                    StCode = this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, i, "종목코드").Trim(),
                                    OrderStatus = this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, i, "주문상태").Trim(),
                                    StName = this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, i, "종목명").Trim(),
                                    OrderCount = Convert.ToInt32(this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, i, "주문수량").Trim()),
                                    OrderPrice = Convert.ToInt32(this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, i, "주문가격").Trim()),
                                    UnfinishedCount = Convert.ToInt32(this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, i, "미체결수량").Trim()),
                                    TotalPrice = Convert.ToInt32(this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, i, "체결누계금액").Trim()),
                                    OrderDivision = this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, i, "주문구분").Trim().Replace("+", "").Replace("-", ""),
                                    TradingDivision = this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, i, "매매구분").Trim(),
                                    OrderConcludTime = Convert.ToDateTime(DateTime.Now.ToShortDateString() + " " + this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, i, "시간").Trim().Insert(4, ":").Insert(2, ":")),
                                    OrderConcludPrice = Convert.ToInt32(this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, i, "체결가").Trim()),
                                    OrderConcludCount = Convert.ToInt32(this.khAPI.CommGetData(e.sTrCode, "", e.sRQName, i, "체결량").Trim())
                                };

                                OnReceiveOrderConcludInfo(orderConcludInfo);
                            }
                        }
                        break;

                }
            }
        }

        void khAPI_OnReceiveConditionVer(object sender, _DKHOpenAPIEvents_OnReceiveConditionVerEvent e)
        {
            //  서버에서 수신한 사용자 조건식을 조건명 인덱스와 조건식 이름을 한 쌍으로 하는 문자열들로 전달합니다.
            //  조건식 하나는 조건명 인덱스와 조건식 이름은 '^'로 나뉘어져 있으며 각 조건식은 ';'로 나뉘어져 있습니다.
            //  이 함수는 반드시 OnReceiveConditionVer()이벤트 함수안에서 사용해야 합니다.
            var conditionName = this.khAPI.GetConditionNameList();

            if (string.IsNullOrEmpty(conditionName))
            {
                return;
            }

            var conditionList = from item in conditionName.Split(';')
                                where !string.IsNullOrEmpty(item)
                                let val = item.Split('^')
                                select new Condition() { ConditionIndex = Convert.ToInt32(val[0]), ConditionName = val[1], Type = ConditionOriginType.Kiwum };

            OnReceiveConditionInfo(conditionList.ToList());
        }

        void khAPI_OnReceiveTrCondition(object sender, _DKHOpenAPIEvents_OnReceiveTrConditionEvent e)
        {
            //BSTR sScrNo,    // 화면번호
            //BSTR strCodeList,   // 종목코드 리스트
            //BSTR strConditionName,    // 조건식 이름
            //int nIndex,   // 조건명 인덱스
            //int nNext   // 연속조회 여부

            //조건검색 요청으로 검색된 종목코드 리스트를 전달하는 이벤트 함수입니다. 
            //종목코드 리스트는 각 종목코드가 ';'로 구분되서 전달됩니다.

            if (string.IsNullOrEmpty(e.strCodeList))
            {
                return;
            }

            OnReceiveConditionStCode(e.strCodeList, Convert.ToInt32(e.nIndex));
        }

        void khAPI_OnReceiveRealCondition(object sender, _DKHOpenAPIEvents_OnReceiveRealConditionEvent e)
        {
            Console.WriteLine(e.strConditionName + " : " + e.sTrCode + " : " + e.strType);
            //BSTR strCode,   // 종목코드
            //BSTR strType,   //  이벤트 종류, "I":종목편입, "D", 종목이탈
            //BSTR strConditionName,    // 조건식 이름 
            //BSTR strConditionIndex    // 조건명 인덱스            

            if (e.strType == "I")
            {
                //편입
                int lRet = this.khAPI.SetRealReg(KHControlViewModel.DefaultScreenCode, e.sTrCode, "9001;302;10;11;25;12;13", "1");// 실시간 시세등록            
                Log.Info(Error.GetErrorMessage(lRet));
            }
            else
            {
                //이탈
                this.khAPI.SetRealRemove(KHControlViewModel.DefaultScreenCode, e.sTrCode);// 실시간 시세해지
            }

            OnReceiveConditionRealStCode(new ConditionRealStCode()
            {
                ConditionIndex = Convert.ToInt32(e.strConditionIndex),
                IsAdded = e.strType == "I",
                StCode = e.sTrCode
            });
        }
        #endregion

        #region | Custom Event |
        public event EventHandler<EventArgs> Connected;
        private void OnConnected()
        {
            if (this.Connected != null)
            {
                this.Connected(this, new EventArgs());
            }
        }

        public event EventHandler<StockCurrentPriceArgs> ReceiveCurrentPrice;
        private void OnReceiveCurrentPrice(StockCurrentPrice stockCurrentPrice)
        {
            if (this.ReceiveCurrentPrice != null)
            {
                this.ReceiveCurrentPrice(null, new StockCurrentPriceArgs(stockCurrentPrice));
            }

            //Send(OpCode.CurrentPrice, 1, stockCurrentPrice);
        }

        public event EventHandler<StockTimePriceArgs> ReceiveTimePrice;
        private void OnReceiveTimePrice(StockTimePrice stockTimePrice)
        {
            if (this.ReceiveTimePrice != null)
            {
                this.ReceiveTimePrice(null, new StockTimePriceArgs(stockTimePrice));
            }
        }

        public event EventHandler<ConditionInfoArgs> ReceiveConditionInfo;
        private void OnReceiveConditionInfo(List<Condition> conditionInfo)
        {
            if (this.ReceiveConditionInfo != null)
            {
                this.ReceiveConditionInfo(null, new ConditionInfoArgs(conditionInfo));
            }
        }

        public event EventHandler<ConditionStCodeArgs> ReceiveConditionStCode;
        private void OnReceiveConditionStCode(string stCodes, int conditionIndex)
        {
            if (this.ReceiveConditionStCode != null)
            {
                this.ReceiveConditionStCode(null, new ConditionStCodeArgs(stCodes, conditionIndex));
            }
        }

        public event EventHandler<ConditionRealStCodeArgs> ReceiveConditionRealStCode;
        private void OnReceiveConditionRealStCode(ConditionRealStCode stCodes)
        {
            if (this.ReceiveConditionRealStCode != null)
            {
                this.ReceiveConditionRealStCode(null, new ConditionRealStCodeArgs(stCodes));
            }
        }

        public event EventHandler<ConditionMultiStockArgs> ReceiveConditionMultiStock;
        private void OnReceiveConditionMultiStock(List<StockCurrentPrice> conditionMultiStock)
        {
            if (this.ReceiveConditionMultiStock != null)
            {
                this.ReceiveConditionMultiStock(null, new ConditionMultiStockArgs(conditionMultiStock));
            }
        }

        public event EventHandler<OrderConcludInfoArgs> ReceiveOrderConcludInfo;
        private void OnReceiveOrderConcludInfo(OrderConcludInfo orderConcludInfo)
        {
            if (this.ReceiveOrderConcludInfo != null)
            {
                this.ReceiveOrderConcludInfo(null, new OrderConcludInfoArgs(orderConcludInfo));
            }
        }

        public event EventHandler<RealCurrentDataArgs> ReceiveRealCurrentData;
        private void OnReceiveRealCurrentData(RealCurrentData realCurrentData)
        {
            if (this.ReceiveRealCurrentData != null)
            {
                this.ReceiveRealCurrentData(null, new RealCurrentDataArgs(realCurrentData));
            }
        }

        public event EventHandler<RealHogaDataArgs> ReceiveRealHogaData;
        private void OnReceiveRealHogaData(RealHogaData RealHogaData)
        {
            if (this.ReceiveRealHogaData != null)
            {
                this.ReceiveRealHogaData(null, new RealHogaDataArgs(RealHogaData));
            }
        }

        public event EventHandler<AccountModelArgs> ReceiveAccountInfo;
        private void OnReceiveAccountInfo(AccountModel accountInfo)
        {

            if (this.ReceiveAccountInfo != null)
            {
                this.ReceiveAccountInfo(null, new AccountModelArgs(accountInfo));
            }

            //Send(OpCode.AccountInfo, 1, accountInfo);
        }
        #endregion

        #region | Ctor |
        public KHControlViewModel()
        {
            this.SGO2IP = "localhost:4949";
            this.PropertyChanged += KHControlViewModel_PropertyChanged;

            this.oneSecondTimer = new DispatcherTimer();
            this.oneSecondTimer.Tick += oneSecondTimer_Tick;
            this.oneSecondTimer.Interval = new TimeSpan(0, 0, 1);

            //this.MainWindowViewModel = mainWindowViewModel;
            this.continuityCacheTimeData = new Dictionary<string, StockTimePrice>();

            //this.khAPIWaitEvent = new ManualResetEvent(false);
            this.apiWaitEvent = new AutoResetEvent(false);
            this.apiRunQueue = new ConcurrentQueue<KeyValuePair<APIRunType, object[]>>();
            this.apiRunThread = new Thread(ApiRunAction);
            this.apiRunThread.Start();
        }

        public void ApiRunAction()
        {
            this.apiWaitEvent.WaitOne();

            while (true)
            {
                try
                {
                    while (!this.apiRunQueue.IsEmpty && this.IsConnected)
                    {
                        KeyValuePair<APIRunType, object[]> action = new KeyValuePair<APIRunType, object[]>();

                        if (this.apiRunQueue.TryDequeue(out action))
                        {
                            //action.Key.Invoke(null, action.Value);
                            switch (action.Key)
                            {
                                case APIRunType.GetStrength:
                                    API_GetStrength(action.Value[0].ToString());
                                    break;
                                case APIRunType.GetCurrentPrice:
                                    API_GetCurrentPrice(action.Value[0].ToString());
                                    break;
                                case APIRunType.GetMultiData:
                                    API_GetMultiData(action.Value[0].ToString());
                                    break;
                                case APIRunType.GetTimePrice:
                                    API_GetTimePrice((StockTimePrice.TimeType)Enum.Parse(typeof(StockTimePrice.TimeType), action.Value[0].ToString()), Convert.ToString(action.Value[1]), Convert.ToString(action.Value[2]), Convert.ToString(action.Value[3]), Convert.ToString(action.Value[4]), Convert.ToString(action.Value[5]), Convert.ToInt32(action.Value[6]));
                                    break;
                                case APIRunType.Get매물대:
                                    API_Get매물대(action.Value[0].ToString(), action.Value[1].ToString(), action.Value[2].ToString(), action.Value[3].ToString(), action.Value[4].ToString());
                                    break;
                                case APIRunType.GetAccountInfo:
                                    API_GetAccountInfo();
                                    break;
                                case APIRunType.SendOrder:
                                    API_SendOrder(action.Value[0].ToString(), (OrderType)Enum.Parse(typeof(OrderType), action.Value[1].ToString()), Convert.ToInt32(action.Value[2]), Convert.ToInt32(action.Value[3]), (HogaCode)Enum.Parse(typeof(HogaCode), action.Value[4].ToString()), action.Value[5].ToString(), Convert.ToInt32(action.Value[6]));
                                    break;
                                case APIRunType.GetCondition:
                                    API_GetCondition();
                                    break;
                                case APIRunType.SendCondition:
                                    API_SendCondition(Convert.ToInt32(action.Value[0]), Convert.ToString(action.Value[1]), Convert.ToInt32(action.Value[2]));
                                    break;
                                case APIRunType.GetUnfinishedConclud:
                                    API_GetUnfinishedConclud();
                                    break;
                                default:
                                    break;
                            }

                            Thread.Sleep(300);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }

                Thread.Sleep(100);
            }
        }

        private void KHControlViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsConnected":
                    if (this.IsConnected)
                    {
                        this.UserInfo = AccountCheck();
                        //Condition, 판매룰 , 구매룰 , 스케줄 조회
                        //this.MainWindowViewModel.SuperControlViewModel.ConditionControlViewModel.GetKiwumConditionAction();
                        //this.MainWindowViewModel.SuperControlViewModel.PurchaseRuleControlViewModel.GetPurchaseRuleAction();
                        //this.MainWindowViewModel.SuperControlViewModel.SellingRuleControlViewModel.GetSellingRuleAction();
                        //this.MainWindowViewModel.SuperControlViewModel.PurchaseScheduleControlViewModel.GetPurchaseScheduleAction();
                        //첫번째 계좌 선택
                        this.SelectedAccount = this.UserInfo.Accounts[0];

                        OnConnected();

                        BackgroundWorker bgw = new BackgroundWorker();
                        bgw.DoWork += (_, __) =>
                        {
                            Thread.Sleep(1500);
                        };
                        bgw.RunWorkerCompleted += (_, __) =>
                        {
                            this.apiWaitEvent.Set();
                        };
                        bgw.RunWorkerAsync();
                    }
                    break;
                case "UserInfo":
                    if (this.IsConnected && (this.UserInfo == null || string.IsNullOrEmpty(this.UserInfo.Name)))
                    {
                        this.UserInfo = AccountCheck();
                    }
                    break;
                case "SelectedAccount":
                    {
                        if (string.IsNullOrEmpty(this.SelectedAccount))
                        {
                            return;
                        }
                        GetAccountInfo();
                        GetUnfinishedConclud();
                    }
                    break;
            }
        }
        #endregion

        #region | Methods |
        public void InitializeAPI(AxKHOpenAPI khAPI)
        {
            this.khAPI = khAPI;

            this.khAPI.OnReceiveTrData += KhAPI_OnReceiveTrData;
            this.khAPI.OnReceiveRealData += KhAPI_OnReceiveRealData;
            this.khAPI.OnReceiveMsg += KhAPI_OnReceiveMsg;
            this.khAPI.OnReceiveChejanData += KhAPI_OnReceiveChejanData;
            this.khAPI.OnEventConnect += KhAPI_OnEventConnect;
            this.khAPI.OnReceiveRealCondition += khAPI_OnReceiveRealCondition;
            this.khAPI.OnReceiveTrCondition += khAPI_OnReceiveTrCondition;
            this.khAPI.OnReceiveConditionVer += khAPI_OnReceiveConditionVer;

            //this.oneSecondTimer.Start();


            //RedisClient client = new RedisClient("localhost:6379");
            //client.Connect();
        }

        /// <summary>
        /// 화면번호 생산
        /// </summary>
        /// <returns></returns>
        private string GetScrNum()
        {
            if (scrNum < 9999)
                scrNum++;
            else
            {
                scrNum = 5001;

                DisconnectAllRealData();
            }

            return scrNum.ToString();
        }

        /// <summary>
        /// 유효성검사
        /// </summary>
        /// <returns></returns>
        private bool CheckValidation()
        {
            if (this.khAPI == null)
            {
                Log.Error("KH API 설정안됨");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Login 창 띄우기
        /// </summary>
        public void Login()
        {
            if (!CheckValidation()) return;

            if (this.khAPI.CommConnect() == 0)
            {
                Log.Info("Login창 열기 성공");
            }
            else
            {
                Log.Info("Login창 열기 실패");
            }
        }

        /// <summary>
        /// 로그아웃
        /// </summary>
        public void Logout()
        {
            DisconnectAllRealData();
            this.khAPI.CommTerminate();
            Log.Info("로그아웃");
        }

        /// <summary>
        /// 실시간 연결 종료
        /// </summary> 
        private void DisconnectAllRealData()
        {
            for (int i = scrNum; i > 5000; i--)
            {
                this.khAPI.DisconnectRealData(i.ToString());
            }

            scrNum = 5000;
        }

        /// <summary>
        /// 계좌 조회
        /// </summary>
        /// <returns></returns>
        public UserInfo AccountCheck()
        {
            return new UserInfo()
            {
                UserID = this.khAPI.GetLoginInfo("USER_ID"),
                Name = this.khAPI.GetLoginInfo("USER_NAME"),
                Accounts = this.khAPI.GetLoginInfo("ACCNO").Split(';').Where(x => !string.IsNullOrEmpty(x)).ToArray(),
                IsMockInvestment = this.khAPI.KOA_Functions("GetServerGubun", string.Empty) == "1"
            };
        }

        public void GetCurrentPrice(string stockCode)
        {
            this.apiRunQueue.Enqueue(new KeyValuePair<APIRunType, object[]>(APIRunType.GetCurrentPrice, new object[] { stockCode }));
        }

        private void API_GetCurrentPrice(string stockCode)
        {
            //APIRunner.Instance.ReservationAction(
            this.khAPI.SetInputValue("종목코드", stockCode.Trim());

            var ret = this.khAPI.CommRqData("주식기본정보" + "_" + stockCode, "OPT10001", 0, GetScrNum());
            Log.Info(Error.GetErrorMessage(ret));
        }

        public void GetMultiData(string stocks)
        {
            this.apiRunQueue.Enqueue(new KeyValuePair<APIRunType, object[]>(APIRunType.GetMultiData, new object[] { stocks }));
        }

        private void API_GetMultiData(string stocks)
        {
            var ret = this.khAPI.CommKwRqData(stocks, 1, stocks.Split(';').Length, 0, "주식복수정보_" + stocks, GetScrNum());
            Log.Info(Error.GetErrorMessage(ret));
        }

        //public void Test()
        //{
        //    //시장구분 = 000:전체, 001:코스피, 101:코스닥
        //    this.khAPI.SetInputValue("시장구분", "000");
        //    //정렬구분 = 1:거래량, 2:거래회전율, 3:거래대금
        //    this.khAPI.SetInputValue("정렬구분", "1");

        //    //관리종목포함 = 0:관리종목 포함, 1:관리종목 미포함
        //    this.khAPI.SetInputValue("관리종목포함", "0");

        //    var ret = this.khAPI.CommRqData("당일거래량", "OPT10030", 0, GetScrNum());
        //    Log.Info(Error.GetErrorMessage(ret));
        //    Thread.Sleep(200);
        //}

        //public void RequestSurgeAmount()
        //{
        //    //           시장구분 = 000:전체, 001:코스피, 101:코스닥
        //    this.khAPI.SetInputValue("시장구분", "000");

        //    //   정렬구분 = 1:급증량, 2:급증률
        //    this.khAPI.SetInputValue("정렬구분", "2");

        //    //   시간구분 = 1:분, 2:전일
        //    this.khAPI.SetInputValue("시간구분", "1");

        //    //   거래량구분 = 5:5천주이상, 10:만주이상, 50:5만주이상, 100:10만주이상, 200:20만주이상, 300:30만주이상, 500:50만주이상, 1000:백만주이상
        //    this.khAPI.SetInputValue("거래량구분", "5");

        //    //   시간 = 분 입력
        //    this.khAPI.SetInputValue("시간", "1");

        //    //   종목조건 = 0:전체조회, 1:관리종목제외, 5:증100제외, 6:증100만보기, 7:증40만보기, 8:증30만보기, 9:증20만보기
        //    this.khAPI.SetInputValue("종목조건", "0");

        //    //   가격구분 = 0:전체조회, 2:5만원이상, 5:1만원이상, 6:5천원이상, 8:1천원이상, 9:10만원이상
        //    this.khAPI.SetInputValue("가격구분", "0");


        //    //2. Open API 조회 함수를 호출해서 전문을 서버로 전송합니다.
        //    var ret = this.khAPI.CommRqData("거래량급증요청", "OPT10023", 0, GetScrNum());
        //    Log.Info(Error.GetErrorMessage(ret));
        //    Thread.Sleep(200);
        //}

        /// <summary>
        /// 체결강도
        /// </summary>
        /// <param name="timeType"></param>
        /// <param name="stockCode"></param>
        /// <param name="range"></param>
        /// <param name="date"></param>
        /// <param name="endDate"></param>
        /// <param name="scrNo"></param>
        /// <param name="next"></param>
        public void GetGetStrength(string stockCode)
        {
            this.apiRunQueue.Enqueue(new KeyValuePair<APIRunType, object[]>(APIRunType.GetStrength, new object[] { stockCode }));
        }

        private void API_GetStrength(string stockCode)
        {
            this.khAPI.SetInputValue("종목코드", stockCode);

            int ret = this.khAPI.CommRqData("체결강도", "OPT10005", 0, GetScrNum());
            //scrNum++;
            Log.Info(Error.GetErrorMessage(ret));
            //Thread.Sleep(200);
        }

        public void GetTimePrice(StockTimePrice.TimeType timeType, string stockCode, string range, string date = "", string endDate = "", string scrNo = "", int next = 0)
        {
            this.apiRunQueue.Enqueue(new KeyValuePair<APIRunType, object[]>(APIRunType.GetTimePrice, new object[] { timeType, stockCode, range, date, endDate, scrNo, next }));
        }

        /// <summary>
        /// param1 = 기준일 or 틱단위, param2 = 끝 날짜
        /// </summary>
        /// <param name="stockCode"></param>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        /// <param name="timeType"></param>
        private void API_GetTimePrice(StockTimePrice.TimeType timeType, string stockCode, string range, string date, string endDate, string scrNo, int next)
        {
            this.khAPI.SetInputValue("종목코드", stockCode);
            //string rqName = string.Empty;
            string code = string.Empty;

            switch (timeType)
            {
                case StockTimePrice.TimeType.Tick:
                    //틱범위 = 1:1틱, 3:3틱, 5:5틱, 10:10틱, 30:30틱
                    this.khAPI.SetInputValue("틱범위", range);
                    //rqName = "주식틱차트조회";
                    code = "OPT10079";
                    break;
                case StockTimePrice.TimeType.Minute:
                    //틱범위 = 1:1분, 3:3분, 5:5분, 10:10분, 15:15분, 30:30분, 45:45분, 60:60분
                    this.khAPI.SetInputValue("틱범위", range);
                    //rqName = "주식분봉차트조회";
                    code = "OPT10080";
                    break;
                case StockTimePrice.TimeType.Day:
                    //기준일자 = YYYYMMDD(20160101 연도4자리, 월 2자리, 일 2자리 형식)
                    this.khAPI.SetInputValue("기준일자", date);
                    //rqName = "주식일봉차트조회";
                    //code = "OPT10081";
                    break;
                case StockTimePrice.TimeType.Week:
                    //기준일자 = YYYYMMDD(20160101 연도4자리, 월 2자리, 일 2자리 형식)
                    this.khAPI.SetInputValue("기준일자", date);
                    this.khAPI.SetInputValue("끝일자", endDate);
                    //rqName = "주식주봉차트조회";
                    code = "OPT10082";
                    break;
                case StockTimePrice.TimeType.Month:
                    //기준일자 = YYYYMMDD(20160101 연도4자리, 월 2자리, 일 2자리 형식)
                    this.khAPI.SetInputValue("기준일자", date);
                    this.khAPI.SetInputValue("끝일자", endDate);
                    //rqName = "주식월봉차트조회";
                    code = "OPT10083";
                    break;
                //case StockTimePrice.TimeType.DayTrend:
                //    this.khAPI.SetInputValue("기준일자", date);
                //    //rqName = "분석이오";
                //    code = "OPT10081";
                //    break;
            }

            this.khAPI.SetInputValue("수정주가구분", "1");

            int ret = this.khAPI.CommRqData(string.Format("{0}_{1}_{2}_{3}_{4}", timeType.ToString(), stockCode, range, date, endDate), code, next, scrNo == "" ? GetScrNum() : scrNo);
            //scrNum++;
            Log.Info(Error.GetErrorMessage(ret));
            //Thread.Sleep(200);
        }

        public void GetUnfinishedConclud()
        {
            this.apiRunQueue.Enqueue(new KeyValuePair<APIRunType, object[]>(APIRunType.GetUnfinishedConclud, null));
        }

        public void API_GetUnfinishedConclud()
        {
            // 계좌번호 = 전문 조회할 보유계좌번호
            this.khAPI.SetInputValue("계좌번호", this.SelectedAccount);

            //   체결구분 = 0:전체, 1:종목
            this.khAPI.SetInputValue("체결구분", "0");

            //   매매구분 = 0:전체, 1:매도, 2:매수
            this.khAPI.SetInputValue("매매구분", "0");

            //2. Open API 조회 함수를 호출해서 전문을 서버로 전송합니다.
            this.khAPI.CommRqData("미체결정보", "opt10075", 0, GetScrNum());
        }

        public void Get매물대(string stCode, string startDate, string endDate, string range, string sort)
        {
            this.apiRunQueue.Enqueue(new KeyValuePair<APIRunType, object[]>(APIRunType.Get매물대, new object[] { stCode, startDate, endDate, range, sort }));
        }

        private void API_Get매물대(string stCode, string startDate, string endDate, string range, string sort)
        {
            //종목코드 = 전문 조회할 종목코드
            this.khAPI.SetInputValue("종목코드", stCode);

            //시작일자 = YYYYMMDD (20160101 연도4자리, 월 2자리, 일 2자리 형식)
            this.khAPI.SetInputValue("시작일자", startDate);

            //종료일자 = YYYYMMDD (20160101 연도4자리, 월 2자리, 일 2자리 형식)
            this.khAPI.SetInputValue("종료일자", endDate);

            //조회기간구분 = 1 or 0
            this.khAPI.SetInputValue("조회기간구분", "1");

            //시점구분 = 0:당일, 1:전일
            this.khAPI.SetInputValue("시점구분", "0");

            //기간 = 5:5일, 10:10일, 20:20일, 40:40일, 60:60일, 120:120일
            this.khAPI.SetInputValue("기간", range);

            //정렬기준 = 1:종가순, 2:날짜순
            this.khAPI.SetInputValue("정렬기준", sort);

            //회원사코드 = 사용안함
            this.khAPI.SetInputValue("회원사코드", "");
            int ret = this.khAPI.CommRqData("매물대", "OPT10043", 0, GetScrNum());
            Log.Info(Error.GetErrorMessage(ret));
        }

        //public override void Dispose(bool disposing)
        //{
        //    if (!disposed)
        //    {
        //        DisconnectAllRealData();
        //        this.apiRunThread.Abort();

        //        disposed = true;
        //    }

        //    base.Dispose(disposing);
        //}

        public void GetAccountInfo()
        {
            this.apiRunQueue.Enqueue(new KeyValuePair<APIRunType, object[]>(APIRunType.GetAccountInfo, null));
        }

        private void API_GetAccountInfo()
        {
            this.khAPI.SetInputValue("계좌번호", this.SelectedAccount);

            //비밀번호 = 사용안함(공백)

            this.khAPI.SetInputValue("비밀번호", "");

            //상장폐지조회구분 = 0:전체, 1:상장폐지종목제외

            this.khAPI.SetInputValue("상장폐지조회구분", "0");

            //비밀번호입력매체구분 = 00

            this.khAPI.SetInputValue("비밀번호입력매체구분", "00");

            //조회구분 = 1:합산, 2:개별

            //this.khAPI.SetInputValue("조회구분", "2");
            var ret = this.khAPI.CommRqData("계좌조회", "opw00004", 0, GetScrNum());
            Log.Info(Error.GetErrorMessage(ret));
        }

        public void SendOrder(string stCode, OrderType orderType, int orderCount, int orderPrice, HogaCode hogaCode, string originNo, int scheduleId = 0)
        {
            this.apiRunQueue.Enqueue(new KeyValuePair<APIRunType, object[]>(APIRunType.SendOrder, new object[] { stCode, orderType, orderCount, orderPrice, hogaCode, originNo, scheduleId }));
        }

        /// <summary>
        /// 주식주문 파라미터 참조
        /// </summary>
        /// <param name="stCode">주식코드</param>
        /// <param name="orderType">1:신규매수, 2:신규매도 3:매수취소, 4:매도취소, 5:매수정정, 6:매도정정</param>
        /// <param name="orderCount">주문수량</param>
        /// <param name="orderPrice">주문가격</param>
        /// <param name="hogaCode">호가 HogaGB 참조</param>
        /// <param name="originNo">원주문번호</param>
        private void API_SendOrder(string stCode, OrderType orderType, int orderCount, int orderPrice, HogaCode hogaCode, string originNo, int scheduleId = 0)
        {
            //계좌번호
            //OrderType
            //(1:신규매수, 2:신규매도 3:매수취소, 
            // 4:매도취소, 5:매수정정, 6:매도정정)

            //hogaGb[0] = new HogaGb("00", "지정가");
            //hogaGb[1] = new HogaGb("03", "시장가");
            //hogaGb[2] = new HogaGb("05", "조건부지정가");
            //hogaGb[3] = new HogaGb("06", "최유리지정가");
            //hogaGb[4] = new HogaGb("07", "최우선지정가");
            //hogaGb[5] = new HogaGb("10", "지정가IOC");
            //hogaGb[6] = new HogaGb("13", "시장가IOC");
            //hogaGb[7] = new HogaGb("16", "최유리IOC");
            //hogaGb[8] = new HogaGb("20", "지정가FOK");
            //hogaGb[9] = new HogaGb("23", "시장가FOK");
            //hogaGb[10] = new HogaGb("26", "최유리FOK");
            //hogaGb[11] = new HogaGb("61", "시간외단일가매매");
            //hogaGb[12] = new HogaGb("81", "시간외종가");

            int lRet = this.khAPI.SendOrder(string.Format("주식주문_{0}_{1}_{2}", stCode, scheduleId, orderType), GetScrNum(), this.SelectedAccount,
                                        Convert.ToInt32(orderType), stCode, orderCount,
                                        orderPrice, Convert.ToInt32(hogaCode).ToString("00"), originNo);

            if (lRet == 0)
            {
                Log.Info("주문이 전송 되었습니다");
            }
            else
            {
                Log.Info("주문이 전송 실패 하였습니다. [에러] : " + lRet);
            }
        }

        private bool GetStockTimePrice(string sTrCode, string rqName, string stCode, StockTimePrice.TimeType timeType, StockTimePrice timePrice, string date, string endDate)
        {
            return true;
        }

        public void GetCondition()
        {
            this.apiRunQueue.Enqueue(new KeyValuePair<APIRunType, object[]>(APIRunType.GetCondition, null));
        }

        private void API_GetCondition()
        {
            var ret = this.khAPI.GetConditionLoad();
            Log.Info(Error.GetErrorMessage(ret));
        }

        /// <summary>
        /// 조건검색
        /// </summary>
        /// <param name="conditionIndex">인덱스</param>
        /// <param name="conditionName">이름</param>
        /// <param name="nSearch">:조건검색, 1:실시간 조건검색</param>
        public void SendCondition(int conditionIndex, string conditionName, int nSearch)
        {
            this.apiRunQueue.Enqueue(new KeyValuePair<APIRunType, object[]>(APIRunType.SendCondition, new object[] { conditionIndex, conditionName, nSearch }));
        }

        /// <summary>
        /// 조건검색
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="nSearch">0:조건검색, 1:실시간 조건검색</param>
        private void API_SendCondition(int conditionIndex, string conditionName, int nSearch)
        {
            var ret = this.khAPI.SendCondition(GetScrNum(), conditionName, conditionIndex, nSearch);
            Log.Info(Error.GetErrorMessage(ret));
        }

        /// <summary>
        /// 수수료 후 매도 예상 가격
        /// </summary>
        /// <param name="price"></param>
        /// <returns></returns>
        public int CalcPriceAfterFees(int price)
        {
            return price - (int)(price * GetFees());
        }

        public double GetFees()
        {
            return StockDefines.RealFees;// this.UserInfo == null ? Defines.RealFees : this.UserInfo.IsMockInvestment ? Defines.MockInvestmentFees : Defines.RealFees;
        }

        public string[] GetAllStockStcode(bool isKospi)
        {
            var marketCodes = this.khAPI.GetCodeListByMarket(isKospi ? "0" : "10").Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries); // 코스피, 코스닥
            return marketCodes;
        }        

        private T CreateInstance<T>(string sTrCode, string sRQName, int index)
        {
            var instance = Activator.CreateInstance<T>();

            var type = typeof(T);
            var properties = type.GetProperties();

            foreach (var property in properties)
            {
                var value = this.khAPI.CommGetData(sTrCode, "", sRQName, index, property.Name).Trim();
                var objValue = new Object();

                switch (property.PropertyType.Name.ToLower())
                {
                    case "int32":
                        objValue = Convert.ToInt32(value);
                        break;
                    case "double":
                        objValue = Convert.ToDouble(value);
                        break;
                    default:
                        objValue = value;
                        break;
                }

                Common.Util.SetPropertyValue(property, instance, objValue);
            }

            return instance;
        }
        #endregion
        
        #region | Dispose |
        public new void Dispose()
        {
            this.Dispose(true);
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                DisconnectAllRealData();
                this.apiRunThread.Abort();

                disposed = true;
            }
        }
        #endregion
    }
}
