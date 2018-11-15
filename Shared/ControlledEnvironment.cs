using System;

namespace Shared {
    /// <summary>
    /// This is a mocking--able system environment. This prevent <see cref="Exit"/> from exiting during the tests
    /// and to check the call was the expected one.
    /// </summary>
    public abstract class ControlledEnvironment {
        /// <summary>
        /// The current application environment. If not mocked, it's the default <see cref="System.Environment"/>.
        /// Otherwise, the mocked one (the one that was set).
        /// </summary>
        public static ControlledEnvironment Current {
            get => _current;
            set => _current = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// The base signature of <see cref="Environment.Exit"/>.
        /// </summary>
        /// <param name="errorCode">The error code to exit with.</param>
        public abstract void Exit(int errorCode);

        /// <summary>
        /// Put back the environment to the default system environment.
        /// </summary>
        public static void Reset() {
            _current = DefaultControlledEnvironment.Instance;
        }

        /// <summary>
        /// The current application environment.
        /// </summary>
        private static ControlledEnvironment _current =
            DefaultControlledEnvironment.Instance;
    }

    /// <inheritdoc />
    /// <summary>
    /// The default application environment.
    /// </summary>
    internal class DefaultControlledEnvironment : ControlledEnvironment {
        /// <inheritdoc />
        /// <summary>
        /// The real runtime exit function.
        /// </summary>
        /// <param name="errorCode"></param>
        public override void Exit(int errorCode) {
            Environment.Exit(errorCode);
        }

        /// <summary>
        /// Set the instance type to a lazy system environment.
        /// </summary>
        public static DefaultControlledEnvironment Instance => INSTANCE.Value;

        /// <summary>
        /// The environment getter.
        /// </summary>
        private static readonly Lazy<DefaultControlledEnvironment> INSTANCE =
            new Lazy<DefaultControlledEnvironment>(() => new DefaultControlledEnvironment());
    }
}
