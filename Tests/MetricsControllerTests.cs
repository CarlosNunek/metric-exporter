using Xunit;

namespace metric_exporter.Tests
{
    public class DummyPassingTest
    {
        [Fact]
        public void Test_AlwaysPasses()
        {
            Assert.True(true);
        }
    }
}

