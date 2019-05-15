using Common.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interface.DataModel
{
    public class TradeStateModel : BaseViewModel
    {
        public enum TradeState
        {
            대기, 매수, 매도
        }

        private TradeState state;
        public TradeState State
        {
            get { return state; }
            set
            {
                state = value;
                OnPropertyChanged("State");
            }
        }

        private int sequence;
        public int Sequence
        {
            get { return sequence; }
            set
            {
                sequence = value;
                OnPropertyChanged("Sequence");
            }
        }

    }
}
