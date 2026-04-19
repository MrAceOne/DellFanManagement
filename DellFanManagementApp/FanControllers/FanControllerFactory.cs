using DellFanManagement.DellSmbiosSmiLib;

namespace DellFanManagement.App.FanControllers
{
    /// <summary>
    /// Determine system capabilities and select an appropriate fan speed controller to use.
    /// </summary>
    class FanControllerFactory
    {
        /// <summary>
        /// Selects a fan speed controller and returns it.
        /// </summary>
        /// <returns>Fan speed controller appropriate for the system.</returns>
        public static FanController GetFanFanController()
        {
            // 优先使用 BZH 控制器（支持单独风扇控制）
            BzhFanController bzhController = new BzhFanController();
            if (bzhController.IsAutomaticFanControlDisableSupported)
            {
                Log.Write("Using BZH fan control (supports individual fan control).");
                return bzhController;
            }
            else
            {
                // 如果 BZH 不可用，回退到 SMI（不支持单独风扇控制）
                if (DellSmbiosSmi.IsFanControlOverrideAvailable())
                {
                    Log.Write("Using SMI fan control (does not support individual fan control).");
                    return new SmiFanController();
                }
                else
                {
                    Log.Write("No fan control available.");
                    return new NullFanController();
                }
            }
        }
    }
}
