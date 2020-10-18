using System;
using System.Collections.Generic;
using AmidUs.Utils;

namespace AmidUs
{
    public class CircularList<T>
    {
        public CircularList(IEnumerable<T> list)
        {
            _nextIndex = 0;
            _list = new List<T>();
            foreach (var item in list)
            {
                Append(item);
            }
        }

        public void Shuffle()
        {
            var rng = new Random();
            rng.Shuffle(_list);
        }

        public T GetNext()
        {
            if (_list.Count == 0)
            {
                return default;
            }

            if (_nextIndex > _list.Count - 1)
            {
                _nextIndex = 0;
            }

            return _list[_nextIndex++];
        }

        public void Append(T item)
        {
            _list.Add(item);
        }

        public T GetAt(int index)
        {
            return _list[index];
        }

        public T GetLast()
        {
            return _list[_list.Count - 1];
        }

        private List<T> _list;
        private int _nextIndex;
    }
}