using System;
using DillPickle.Framework.Listeners;
using DillPickle.Framework.Types;
using NUnit.Framework;

namespace DillPickle.Tests.Runner
{
    [TestFixture]
    public class TestConsoleWritingEventListener : FixtureBase
    {
        [Test]
        public void TestTimestampFormatShowTimestampFromLocalTime()
        {
            // Arrange
            var consoleWriter =  new ConsoleWritingEventListener();
            var currentTime = new DateTime(2011, 04, 14, 23, 59, 59, DateTimeKind.Local);

            Time.SetTime(currentTime);

            // Act
            consoleWriter.ShowCurrentTimes = true;
            string timeStamp = consoleWriter.PossiblyTimes();

            Time.Reset();

            // Assert
            Assert.AreEqual(" [23:59:59]", timeStamp);
        }

        [Test]
        public void TestTimestampFormatShowTimestampFromUTCTime()
        {
            // Arrange
            var consoleWriter = new ConsoleWritingEventListener();
            var currentTime = new DateTime(2011, 04, 14, 23, 59, 59, DateTimeKind.Utc);

            Time.SetTime(currentTime);

            // Act
            consoleWriter.ShowCurrentTimes = true;
            string timeStamp = consoleWriter.PossiblyTimes();

            Time.Reset();

            // Assert
            Assert.AreEqual(" [01:59:59]", timeStamp);
        }

        [Test]
        public void TestTimestampFormatDoNotShowTimestamp()
        {
            // Arrange
            var consoleWriter = new ConsoleWritingEventListener();
            var currentTime = new DateTime(2011, 04, 14, 23, 59, 59);
            Time.SetTime(currentTime);

            // Act
            string timeStamp = consoleWriter.PossiblyTimes();

            Time.Reset();

            // Assert
            Assert.AreEqual(string.Empty, timeStamp);
        }
    }
}
