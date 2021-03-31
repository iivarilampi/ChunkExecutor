using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ChunkExecutorTests
{
    public class ChunkExecutorTests
    {
        List<int> _actualList;
        
        [Fact]
        public void ListIsSuccessfullyExecutedTest()
        {
            var sut = new ChunkExecutor.ChunkExecutor(3);            
            var inputList = new List<int>()  {1, 2, 3, 4, 5, 6, 7};
            _actualList = new List<int>();
            var expectedList = new List<int>() {2, 4, 6, 8, 10, 12, 14};
            sut.Execute(MultiplyWithTwo, inputList);
            Assert.Equal(expectedList,_actualList);
        }
        private Task MultiplyWithTwo(IEnumerable list) {
            foreach (var number in list)
            {
                _actualList.Add((int)number*2);
            }

            return Task.CompletedTask;
        }
    }
}
