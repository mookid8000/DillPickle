using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DillPickle.Framework.Listeners;
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
            var consoleWriter = Mock<ConsoleWritingEventListener>();
            var currentTime = new DateTime(2011, 04, 14, 23, 59, 59);

            consoleWriter.Stub(cwel => cwel.ShowTimestamps).Return(true);
            consoleWriter.Stub(cwel => cwel.CurrentTime).Return(currentTime);

            // Act
            string timeStamp = consoleWriter.PossiblyTimestamp();

            // Assert
            Assert.AreEqual(" [23:59:59]", timeStamp);
        }

        [Test]
        public void TestTimestampFormatDoNotShowTimestamp()
        {
            // Arrange
            var consoleWriter = Mock<ConsoleWritingEventListener>();
            var currentTime = new DateTime(2011, 04, 14, 23, 59, 59);

            consoleWriter.Stub(cwel => cwel.ShowTimestamps).Return(false);
            consoleWriter.Stub(cwel => cwel.CurrentTime).Return(currentTime);

            // Act
            string timeStamp = consoleWriter.PossiblyTimestamp();

            // Assert
            Assert.AreEqual("", timeStamp);
        }
    }
}
