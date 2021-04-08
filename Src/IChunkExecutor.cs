using System;
using System.Collections;
using System.Threading.Tasks;

namespace ChunkExecutor
{
    internal interface IChunkExecutor
    {
        Task<int> Execute(Func<IEnumerable, Task> method, IEnumerable objectList);
    }
}