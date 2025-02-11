using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulabs_Burse_Console
{
    internal class LimitedQueue<T> : Queue<T>
    {
        private readonly uint _limit;

        public LimitedQueue(uint limit) :base((int)limit)
        {
            _limit = limit;
        }

        public new void Enqueue(T obj)
        {
            if (Count == _limit) Dequeue();
            base.Enqueue(obj);
        }

        /**
         * makes new array with all the queue's items
         * with the oldest as arr[0] and the newest as arr[^1]
         * @return queue as array
         */
        public T[] AsArray()
        {
            T[] res = new T[Count];
            int i = 0;
            foreach (T obj in this)
            {
                res[i++] = obj;
            }
            return res;
        }
    }
}
