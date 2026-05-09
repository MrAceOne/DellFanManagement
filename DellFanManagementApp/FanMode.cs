namespace DellFanManagement.App
{
    /// <summary>
    /// Represents the user-selected fan control mode on the UI.
    /// This is decoupled from the actual EC fan control state.
    /// </summary>
    public enum FanMode
    {
        /// <summary>
        /// Automatic mode - EC controls fan speed.
        /// </summary>
        Automatic = 0,

        /// <summary>
        /// Manual mode - user controls fan speed.
        /// </summary>
        Manual = 1
    }
}
