using System;

namespace Akka.Hive.Definitions
{
    /// <summary>
    /// Time wrapper that cuts milliseconds on creation, to enable async time comparison.
    /// This ensures a true even if the time is some milliseconds of, due to the async actor calls.
    /// </summary>
    public class Time
    {
        public Time(DateTime dt)
        {
            Value = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, 0, dt.Kind);
        }

        public static Time ZERO =>  new Time(new DateTime(637134336000000000)); // 01.01.2020

        public DateTime Value { get; }
        public DateTime? NullableValue => Value;
        public long AsLongFromZero => Value.Ticks;
        public long AsLong(Time from)
        {
            TimeSpan t = Value - from.Value;
            return t.Ticks;
        }

        public Time(long x, Time from)
        {
            var dt = from.Value;
            var time = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, 0, dt.Kind);
            Value = time.AddSeconds(value: x);
        }

        public static Time Now => new (DateTime.Now);


        public Time Add(TimeSpan timeSpan)
        {
            return new (Value.Add(timeSpan));
        }
    }
}