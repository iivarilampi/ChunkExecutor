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

        public static TheoryData<List<object>, int> ListsWithFailingItems
        {
            get
            {
                var data = new TheoryData<List<object>, int>();
                var listSizes = Enumerable.Range(10, 50).ToList();
                foreach (var listSize in listSizes)
                {
                    var failingItemIndexes = new[] {0, 2, 3, listSize / 2, listSize - 1};
                    var inputList = Enumerable.Range(1, listSize).ToList().ConvertAll(i => (object) i);
                    foreach (var index in failingItemIndexes) inputList[index] = null;
                    for (var chunkSize = 1; chunkSize <= listSize / 2; ++chunkSize) data.Add(inputList, chunkSize);
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

            executedItemCount.Should().Be(expectedList.Count);
            sut.ExecutedItemCount.Should().Be(expectedList.Count);
            _actualList.Should().Equal(expectedList);
            var numberOfDelays = listSize / chunkSize - 1 + Math.Min(listSize % chunkSize, 1);
            stopWatch.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(numberOfDelays * delayInMilliseconds);
        }

        [Theory]
        [MemberData(nameof(ListsWithFailingItems), MemberType = typeof(SerialChunkExecutorTests))]
        public void ListHasFailingItemsTest(List<object> inputList, int chunkSize)
        {
            var expectedList = new List<int>();
            for (var listIndex = 0; listIndex < inputList.Count; listIndex += chunkSize)
            {
                var actualChunkSize = Math.Min(chunkSize, inputList.Count() - listIndex);
                var chunk = inputList.GetRange(listIndex, actualChunkSize).ToList();
                expectedList.AddRange(chunk.TakeWhile(item => item is { }).Select(item => (int) item * 2));
            }

            var sut = new SerialChunkExecutor(chunkSize);
            var exception = Record.ExceptionAsync(() => sut.Execute(MultiplyWithTwo, inputList));
            exception.Should().NotBeNull();

            sut.ExecutedItemCount.Should().BeLessOrEqualTo(expectedList.Count);
            sut.FailedObjects.Should().HaveCountGreaterOrEqualTo(inputList.Count(i => i is null));
            _actualList.Should().Equal(expectedList);
        }
    }
}