namespace ControlUnit.Controller.Core
{
    /// <summary>
    /// Service-Contract for the engine control
    /// </summary>
    public interface IControllerService
    {
        void Test(int value, string x, int value2);

        /// <summary>
        /// Starts the engine
        /// </summary>
        void Start();

        /// <summary>
        /// Shuts the engine down
        /// </summary>
        void Stop();

        /// <summary>
        /// Accelerate from 0 to 100 %
        /// </summary>
        /// <param name="value">0-100%</param>
        void Accelerate(int value);

        /// <summary>
        /// Drives to left
        /// </summary>
        /// <param name="value">0-100%</param>
        void TurnLeft(int value);

        /// <summary>
        /// Drives to right
        /// </summary>
        /// <param name="value">0-100%</param>
        void TurnRight(int value);
    }
}
