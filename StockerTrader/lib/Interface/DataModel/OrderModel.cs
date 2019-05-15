using Common.Base;
using Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interface.Model
{
    public class OrderModel : BaseViewModel
    {
        public override string _id { get; set; }
        public string StCode { get; set; }
        public string StName { get; set; }
        public long OrderUnixTime { get; set; }

        private OrderType orderType;
        /// <summary>
        /// 주문타입 1:신규매수, 2:신규매도 3:매수취소, 4:매도취소, 5:매수정정
        /// </summary>
        public OrderType OrderType
        {
            get
            {
                return orderType;
            }
            set
            {
                orderType = value;
                OnPropertyChanged("OrderType");
            }
        }

        private int currentPrice;
        /// <summary>
        /// 현재가
        /// </summary>
        public int CurrentPrice
        {
            get
            {
                return currentPrice;
            }
            set
            {
                currentPrice = value;
                OnPropertyChanged("CurrentPrice");
            }
        }

        private int orderPrice;
        /// <summary>
        /// 주문가격
        /// </summary>
        public int OrderPrice
        {
            get
            {
                return orderPrice;
            }
            set
            {
                orderPrice = value;
                OnPropertyChanged("OrderPrice");
            }
        }


        private int orderCount;
        /// <summary>
        /// 주문수량
        /// </summary>
        public int OrderCount
        {
            get
            {
                return orderCount;
            }
            set
            {
                orderCount = value;
                OnPropertyChanged("OrderCount");
            }
        }


        private HogaCode hogaCode;
        /// <summary>
        /// 호가코드
        /// </summary>
        public HogaCode HogaCode
        {
            get
            {
                return hogaCode;
            }
            set
            {
                hogaCode = value;
                OnPropertyChanged("HogaCode");
            }
        }


    }
}
