namespace Configurations.Extensions
{
    /// <summary>
    /// Allows to split large list to smaller batches for further processing
    /// </summary>
    public static class ListExtensions
    {
        public static List<List<T>> GetQueues<T>(this List<T> items, int limit)
        {
            var queues = new List<List<T>>();
            for (var i = 0; i < items.Count; i++)
            {
                if (i % limit == 0)
                {
                    var queue = new List<T>();
                    queues.Add(queue);
                }

                queues.Last().Add(items[i]);
            }

            return queues;
        }
    }
}
