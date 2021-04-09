using System;
using System.Collections;
using System.Threading.Tasks;

namespace ChunkExecutor
{   
    interface IChunkExecutor
    {
        Task<int> Execute(Func<IEnumerable, Task> method, IEnumerable objectList);
        int ExecutedItemCount { get; }
        
        IEnumerable FailedObjects { get; }
    }
}