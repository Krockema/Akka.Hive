using System;

namespace Akka.Hive.Definitions
{
    public class Time
    {
        
        public Time(DateTime dt)
        {
            Value = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, 0, dt.Kind);
        }
        public DateTime Value { get; }
        public DateTime? NullableValue => Value;
        public long AsLongFromZero => Value.ToFileTimeUtc();
        public long AsLong(Time from)
        {
            TimeSpan t = Value - from.Value;
            return (long)t.TotalMilliseconds / 1000;
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