using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace ChunkExecutorTests
{
    public class SerialChunkExecutorTests
    {
        private readonly List<int> _actualList;
        public SerialChunkExecutorTests()
        {
            _actualList = new List<int>();
            
        }

        [Fact]
        public void ListIsSuccessfullyExecutedTest()
        {
            var sut = new ChunkExecutor.SerialChunkExecutor(3);            
            var inputList = new List<int>()  {1, 2, 3, 4, 5, 6, 7};
            var expectedList = new List<int>() {2, 4, 6, 8, 10, 12, 14};
            var task = sut.Execute(MultiplyWithTwo, inputList);
            Assert.Equal(expectedList.Count,task.Result);
            Assert.Equal(expectedList,_actualList);
            
        }

        [Fact]
        public void ExecutingListFailsTest()
        {
            var inputList = new List<object>() {1, 2, 3, null, 5, 6, 7};
            var sut = new ChunkExecutor.SerialChunkExecutor(3);
            var exception = Record.ExceptionAsync(() => sut.Execute(MultiplyWithTwo, inputList));
            exception.Should().NotBeNull();
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
