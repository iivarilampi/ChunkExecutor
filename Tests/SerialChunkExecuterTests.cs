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

        public static TheoryData<List<object>, List<object>, int> ListsWithFailingItems
        {
            get
            {
                var data = new TheoryData<List<object>, List<object>, int>();

                var listSize = 20;
                var failingItemIndexes = new[] {0, 2, 3, listSize / 2, listSize - 1};
                var inputList = Enumerable.Range(1, listSize).ToList().ConvertAll(i => (object) i);
                foreach (var index in failingItemIndexes) inputList[index] = null;
                for (var chunkSize = 1; chunkSize <= 10; chunkSize += 2)
                {
                    var expectedList = new List<object>();
                    inputList.Where(i => i != null).ToList().ForEach(i => expectedList.Add((int) i * 2));
                    data.Add(inputList, expectedList, chunkSize);
                }
                return data;
            }
        }


        private Task MultiplyWithTwo(IEnumerable list)
        {
            foreach (var number in list) _actualList.Add((int) number * 2);

            return Task.CompletedTask;
        }

        [Theory]
        [InlineData(7, 3, 0)]
        [InlineData(70, 3, 30)]
        [InlineData(1, 1, 300)]
        [InlineData(10, 10, 3)]
        [InlineData(10, 5, 3)]
        public void ListIsSuccessfullyExecutedTest(int listSize, int chunkSize, int delayInMilliseconds)
        {
            var sut = new SerialChunkExecutor(chunkSize, TimeSpan.FromMilliseconds(delayInMilliseconds));
            var inputList = Enumerable.Range(1, listSize).ToList();
            var expectedList = new List<int>();
            inputList.ForEach(i => expectedList.Add(i * 2));
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var executedItemCount = sut.Execute(MultiplyWithTwo, inputList).Result;
            stopWatch.Stop();
            Assert.Equal(expectedList.Count, executedItemCount);
            Assert.Equal(expectedList.Count, sut.ExecutedItemCount);
            Assert.Equal(expectedList, _actualList);
            var numberOfDelays = listSize / chunkSize - 1 + Math.Min(listSize % chunkSize, 1);
            Assert.True(stopWatch.ElapsedMilliseconds >= numberOfDelays * delayInMilliseconds);
        }

        [Theory]
        [MemberData(nameof(ListsWithFailingItems), MemberType = typeof(SerialChunkExecutorTests))]
        public void ListHasFailingItemsTest(List<object> inputList, List<object> expectedList, int chunkSize)
        {
            var sut = new SerialChunkExecutor(chunkSize);
            var exception = Record.ExceptionAsync(() => sut.Execute(MultiplyWithTwo, inputList));
            exception.Should().NotBeNull();
            Assert.Equal(expectedList.Count, sut.ExecutedItemCount);
            Assert.Equal(expectedList, _actualList.ConvertAll(i => (object) i));
        }
    }
}