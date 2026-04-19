using DellFanManagement.DellSmbiosSmiLib;

namespace DellFanManagement.App.FanControllers
{
    /// <summary>
    /// Allows fan speed control using the WMI/SMI interface.
    /// </summary>
    class SmiFanController : FanController
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public SmiFanController()
        {
            IsAutomaticFanControlDisableSupported = true;
            IsSpecificFanControlSupported = true;
            IsIndividualFanControlSupported = false;
        }

        /// <summary>
        /// Disable automatic fan control.
        /// </summary>
        /// <returns>True on success, false on failure.</returns>
        public override bool DisableAutomaticFanControl()
        {
            Log.Write("[SMI] Disabling automatic fan control");
            bool result = DellSmbiosSmi.DisableAutomaticFanControl();
            Log.Write(string.Format("[SMI] Disable automatic fan control result: {0}", result));
            return result;
        }

        /// <summary>
        /// Enable automatic fan control.
        /// </summary>
        /// <returns>True on success, false on failure.</returns>
        public override bool EnableAutomaticFanControl()
        {
            Log.Write("[SMI] Enabling automatic fan control");
            bool result = DellSmbiosSmi.EnableAutomaticFanControl();
            Log.Write(string.Format("[SMI] Enable automatic fan control result: {0}", result));
            return result;
        }

        /// <summary>
        /// Set the fan speed.
        /// </summary>
        /// <param name="level">Speed level to set.</param>
        /// <param name="fanIndex">Which fan to set.</param>
        /// <returns>True on succes, false on failure.</returns>
        public override bool SetFanLevel(FanLevel level, FanIndex fanIndex)
        {
            if (fanIndex != FanIndex.AllFans)
            {
                // Can't control fans individually via SMI.
                return false;
            }

            SmiFanLevel smiLevel;
            switch (level)
            {
                case FanLevel.Off:
                    smiLevel = SmiFanLevel.Off;
                    break;
                case FanLevel.Medium:
                    smiLevel = SmiFanLevel.Low;
                    break;
                case FanLevel.High:
                    smiLevel = SmiFanLevel.High;
                    break;
                default:
                    return false;
            }

            Log.Write(string.Format("[SMI] Setting fan level: {0}", smiLevel));
            bool result = DellSmbiosSmi.SetFanLevel(smiLevel);
            Log.Write(string.Format("[SMI] Set fan level result: {0}", result));
            return result;
        }

        /// <summary>
        /// No shutdown method is needed for this fan controller.
        /// </summary>
        public override void Shutdown()
        {
            // Take no action.
        }
    }
}
