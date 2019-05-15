using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Base
{
    public class BaseViewModel : INotifyPropertyChanged, IDisposable
    {
        protected bool disposed = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public virtual string _id { get; set; }
        public virtual void EditItem<T>(T info) { }
        public virtual void EditItem<T>(IEnumerable<T> info) { }
        public virtual void RemoveItem<T>(T info) { }
        public virtual void RemoveItem<T>(IEnumerable<T> info) { }

        public void Dispose() { }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;
            }
        }
    }

    public static class BaseViewModelExtensions
    {
        public static string NextBaseId(this IEnumerable<BaseViewModel> baseCollection)
        {
            var idNum = 1;
            if (baseCollection.Any())
            {
                var maxValue = baseCollection.Max(x => Convert.ToInt32(x._id));
                idNum = maxValue + 1;
            }

            return idNum.ToString();
        }

        public static void ResetBaseId(this IEnumerable<BaseViewModel> baseCollection)
        {
            int sequence = 0;
            foreach (var item in baseCollection)
            {
                item._id = (++sequence).ToString();
            }
        }
    }
}
