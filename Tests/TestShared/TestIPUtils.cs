using NUnit.Framework;
using Shared;

namespace Tests.TestShared {
    /// <summary>
    /// Test the <code>host[:port]</code> parsing utility of the internal IP utilities.
    /// </summary>
    public class TestIPUtils {
        /// <summary>
        /// Test <see cref="IPUtils.TryParseEndpoint"/> against valid data.
        /// </summary>
        ///
        /// <param name="inputString">The string to parse.</param>
        /// <param name="expectedIP">The expected IP to be found</param>
        /// <param name="expectedPort">The expected port to be found</param>
        [TestCase("62.0.0.2", "62.0.0.2", DefaultConfig.DEFAULT_SERVER_PORT)]
        [TestCase("127.0.0.1:5000", "127.0.0.1", 5000)]
        public void Test_TryParseEndpoint_withValidData(
            string inputString, string expectedIP, short expectedPort) {

            // Parse the input string
            var isParseSuccess =
                IPUtils.TryParseEndpoint(inputString, out var parseResult);

            // It should have been a success
            Assert.IsTrue(isParseSuccess);

            // We should have a result set
            Assert.NotNull(parseResult);

            // Verify the parsed data is was we expected
            Assert.AreEqual(parseResult.Port, expectedPort);
            Assert.AreEqual(parseResult.Address.ToString(), expectedIP);
        }

        /// <summary>
        /// Test <see cref="IPUtils.TryParseEndpoint"/> against invalid data:
        /// <list type="bullet">
        ///     <item>Empty data;</item>
        ///     <item>Invalid IP;</item>
        ///     <item>Invalid Port;</item>
        ///     <item>Invalid IP and Port.</item>
        /// </list>
        /// </summary>
        /// <param name="inputString"></param>
        [TestCase("")]
        [TestCase("62.0.0.299")]
        [TestCase("212.0.0.0:50:")]
        [TestCase("127.0.0.1:500000")]
        [TestCase("287.0.0.1:500000")]
        public void Test_TryParseEndpoint_withInvalidData(string inputString) {
            // Parse the input string
            var isParseSuccess =
                IPUtils.TryParseEndpoint(inputString, out var parseResult);

            // It should be a failure
            Assert.IsFalse(isParseSuccess);

            // We should not have a result set
            Assert.IsNull(parseResult);
        }
    }
}
