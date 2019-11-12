using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QChat.Common.Collections
{
    public struct CapacityArray<T>
    {
        private T[] _array;
        private int _count;
        private int _capacity;

        public int Count { get => _count; }
        public int Capacity { get => _capacity; }

        public CapacityArray(int capacity)
        {
            _array = new T[capacity];
            _capacity = capacity;
            _count = 0;
        }

        public bool Add(T item)
        {
            if (_count == _capacity) return false;

            _array[_count] = item;
            _count++;

            return true;       
        }

        //TODO: Remove method

        public T Last()
        {
            return _array[_count - 1];
        }

        public T this[int index]
        {
            get
            {
                if (index >= _capacity && index < 0) throw new IndexOutOfRangeException();
                return _array[index];
            }
            set
            {
                if (index >= _capacity && index < 0) throw new IndexOutOfRangeException();
                _array[index] = value;
            }
        }
    }
}
