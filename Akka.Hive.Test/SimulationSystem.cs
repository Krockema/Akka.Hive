using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Hive.Definitions;
using Akka.Hive.Test.Moc;
using Xunit;
using Akka.TestKit.Xunit2;

namespace Akka.Hive.Test
{
    public class SimulationSystem : TestKit.Xunit2.TestKit
    {
        public static IEnumerable<object[]> GetTestData()
        {
            yield return new object[]{ SimulationCreator(debugMode: false, 0) };
            yield return new object[]{ SimulationCreator(debugMode: true, 0) };
        }

        public static IEnumerable<object[]> GetDelayTestData()
        {
            yield return new object[] { SimulationCreator(debugMode: false, 10) };
        }


        public static Hive SimulationCreator(bool debugMode, long timeToAdvance)
        {
            var simConfig = HiveConfig.ConfigureSimulation(false)
                                      .WithTimeSpanToTerminate(TimeSpan.FromDays(1))                
                                      .WithDebugging(akka: false, hive: true)
                                      .WithInterruptInterval(TimeSpan.FromMinutes(120))
                                      .WithTickSpeed(TimeSpan.FromMilliseconds(timeToAdvance))
                                      .Build();
            var sim = new Hive(simConfig);
            // ActorMonitoringExtension.Monitors(sim.ActorSystem).IncrementActorCreated();
            // ActorMonitoringExtension.Monitors(sim.ActorSystem).IncrementMessagesReceived();
            return sim;
        }


        [Theory]
        [MemberData(nameof(GetTestData))]
        public void IsStarting(Hive engine)
        {
            Assert.True(engine.IsReady());
            
            var task = engine.RunAsync();
            Within(TimeSpan.FromSeconds(3), async () =>
            {
                await task;
                Assert.False(task.IsCompletedSuccessfully);
            });
        }

        [Theory]
        [MemberData(nameof(GetTestData))]
        public void MessageCurrentTime(Hive engine)
        {
            var ping = engine.ActorSystem.ActorOf<PingBackAndForward>();
            var target = this.CreateTestProbe();

            var msg = new Ping(message: Ping.Name
                              , target: target);

            Within(TimeSpan.FromSeconds(3), () =>
            {
                engine.ContextManagerRef.Tell(msg);

                target.FishForMessage(o => o.GetType() == typeof(Ping));

                Assert.Equal(TestActor, target.Sender);

            });
        }

        [Theory]
        [MemberData(nameof(GetTestData))]
        public void MessageScheduled(Hive hive)
        {
            var source = this.CreateTestProbe();
            var worker = hive.ActorSystem.ActorOf(Props.Create(() 
                            => new ActingObject(hive.ContextManagerRef, Time.Now, hive.Config)));

            var msg = new Work(message: 10
                              , target: worker);

            Within(TimeSpan.FromSeconds(3), async () =>
            {
                hive.ContextManagerRef.Tell(msg, source);
                await hive.RunAsync();

                var work = source.FishForMessage(o => o.GetType() == typeof(Work)) as Work;
                
                Assert.Equal("Done", work.Message.ToString());
                Assert.Equal(TestActor, source.Sender);
            });
        }


        [Fact]
        public void IsStoppingAfterAtInterval()
        {
            Assert.True(true);
        }

        [Fact]
        public void IsRecurringAfterStop()
        {
            Assert.True(true);
        }

        [Fact]
        public void IsShutdown()
        {
            Assert.True(true);
        }
    }
}