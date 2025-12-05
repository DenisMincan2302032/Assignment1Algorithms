using System.Linq;

namespace Lab_Week_1_Friday
{
    public class LinkedList<T>
    {
        private class Node
        {
            public T Data;
            public Node Next;
            public Node Previous;

            public Node(T data)
            {
                Data = data;
                Next = null;
                Previous = null;
            }
        }

        private Node ?Head;
        private int count = 0;

        public int Count()
        {
            return count;
        }

        public void pushFront(T item)
        {
            Node newNode = new Node(item);
            if (Head != null)
            {
                newNode.Next = Head;
                Head.Previous = newNode;
            }
            Head = newNode;
            count++;
        }


        public bool GetFront(ref T item)
        {
            if (Head != null)
            {
                item = Head.Data;
                return true;
            }
            item = default(T);
            return false;
        }

        public bool popFront()
        {
            if (Head == null) return false;

            Head = Head.Next;
            if (Head != null)
            {
                Head.Previous = null;
            }
            count--;
            return true;
        }
    }
}