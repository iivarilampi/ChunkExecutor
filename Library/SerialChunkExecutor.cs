using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChunkExecutor
{
    public class SerialChunkExecutor:IChunkExecutor
    {
        private readonly int _chunkSize;

        public SerialChunkExecutor(int chunkSize)
        {
            _chunkSize = chunkSize;
        }
        public async Task<int> Execute(Func<IEnumerable,Task> method,  IEnumerable objectList)
        {
            var chunk = new List<object>();
            int processedItemCount = 0;
            
            foreach (var listObject in objectList)
            {
                chunk.Add(listObject);
                
                if (chunk.Count == _chunkSize)
                {
                    await method(chunk);
                    processedItemCount += chunk.Count;
                    chunk.Clear();
                }
            }
            
            // Execute the last chunk
            if (chunk.Count > 0)
            {
                await method(chunk);
                processedItemCount += chunk.Count;
            }

            return processedItemCount;
        }
    }
}