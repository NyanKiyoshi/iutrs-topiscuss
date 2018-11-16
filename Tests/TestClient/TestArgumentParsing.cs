using Client;
using Moq;
using Shared;
using NUnit.Framework;

namespace Tests.TestClient {
    /// <summary>
    /// Tests the <see cref="ClientProgram.ParseArgs"/>'s parsing capabilities.
    /// </summary>
    public class TestArgumentParsing {
        /// <summary>
        /// Test <see cref="ClientProgram.ParseArgs"/> against invalid argument count,
        /// which, are expected to trigger a call to
        /// <see cref="ClientProgram.PrintUsage"/> with the error code: <tt>1</tt>.
        /// </summary>
        /// <param name="args">The passed system arguments.</param>
        [TestCase(arg: new string[]{})]
        [TestCase(arg: new[]{"Too", "Many"})]
        public void Test_ParseArgs_WithInvalidArgumentCount(string[] args = null) {
            // Mock the environment
            var environment = new Mock<ControlledEnvironment>();
            ControlledEnvironment.Current = environment.Object;

            try {
                // Try parsing the arguments
                ClientProgram.ParseArgs(args);

                // Verify it failed to parse them, and thus, exited the application with an error
                environment.Verify(calledEnv => calledEnv.Exit(1));
            }
            finally {
                // Remove the mocked environment
                ControlledEnvironment.Reset();
            }
        }

        /// <summary>
        /// Test <see cref="ClientProgram.ParseArgs"/> is handling <tt>--help</tt>.
        /// <see cref="ClientProgram.PrintUsage"/> should thus be called with the error code: <tt>0</tt>.
        /// </summary>
        [TestCase]
        public void Test_ParseArgs_GetHelp() {
            // Mock the environment
            var environment = new Mock<ControlledEnvironment>();
            ControlledEnvironment.Current = environment.Object;

            try {
                // Try parsing the arguments
                ClientProgram.ParseArgs(new []{ClientProgram.HELP_ARGUMENT});

                // Verify the application exited with `0`
                environment.Verify(calledEnv => calledEnv.Exit(0));
            }
            finally {
                // Remove the mocked environment
                ControlledEnvironment.Reset();
            }
        }

        /// <summary>
        /// Test <see cref="ClientProgram.ParseArgs"/> is exiting the application with <tt>1</tt>
        /// if the provided data is invalid.
        /// </summary>
        [TestCase]
        public void Test_ParseArgs_WithInvalidData() {
            // Mock the environment
            var environment = new Mock<ControlledEnvironment>();
            ControlledEnvironment.Current = environment.Object;

            try {
                // Try parsing the arguments
                ClientProgram.ParseArgs(new []{"This is not an IP. How comes?"});

                // Verify it failed to parse them, and thus, exited the application with an error
                environment.Verify(calledEnv => calledEnv.Exit(1));
            }
            finally {
                // Remove the mocked environment
                ControlledEnvironment.Reset();
            }
        }

        /// <summary>
        /// Test <see cref="ClientProgram.ParseArgs"/> is doing nothing if the data is valid.
        /// </summary>
        [TestCase]
        public void Test_ParseArgs_WithValidData() {
            // Try parsing the arguments
            ClientProgram.ParseArgs(new []{"127.0.0.1:5000"});
            Assert.Pass();
        }
    }
}
