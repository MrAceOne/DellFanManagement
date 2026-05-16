using System.Collections.Generic;

namespace DellFanManagement.App.FanSpeedReaders
{
    /// <summary>
    /// Determine system capabilities and select an appropriate fan speed reader object to use.
    /// </summary>
    public static class FanSpeedReaderFactory
    {
        /// <summary>
        /// Selects a fan speed reader and returns it.
        /// WMI reader is deferred until Bzh and Smi both fail, to avoid slow WMI queries on startup.
        /// </summary>
        /// <returns>Fan speed reader appropriate for the system; null if one cannot be found.</returns>
        public static IFanSpeedReader GetFanSpeedReader()
        {
            // Fast readers first — Bzh and Smi have no expensive constructors.
            List<IFanSpeedReader> fastReaders = new()
            {
                new BzhFanSpeedReader(),
                new SmiFanSpeedReader()
            };

            IFanSpeedReader selectedReader = new NullFanSpeedReader();
            int bestFanCount = 0;

            foreach (IFanSpeedReader reader in fastReaders)
            {
                FanSpeeds fanSpeedReading = reader.GetFanSpeeds();
                int fanCount = 0;

                if (fanSpeedReading.Fan1Rpm != null)
                {
                    fanCount++;
                }
                if (fanSpeedReading.Fan2Rpm != null)
                {
                    fanCount++;
                }

                if (fanCount > bestFanCount)
                {
                    bestFanCount = fanCount;
                    selectedReader = reader;
                }
            }

            // Only probe WMI if fast readers found nothing — WMI constructor queries root/dcim/sysman
            // and can block for several seconds.
            if (bestFanCount == 0)
            {
                IFanSpeedReader wmiReader = new WmiFanSpeedReader();
                FanSpeeds wmiReading = wmiReader.GetFanSpeeds();
                int wmiFanCount = 0;

                if (wmiReading.Fan1Rpm != null)
                {
                    wmiFanCount++;
                }
                if (wmiReading.Fan2Rpm != null)
                {
                    wmiFanCount++;
                }

                if (wmiFanCount > bestFanCount)
                {
                    bestFanCount = wmiFanCount;
                    selectedReader = wmiReader;
                }
            }

            Log.Write(string.Format("Selected fan speed reader: {0}", selectedReader?.GetType()));
            return selectedReader;
        }
    }
}
