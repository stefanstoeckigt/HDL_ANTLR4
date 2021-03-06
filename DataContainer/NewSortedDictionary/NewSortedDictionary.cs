using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;

namespace DataContainer.MySortedDictionary
{
    [System.Serializable]
    public class NewSortedDictionary<TValue> : 
                                              IDictionary<UInt64, TValue>,
                                              ICollection<KeyValuePair<UInt64, TValue>>,
                                              IEnumerable<KeyValuePair<UInt64, TValue>>,
                                              IDictionary,
                                              ICollection,
                                              IEnumerable
    {
        #region Properties

        /// <summary>
        /// Класс, используемый при сортировке для сравнения ключей
        /// </summary>
        private IComparer<UInt64> comparer;
        public IComparer<UInt64> Comparer
        {
            get { return comparer; }
            set { comparer = value; }
        }

        /// <summary>
        /// Список ключей
        /// </summary>
        private List<UInt64> keys;
        public List<UInt64> Keys
        {
            get { return keys; }
        }

        /// <summary>
        /// Хранимые данные
        /// </summary>
        private List<TValue> values;
        public List<TValue> Values
        {
            get { return values; }
        }

        /// <summary>
        /// Количество елементов
        /// </summary>
        public int Count
        {
            get { return values.Count; }
        }

        #endregion

        #region Constructors
        public NewSortedDictionary()
        {
            keys = new List<UInt64>();
            values = new List<TValue>();
            comparer = Comparer<UInt64>.Default;
        }
        public NewSortedDictionary(IComparer<UInt64> comparer)
        {
            this.comparer = comparer;
            keys = new List<UInt64>();
            values = new List<TValue>();
        }
        public NewSortedDictionary(IDictionary<UInt64, TValue> dictionary)
        {
            this.comparer = Comparer<UInt64>.Default;
            keys = new List<UInt64>();
            values = new List<TValue>();
            foreach (UInt64 key in dictionary.Keys)
            {
                Add(key, dictionary[key]);
            }
        }

        #endregion

        #region Search

        /// <summary>
        /// Добавление нового события по транспортной задержке
        /// </summary>
        /// <param name="Time"></param>
        /// <param name="Value"></param>
        public void AddTransportEvent(UInt64 Time, TValue Value)
        {
            if ((Keys.Count == 0) || (Time > Keys[Keys.Count - 1]))
                Append(Time, Value);
            if(EndTime > Time)
                ClearValueToEnd(Time);
            Append(Time, Value);
        }

        /// <summary>
        /// Добавление нового события по инерционной задержке
        /// </summary>
        /// <param name="Time"></param>
        /// <param name="Value"></param>
        public void AddInertialEvent(UInt64 CurrentTime, UInt64 Time, TValue Value)
        {
            if((Keys.Count == 0) || (Time > Keys[Keys.Count-1]))
                Append(Time, Value);

            if (EndTime > Time)
                ClearValueToEnd(Time);

            int eventIndex = BinarySearchKey(Time);
            UInt64 eventTime = Keys[eventIndex];
            TValue eventValue = Values[eventIndex];

            int current_eventIndex = BinarySearchKey(CurrentTime);
            UInt64 current_eventTime = Keys[current_eventIndex];
            TValue current_eventValue = Values[current_eventIndex];

            ClearDataInRange(CurrentTime, Time);
            Append(current_eventTime, current_eventValue);

            if (eventValue.Equals(Value))
            {
                Append(eventTime, eventValue);
            }
            else
            {
                Append(Time, Value);
            }
        }


        /// <summary>
        /// Добавление нового события о смешаной задержке
        /// </summary>
        /// <param name="Time"></param>
        /// <param name="Value"></param>
        /// <param name="Rejection"></param>
        public void AddInertialEvent(UInt64 CurrentTime, UInt64 Time, TValue Value, UInt64 Rejection)
        {
            if ((Keys.Count == 0) || (Time > Keys[Keys.Count - 1]))
                Append(Time, Value);

            if (EndTime > Time)
                ClearValueToEnd(Time);

            int eventIndex = BinarySearchKey(Time);
            UInt64 eventTime = Keys[eventIndex];
            TValue eventValue = Values[eventIndex];

            int current_eventIndex = BinarySearchKey(CurrentTime);
            UInt64 current_eventTime = Keys[current_eventIndex];
            TValue current_eventValue = Values[current_eventIndex];

            if(Time > Rejection)
                ClearDataInRange(Time - Rejection, Time);
            else
                ClearDataInRange(CurrentTime, Time);

            if (eventValue.Equals(Value))
            {
                Append(eventTime, eventValue);
            }
            else
            {
                Append(Time, Value);
            }
        }

        /// <summary>
        /// Возвращает индекс елемента зная его ключ
        /// Возвращает -1 при неудачном поиске
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int BinarySearchKeyIndex(UInt64 key)
        {
            int left_index = 0;
            int right_index = keys.Count - 1;
            int index = (left_index + right_index) / 2;

            do
            {
                index = (left_index + right_index) / 2;
                int compareresult = comparer.Compare(key, keys[index]);

                if (compareresult < 0)
                {
                    right_index = index;
                    continue;
                }
                if (compareresult > 0)
                {
                    left_index = index;
                    continue;
                }
                if (compareresult == 0)
                    return index;
            }
            while (right_index - left_index > 3);


            for (index = left_index; index <= right_index; index++)
            {
                int compareresult = comparer.Compare(key, keys[index]);
                if (compareresult == 0)
                    return index;
            }

            return -1;
        }

        /// <summary>
        /// Возвращает индекс по которому нужно втавить новый элемент
        /// Возвращает -1 при в случае когда данный элемент присутствует в словаре
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private int BinarySearchForInsert(UInt64 newKey, TValue newValue)
        {
            if (keys.Count == 0)
                return 0;

            int left_index = 0;
            int right_index = keys.Count - 1;
            int index = 0;

            do
            {
                index = (left_index + right_index) / 2;
                int compareresult = comparer.Compare(newKey, keys[index]);

                if (compareresult < 0)
                {
                    right_index = index;
                    continue;
                }
                if (compareresult > 0)
                {
                    left_index = index;
                    continue;
                }
                if (compareresult == 0)
                    return -1;
            }
            while (right_index - left_index > 3);

            for (index = left_index; index <= right_index; index++)
            {
                int compareresult = comparer.Compare(newKey, keys[index]);
                if (compareresult == 0)
                    return index;
                if (compareresult < 0)
                {
                    if ((index >= 1) && (newValue.Equals(values[index - 1])))
                        return -1;
                    else
                        return index;
                }
            }
            if (newValue.Equals(values[right_index]))
                return -1;
            else
                return right_index + 1;
        }


        /// <summary>
        /// Выполняет поиск в случае когда значение key не находится в списке ключей
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int BinarySearchKey(UInt64 key)
        {
            if (keys.Count == 0)
                return 0;

            int left_index = 0;
            int right_index = keys.Count - 1;
            int index = 0;

            do
            {
                index = (left_index + right_index) / 2;
                int compareresult = comparer.Compare(key, keys[index]);

                if (compareresult < 0)
                {
                    right_index = index;
                    continue;
                }
                if (compareresult > 0)
                {
                    left_index = index;
                    continue;
                }
                if (compareresult == 0)
                    return index;
            }
            while (right_index - left_index > 3);

            int res = index;
            for (index = left_index; index <= right_index; index++)
            {
                int compareresult = comparer.Compare(key, keys[index]);
                if (compareresult == 0)
                    return index;
                if (compareresult < 0)
                    return res;
                else
                {
                    res = index;
                    continue;
                }
            }
            return right_index;
        }

        #endregion

        #region IDictionary<UInt64,TValue> Members

        public void Add(UInt64 key, TValue value)
        {
            int index = BinarySearchForInsert(key, value);
            if (index != -1)
            {
                if ((keys.Count > index) && (keys[index] == key))
                {
                    values[index] = value;
                }
                else
                {
                    keys.Insert(index, key);
                    values.Insert(index, value);
                }                    
            }
        }

        public bool ContainsKey(UInt64 key)
        {
            return (BinarySearchKeyIndex(key) != -1);
        }

        ICollection<UInt64> IDictionary<UInt64, TValue>.Keys
        {
            get
            {
                return keys;
            }
        }
        ICollection<TValue> IDictionary<UInt64, TValue>.Values
        {
            get
            {
                return values;
            }
        }


        public bool Remove(UInt64 key)
        {
            int index = BinarySearchKeyIndex(key);
            if (index == -1)
                return false;
            else
            {
                keys.RemoveAt(index);
                values.RemoveAt(index);
                return true;
            }
        }

        public bool TryGetValue(UInt64 key, out TValue value)
        {
            int index = BinarySearchKeyIndex(key);
            if (index == -1)
            {
                value = default(TValue);
                return false;
            }
            else
            {
                value = values[index];
                return true;
            }
        }

        public TValue this[UInt64 key]
        {
            get
            {
                int index = BinarySearchKeyIndex(key);
                if (index == -1)
                {
                    throw new Exception("invalid key");
                }
                else
                {
                    return values[index];
                }
            }
            set
            {
                int index = BinarySearchKeyIndex(key);
                if (index == -1)
                {
                    throw new Exception("invalid key");
                }
                else
                {
                    values[index] = value;
                }
            }
        }

        #endregion

        #region IDictionary Members

        public void Add(object key, object value)
        {
            if ((key is UInt64) && (value is TValue))
            {
                Add((UInt64)key, (TValue)value);
            }
            else
            {
                throw new Exception("Invalid type of key or value");
            }
        }

        public bool Contains(object key)
        {
            if (key is UInt64)
            {
                return (BinarySearchKeyIndex((UInt64)key) != -1);
            }
            else
                return false;
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new NewSortedDictionaryEnumerator<TValue>(this);
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        ICollection IDictionary.Keys
        {
            get { return keys; }
        }

        ICollection IDictionary.Values
        {
            get { return values; }
        }

        public void Remove(object key)
        {
            if (key is UInt64)
            {
                int index = BinarySearchKeyIndex((UInt64)key);
                if (index != -1)
                {
                    keys.RemoveAt(index);
                    values.RemoveAt(index);
                }
            }
        }

        public object this[object key]
        {
            get
            {
                if (key is UInt64)
                {
                    int index = BinarySearchKeyIndex((UInt64)key);
                    if (index != -1)
                        return values[index];
                }
                throw new Exception("Cant find key in dictionary");
            }
            set
            {
                if ((key is UInt64) && (value is TValue))
                {
                    int index = BinarySearchKeyIndex((UInt64)key);
                    if (index != -1)
                        values[index] = (TValue)value;
                }
                throw new Exception("Cant find key in dictionary");
            }
        }

        #endregion

        #region ICollection<KeyValuePair<UInt64,TValue>> Members

        public void Add(KeyValuePair<UInt64, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            keys.Clear();
            values.Clear();
        }

        public bool Contains(KeyValuePair<UInt64, TValue> item)
        {
            int index = BinarySearchKeyIndex(item.Key);
            if (index != -1)
            {
                if (values[index].Equals(item.Value) == true)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        public void CopyTo(KeyValuePair<UInt64, TValue>[] array, int arrayIndex)
        {
            for (int i = 0; i < Count; i++)
            {
                array[arrayIndex + i] = new KeyValuePair<UInt64, TValue>(keys[i], values[i]);
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<UInt64, TValue> item)
        {
            int index = BinarySearchKeyIndex(item.Key);
            if (index != -1)
            {
                if (values[index].Equals(item.Value) == true)
                {
                    keys.RemoveAt(index);
                    values.RemoveAt(index);
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }

        #endregion

        #region ICollection Members

        public void CopyTo(Array array, int index)
        {
            for (int i = 0; i < Count; i++)
            {
                array.SetValue(new KeyValuePair<UInt64, TValue>(keys[i], values[i]), index + i);
            }
        }

        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!           Дописать воддержку многопоточности
        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        public bool IsSynchronized
        {
            get { return false; }
        }

        public object SyncRoot
        {
            get { return null; }
        }

        #endregion

        #region IEnumerable<KeyValuePair<UInt64,TValue>> Members

        public IEnumerator<KeyValuePair<UInt64, TValue>> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return new KeyValuePair<UInt64, TValue>(keys[i], values[i]);
            }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return new KeyValuePair<UInt64, TValue>(keys[i], values[i]);
            }
        }

        #endregion

        /// <summary>
        /// Добавление нового значения в конец словаря
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Append(UInt64 key, TValue value)
        {
            if (Count == 0)
            {
                keys.Add(key);
                values.Add(value);
            }
            else
            {
                UInt64 lasUInt64 = keys[Count - 1];
                if (lasUInt64 > key)
                {
                    if (values[Count - 1].Equals(value) == false)
                    {
                        values.Add(value);
                        keys.Add(key);
                    }
                }
                if (lasUInt64 == key)
                {
                    values[Count - 1] = value;
                    keys[Count - 1] = key;
                }
                if (lasUInt64 < key)
                    Add(key, value);
            }
        }

        /// <summary>
        /// установка значения Value начиная со значения start и заканчивая end
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="Value"></param>
        public void SetValueInRange(UInt64 start, UInt64 end, TValue Value)
        {
            if (comparer.Compare(start, end) < 0)
            {
                TValue oldValue = GetValue(end);
                int startIndex = BinarySearchKey(start);
                int endIndex = BinarySearchKey(end);
                keys.RemoveRange(startIndex, endIndex - startIndex + 1);
                values.RemoveRange(startIndex, endIndex - startIndex + 1);
                Add(start, Value);
                Add(end, oldValue);
            }
            else
                throw new Exception("Invalid diapasone");
        }

        /// <summary>
        /// "Слитие воедино двоих словарей"
        /// будет применяться для вставки результатов работы генераторов
        /// </summary>
        /// <param name="data"></param>
        public void InsertData(NewSortedDictionary<TValue> data)
        {
            int startIndex = BinarySearchKey(data.keys[0]);
            int endIndex = BinarySearchKey(data.keys[data.Count-1]);
            keys.RemoveRange(startIndex, endIndex - startIndex + 1);
            values.RemoveRange(startIndex, endIndex - startIndex + 1);
            keys.InsertRange(startIndex, data.keys);
            values.InsertRange(startIndex, data.values);
        }

        public TValue GetValue(UInt64 key)
        {
            if (Count == 0)
                return default(TValue);
            else
                return values[BinarySearchKey(key)];
        }

        /// <summary>
        /// Удаляет все данные на промежутке времени от start до end
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public void ClearDataInRange(UInt64 start, UInt64 end)
        {
            int start_index = BinarySearchKey(start);
            int end_index   = BinarySearchKey(end);

            if ((keys.Count > start_index) && (keys[start_index] < start))
                start_index++;

            if ((keys.Count > end_index) && (keys[end_index] <= end))
                end_index++;

            if ((end_index - start_index) >= 1)
            {
                keys.RemoveRange(start_index, end_index - start_index);
                values.RemoveRange(start_index, end_index - start_index);
            }
        }

        /// <summary>
        /// Функция, используемая для вставки данных
        /// </summary>
        /// <param name="data"></param>
        /// <param name="StartTime"></param>
        /// <param name="EndTime"></param>
        public void InsertValues(SortedList<ulong, TValue> data, UInt64 StartTime, UInt64 EndTime)
        {
            //Запоминаем старые значения
            TValue end = GetValue(EndTime);
            TValue start = GetValue(StartTime);

            UInt64 start_time = (keys.Count != 0) ? keys[BinarySearchKey(StartTime)] : 0;

            ClearDataInRange(StartTime, EndTime);
            if (start_time != StartTime)
                Add(start_time, start);

            //Добавляем новые значения
            foreach (KeyValuePair<UInt64, TValue> v in data)
            {
                Add(v.Key, v.Value);
            }

            //Восстанавливаем старые значения
            if((end != null) && (end.Equals(data.Values[data.Count - 1]) == false))
                Add(EndTime, end);
        }

        /// <summary>
        /// Удаляет все записи, начиная со времени startTime и до конца
        /// </summary>
        /// <param name="startTime"></param>
        public void ClearValueToEnd(UInt64 startTime)
        {
            int start_index = BinarySearchKey(startTime);
            int end_index = Count - 1;

            if ((end_index - start_index) >= 1)
            {
                keys.RemoveRange(start_index, end_index - start_index);
                values.RemoveRange(start_index, end_index - start_index);
            }
        }

        /// <summary>
        /// Максимальный размер словаря
        /// </summary>
        public int Capacity
        {
            get
            {
                return 100000;//Math.Min(keys.Capacity, values.Capacity);
            }
        }


        /// <summary>
        /// Начальное время
        /// </summary>
        public UInt64 StartTime
        {
            get 
            {
                if (keys.Count == 0)
                    return 0;
                else
                    return keys[0];
            }
        }

        /// <summary>
        /// Конечное время
        /// </summary>
        public UInt64 EndTime
        {
            get
            {
                if (Keys.Count == 0)
                    return 0;
                else
                    return Keys[Count - 1];
            }
        }
    }
}
