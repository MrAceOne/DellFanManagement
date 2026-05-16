using System.Collections.Generic;

namespace DellFanManagement.App.FanSpeedReaders
{
    /// <summary>
    /// Determine system capabilities and select an appropriate fan speed reader object to use.
    /// </summary>
    public static class FanSpeedReaderFactory
    {
        /// <summary>
        /// Always use BzhFanSpeedReader for reading fan speeds.
        /// </summary>
        /// <returns>BzhFanSpeedReader instance.</returns>
        public static IFanSpeedReader GetFanSpeedReader()
        {
            IFanSpeedReader selectedReader = new BzhFanSpeedReader();
            Log.Write(string.Format("Selected fan speed reader: {0}", selectedReader?.GetType()));
            return selectedReader;
        }
    }
}
