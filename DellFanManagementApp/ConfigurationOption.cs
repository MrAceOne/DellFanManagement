namespace DellFanManagement.App
{
    /// <summary>
    /// Just some string constants used when dealing with the ConfigurationStore class.
    /// </summary>
    public class ConfigurationOption
    {
        /// <summary>
        /// Record whether or not the disclaimer message has been shown.
        /// </summary>
        public static readonly ConfigurationOption DisclaimerShown = new(ConfigurationOptionType.Integer, "DisclaimerShown");

        /// <summary>
        /// Path to NVIDIA Inspector or an application that can manipulate the NVIDIA GPU P-state.
        /// </summary>
        public static readonly ConfigurationOption NVPStateApplicationPath = new(ConfigurationOptionType.String, "NVPState");

        /// <summary>
        /// Store the user-selected fan control mode (0 = Automatic, 1 = Manual).
        /// </summary>
        public static readonly ConfigurationOption FanControlMode = new(ConfigurationOptionType.Integer, "FanControlMode");

        /// <summary>
        /// CPU temperature threshold for manual mode fan control (default 45 degrees).
        /// </summary>
        public static readonly ConfigurationOption ManualModeCpuTemperatureThreshold = new(ConfigurationOptionType.Integer, "ManualModeCpuTemperatureThreshold");

        /// <summary>
        /// GPU temperature threshold for manual mode fan control (default 45 degrees).
        /// </summary>
        public static readonly ConfigurationOption ManualModeGpuTemperatureThreshold = new(ConfigurationOptionType.Integer, "ManualModeGpuTemperatureThreshold");

        /// <summary>
        /// Temperature check interval in seconds for manual mode (default 30 seconds).
        /// </summary>
        public static readonly ConfigurationOption ManualModeCheckIntervalSeconds = new(ConfigurationOptionType.Integer, "ManualModeCheckIntervalSeconds");

        /// <summary>
        /// Indicates whether this configuration option is for a "number" or a "string".
        /// </summary>
        public ConfigurationOptionType Type { get; private set; }

        /// <summary>
        /// Key, or basically the name of this configuration option.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">What type of data is going to be stored.</param>
        /// <param name="key">Name of this configuration option.</param>
        private ConfigurationOption(ConfigurationOptionType type, string key)
        {
            Type = type;
            Key = key;
        }
    }
}
