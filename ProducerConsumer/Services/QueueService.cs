namespace ProducerConsumer.Services
{
    [Obsolete]
    public class QueueService<T> where T: class
    {
        private Queue<T> queue = new();
        private SemaphoreSlim left;
        private SemaphoreSlim curr = new(0);

        public QueueService(int capacity)
        {
            left = new(capacity);
        }

        public void Enqueue(T item)
        {
            Console.WriteLine("S:"+left.CurrentCount);
            lock (left)
            {
                left.Wait();
                queue.Enqueue(item);
                curr.Release();
            }
        }

        public T Dequeue()
        {
            lock (curr)
            {
                curr.Wait();
                T item = queue.Dequeue();
                left.Release();
                return item;
            }
        }

        public int Size()
        {
            return queue.Count;
        }
    }
}

