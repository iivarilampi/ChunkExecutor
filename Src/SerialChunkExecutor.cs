using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChunkExecutor
{
    public class SerialChunkExecutor : IChunkExecutor
    {
        private readonly int _chunkSize;
        private readonly TimeSpan _delayBetweenChunks;
        private int _executedItemCount;

        public SerialChunkExecutor(int chunkSize, TimeSpan? delayBetweenChunks = null)
        {
            _chunkSize = chunkSize;
            _executedItemCount = 0;
            _delayBetweenChunks = delayBetweenChunks.HasValue ? delayBetweenChunks.Value : TimeSpan.Zero;
        }

        public async Task<int> Execute(Func<IEnumerable, Task> method, IEnumerable objectList)
        {
            var chunk = new List<object>(); 
            _executedItemCount = 0;

            foreach (var listObject in objectList)
            {
                chunk.Add(listObject);

                if (chunk.Count == _chunkSize)
                {
                    await method(chunk);
                    _executedItemCount += chunk.Count;
                    chunk.Clear();
                    Thread.Sleep(_delayBetweenChunks);
                }
            }

            // Execute the last chunk
            if (chunk.Count > 0)
            {
                await method(chunk);
                _executedItemCount += chunk.Count;
            }

            return _executedItemCount;
        }

        public int ExecutedItemCount {
            get { return _executedItemCount; }
        }

        public IEnumerable FailedObjects { get; }
    }
}