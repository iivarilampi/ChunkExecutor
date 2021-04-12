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
        private readonly List<object> _failedItems;

        public SerialChunkExecutor(int chunkSize, TimeSpan? delayBetweenChunks = null)
        {
            _chunkSize = chunkSize;
            ExecutedItemCount = 0;
            _delayBetweenChunks = delayBetweenChunks.HasValue ? delayBetweenChunks.Value : TimeSpan.Zero;
            _failedItems = new List<object>();
        }

        public async Task<int> Execute(Func<IEnumerable, Task> method, IEnumerable objectList)
        {
            var chunk = new List<object>();
            ExecutedItemCount = 0;

            foreach (var listObject in objectList)
            {
                chunk.Add(listObject);

                if (chunk.Count == _chunkSize)
                {
                    ExecutedItemCount += await ExecuteChunk(method, chunk);
                    chunk.Clear();
                    Thread.Sleep(_delayBetweenChunks);
                }
            }

            // Execute the last chunk
            if (chunk.Count > 0) ExecutedItemCount += await ExecuteChunk(method, chunk);

            return ExecutedItemCount;
        }

        public int ExecutedItemCount { get; private set; }

        public IEnumerable FailedObjects => _failedItems;

        private async Task<int> ExecuteChunk(Func<IEnumerable, Task> method, ICollection<object> chunk)
        {
            try
            {
                await method(chunk);
                return chunk.Count;
            }
            catch (Exception)
            {
                // If chunk execution failed, all items are marked as failed because we don't know which items did raise the exception
                _failedItems.AddRange(chunk);
                return 0;
            }
        }
    }
}