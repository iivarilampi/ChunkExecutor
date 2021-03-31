using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChunkExecutor
{
    public class ChunkExecutor:IChunkExecutor
    {
        private readonly int _chunkSize;

        public ChunkExecutor(int chunkSize)
        {
            _chunkSize = chunkSize;
        }
        public async Task Execute(Func<IEnumerable,Task> method,  IEnumerable objectList)
        {
            var chunk = new List<object>();
            foreach (var listObject in objectList)
            {
                chunk.Add(listObject);
                
                if (chunk.Count == _chunkSize)
                {
                    await method(chunk);
                    chunk.Clear();
                }
            }
            
            // Execute the last chunk
            if (chunk.Count > 0)
            {
                await method(chunk);
            }
        }
    }
}