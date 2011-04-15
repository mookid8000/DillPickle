using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DillPickle.Framework.Listeners;
using DillPickle.Framework.Types;
using NUnit.Framework;
using Rhino.Mocks;

namespace DillPickle.Tests.Runner
{
    [TestFixture]
    public class TestConsoleWritingEventListener : FixtureBase
    {
        [Test]
        public void TestTimestampFormatShowTimestamp()
        {
            // Arrange
            var consoleWriter =  new ConsoleWritingEventListener();
            var currentTime = new DateTime(2011, 04, 14, 23, 59, 59);

            Time.SetTime(currentTime);

            // Act
            consoleWriter.ShowTimestamps = true;
            string timeStamp = consoleWriter.PossiblyTimestamp();

            // Assert
            Assert.AreEqual(" [23:59:59]", timeStamp);
        }

        [Test]
        public void TestTimestampFormatDoNotShowTimestamp()
        {
            // Arrange
            var consoleWriter = new ConsoleWritingEventListener();
            var currentTime = new DateTime(2011, 04, 14, 23, 59, 59);
            Time.SetTime(currentTime);

            // Act
            string timeStamp = consoleWriter.PossiblyTimestamp();

            // Assert
            Assert.AreEqual(string.Empty, timeStamp);
        }
    }
}
