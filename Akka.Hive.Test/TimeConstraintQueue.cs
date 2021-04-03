using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Akka.Hive.Test
{
    // <Start, Duration>
    public class TimeConstraintQueue : SortedList<long, long>
    {
        private readonly ITestOutputHelper _output;
        // Conflicting Time Sets to Test.
        //  
        // Existing Time Set          |------------------|
        // Overlapping Start       |----------|
        // Overlapping End                         |---------|
        // Subset                         |-----------|
        // Superset              |-----------------------------|

        public TimeConstraintQueue(ITestOutputHelper output)
        {
            _output = output;
            // T0 1 2 3 4 5 6 7 8 9 10 11 12 13 
            // I0 1 1 - - - 2 2 - 3  4  4  -  -   
            this.Add(1, 2);
            this.Add(6, 2);
            this.Add(9, 1);
            this.Add(10, 2);
        }

        [Fact]
        public void QueueIsValid()
        {
            var nextItem = this.GetEnumerator();     
            var (key, value) = nextItem.Current;
            
            while (nextItem.MoveNext())
            {
                var end = key + value;
                var startNext = nextItem.Current;

                Assert.True(end <= startNext.Key);   
                (key, value) = nextItem.Current;
            }
            nextItem.Dispose();
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(2, 1)]
        [InlineData(4, 1)]
        public void FindFirstValidSlot(long tDuration, int tExpectedLength)
        {
            var enumerator = this.GetEnumerator();     
            var current = enumerator.Current;
            var validSlots = new  SortedList<long, long>();
            var slotFound = false;
            while (enumerator.MoveNext())
            {
                var endPre = current.Key + current.Value;
                var startPost = enumerator.Current.Key;

                if(endPre <= startPost - tDuration) {
                    slotFound = validSlots.TryAdd(endPre, startPost - endPre);
                    break;
                }
                current = enumerator.Current;
            }

            if (!slotFound)
                validSlots.Add(current.Key + current.Value, long.MaxValue);
            
            enumerator.Dispose();

            Assert.Equal(tExpectedLength, validSlots.Count );

            validSlots.ToList().ForEach(x => _output.WriteLine( $"start: {x.Key}, slotSize {x.Value} "));
        }
    }
}
