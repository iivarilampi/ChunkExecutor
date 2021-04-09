using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ChunkExecutor;
using FluentAssertions;
using Xunit;

namespace ChunkExecutorTests
{
    public class SerialChunkExecutorTests
    {
        private readonly List<int> _actualList;

        public SerialChunkExecutorTests()
        {
            _actualList = new List<int>();
        }

        [Theory]
        [InlineData(7,3,0)]
        [InlineData(70,3,30)]
        [InlineData(1,1,300)]
        [InlineData(10,10,3)]
        [InlineData(10,5,3)]
        public void ListIsSuccessfullyExecutedTest(int listSize, int chunkSize, int delayInMilliseconds)
        {
            var sut = new SerialChunkExecutor(chunkSize,TimeSpan.FromMilliseconds(delayInMilliseconds));
            var inputList = Enumerable.Range(1,listSize).ToList();
            var expectedList = new List<int>();
            inputList.ForEach(i => expectedList.Add(i * 2));
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var executedItemCount = sut.Execute(MultiplyWithTwo, inputList).Result;
            stopWatch.Stop();
            Assert.Equal(expectedList.Count, executedItemCount);
            Assert.Equal(expectedList.Count, sut.ExecutedItemCount);
            Assert.Equal(expectedList, _actualList);
            var numberOfDelays = (listSize / chunkSize) - 1 + Math.Min(listSize % chunkSize,1);
            Assert.True(stopWatch.ElapsedMilliseconds >= numberOfDelays*delayInMilliseconds);
        }

        [Fact]
        public void ExecutingListFailsTest()
        {
            var inputList = new List<object> {1, 2, 3, null, 5, 6, 7};
            var sut = new SerialChunkExecutor(3);
            var exception = Record.ExceptionAsync(() => sut.Execute(MultiplyWithTwo, inputList));
            exception.Should().NotBeNull();
            Assert.Equal(3,sut.ExecutedItemCount);
        }

        private Task MultiplyWithTwo(IEnumerable list)
        {
            foreach (var number in list) _actualList.Add((int) number * 2);

            return Task.CompletedTask;
        }
    }
}