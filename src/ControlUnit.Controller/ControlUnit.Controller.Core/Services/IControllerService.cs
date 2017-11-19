namespace ControlUnit.Controller.Core
{
    /// <summary>
    /// Service-Contract for the engine control
    /// </summary>
    public interface IControllerService
    {
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
        void Accelerate(double value);

        /// <summary>
        /// Controls direct speed of right track
        /// </summary>
        /// <param name="value"></param>
        void TurnRight(double value);

        /// <summary>
        /// Controls direct speed of left track
        /// </summary>
        /// <param name="value"></param>
        void TurnLeft(double value);
    }
}
