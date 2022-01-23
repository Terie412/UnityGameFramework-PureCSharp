using System.Collections.Generic;
using System.Threading;

public class BlockQueue<T>
{
    private readonly Queue<T> queue = new Queue<T>();

    public void Enqueue(T item)
    {
        lock (queue)
        {
            queue.Enqueue(item);
            if (queue.Count == 1)
            {
                Monitor.PulseAll(queue);
            }
        }
    }
    public T Dequeue()
    {
        lock (queue)
        {
            while (queue.Count == 0)
            {
                Monitor.Wait(queue);
            }
            T item = queue.Dequeue();
            return item;
        }
    }

    public int Count
    {
        get
        {
            lock (queue)
            {
                return queue.Count;
            };
        }

    }
}