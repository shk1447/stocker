using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectorInterface.Class
{
    /// <summary>
    /// Set to Lock Dictionary, Get to ReadOnlyDictionary
    /// </summary>
    /// /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class LockDictionary<TKey, TValue>
    {
        #region | Private |
        Dictionary<TKey, TValue> lockCollectionDict;
        ReadOnlyDictionary<TKey, TValue> readOnlyDict;
        #endregion

        #region | Ctor |
        /// <summary>
        /// CRUD 시 Lock 처리하는 Collection
        /// </summary>
        public LockDictionary()
        {
            this.lockCollectionDict = new Dictionary<TKey, TValue>();
            this.readOnlyDict = new ReadOnlyDictionary<TKey, TValue>(this.lockCollectionDict);
        }

        public LockDictionary(TKey key, TValue value)
            : this()
        {
            Add(key, value);
        }
        #endregion

        #region | This |
        /// <summary>
        /// Get Set Collection Key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TValue this[TKey key]
        {
            get
            {
                return this.readOnlyDict[key];
            }
            set
            {
                lock (this)
                {
                    this.lockCollectionDict[key] = value;
                }
            }
        }

        public virtual IEnumerable<TKey> Keys
        {
            get
            {
                return this.readOnlyDict.Keys.ToList();
            }
        }

        public virtual IEnumerable<TValue> Values
        {
            get
            {
                return this.readOnlyDict.Values.ToList();
            }
        }
        #endregion

        #region | Properties |
        public int Count
        {
            get
            {
                return this.readOnlyDict.Count;
            }
        }
        #endregion

        #region | Method |
        /// <summary>
        /// Add Item
        /// </summary>
        /// <param name="objectID"></param>
        public bool Add(TKey key, TValue value)
        {
            lock (this.lockCollectionDict)
            {
                if (!this.lockCollectionDict.ContainsKey(key))
                {
                    this.lockCollectionDict.Add(key, value);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// ContainsKey
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(TKey key)
        {
            return this.readOnlyDict.ContainsKey(key);
        }

        /// <summary>
        /// Remove Item
        /// </summary>
        /// <param name="key"></param>
        public void Remove(TKey key)
        {
            lock (this.lockCollectionDict)
            {
                if (this.lockCollectionDict.ContainsKey(key))
                {
                    this.lockCollectionDict.Remove(key);
                }
            }
        }

        /// <summary>
        /// Clear Object ID List
        /// </summary>
        public void Clear()
        {
            lock (this.lockCollectionDict)
            {
                this.lockCollectionDict.Clear();
            }
        }

        /// <summary>
        /// Get Read Only Dictionary
        /// </summary>
        /// <returns></returns>
        public ReadOnlyDictionary<TKey, TValue> GetReadOnlyListDictionary()
        {
            return this.readOnlyDict;
        }

        public KeyValuePair<TKey, TValue> FirstOrDefault()
        {
            return this.readOnlyDict.FirstOrDefault();
        }

        public KeyValuePair<TKey, TValue> FirstOrDefault(Func<KeyValuePair<TKey, TValue>, bool> predicate)
        {
            return this.readOnlyDict.FirstOrDefault(predicate);
        }
        #endregion
    }
}
