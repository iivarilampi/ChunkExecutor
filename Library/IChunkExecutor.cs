using System;
using System.Collections;
using System.Threading.Tasks;

namespace ChunkExecutor
{
    interface IChunkExecutor
    {
        Task Execute(Func<IEnumerable,Task> method,  IEnumerable objectList);
    }
}