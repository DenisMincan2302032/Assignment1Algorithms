using System;
using System.Collections.Generic;

namespace Lab_Week_1_Friday
{
    public class LinkedQueue<T>
    {
        // Assuming LinkedList<T> is the fixed version from the previous answers.
        private LinkedList<T> _list = new LinkedList<T>();

        // Adds a new item at the back of the queue (FIFO)
        public void Enqueue(T item)
        {
     
            // A truly efficient queue using a custom LinkedList requires a Head AND a Tail pointer
            // in the LinkedList class.

            // However, to fix the compilation error while retaining the current logic:

            if (_list.Count() == 0)
            {
                _list.pushFront(item);
                return;
            }

            // Temporarily store all existing elements
            var temp = new List<T>();
            T frontItem = default(T); // Use default(T) for initialization

            while (_list.Count() > 0)
            {
   
                // Note: GetFront requires a 'ref' parameter, which is a bit unusual for a Peek/Pop pattern.
                _list.GetFront(ref frontItem);
                temp.Add(frontItem);
                _list.popFront();
            }

            // Push the new item to the front (which is the new tail of the queue)
            _list.pushFront(item);

            // Re-push all old items back in reverse order (preserving FIFO queue order)
            for (int i = temp.Count - 1; i >= 0; i--)
            {
                _list.pushFront(temp[i]);
            }
        }

        // Removes an item from the front of the queue (FIFO)
        public bool Dequeue(ref T value)
        {
            // Dequeueing from the front is correct and efficient
            bool result = _list.GetFront(ref value);
            if (result)
            {
                _list.popFront();
            }
            return result;
        }

        // Returns the front item of the queue as long as the queue is not empty
        public T? Peek()
        {
            // Use GetFront(ref T) to retrieve the value.
            T value = default(T);
            bool success = _list.GetFront(ref value);

            // Return the value if successful, otherwise return default/null
            if (success)
            {
                return value;
            }
            return default; // Returns default(T) or null if T is a reference type
        }

        // Returns the number of items in the queue
        public int Length()
        {
            return _list.Count();
        }

        // Empties the queue
        public void Clear()
        {
            while (_list.Count() > 0)
            {
                _list.popFront();
            }
        }
    }
}