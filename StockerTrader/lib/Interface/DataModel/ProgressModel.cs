using Common.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interface.Model
{
    public class ProgressModel : BaseViewModel
    {
        private double minimum;
        public double Minimum
        {
            get
            {
                return minimum;
            }
            set
            {
                minimum = value;
                OnPropertyChanged("Minimum");
            }
        }

        private double maximum;
        public double Maximum
        {
            get
            {
                return maximum;
            }
            set
            {
                maximum = value;
                OnPropertyChanged("Maximum");
            }
        }

        private double current;
        public double Current
        {
            get
            {
                return current;
            }
            set
            {
                current = value;
                OnPropertyChanged("Current");
            }
        }

        private int percent;
        public int Percent
        {
            get
            {
                return percent;
            }
            set
            {
                percent = value;
                OnPropertyChanged("Percent");
            }
        }
    }
}
