using HidSharp.Utility;
using System;
using System.Runtime.InteropServices;
using System.Security.Authentication.ExtendedProtection;
using System.Threading;
using System.Windows.Forms;

namespace DellFanManagement.App
{
    public static class CpuPowerManager
    {
        public const uint Epp = 1;
        public const uint FrequencyMax = 2;

        private static Guid GUID_ACTIVE_SCHEME; //当前使用电源模式
        private static Guid GUID_PROCESSOR_SETTINGS_SUBGROUP = new Guid("54533251-82be-4824-96c1-47b60b740d00"); //CPU性能增强功能相关的 GUID
        public static Guid GUID_PROCESSOR_PERFEPP = new Guid("36687f9e-e3a5-4dbf-b1dc-15eb381c6863"); //EPP（Energy Performance Preference）设置的 GUID
        public static Guid GUID_PROCESSOR_FREQUENCYMAX = new Guid("75b0ae3f-bce0-45a7-8c89-c9611c25e100");// 处理器频率上限设置的 GUID

        // 修改 API 声明，使用 [In] 特性
        [DllImport("powrprof.dll", SetLastError = true)]
        private static extern uint PowerWriteACValueIndex(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            ref Guid SubGroupOfPowerSettingsGuid,
            ref Guid PowerSettingGuid,
            uint AcValueIndex
        );

        [DllImport("powrprof.dll", SetLastError = true)]
        private static extern uint PowerWriteDCValueIndex(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            ref Guid SubGroupOfPowerSettingsGuid,
            ref Guid PowerSettingGuid,
            uint DcValueIndex
        );

        // 2. 读取 AC 值索引
        [DllImport("powrprof.dll")]
        static extern uint PowerReadACValueIndex(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            ref Guid SubGroupOfPowerSettingsGuid,
            ref Guid PowerSettingGuid,
            out uint AcValueIndex);

        [DllImport("powrprof.dll")]
        static extern uint PowerReadDCValueIndex(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            ref Guid SubGroupOfPowerSettingsGuid,
            ref Guid PowerSettingGuid,
            out uint DcValueIndex);



        [DllImport("powrprof.dll", SetLastError = true)]
        private static extern uint PowerGetActiveScheme(
            IntPtr RootPowerKey,
            out IntPtr ActivePolicyGuid
        );

        [DllImport("powrprof.dll", SetLastError = true)]
        private static extern uint PowerSetActiveScheme(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LocalFree(IntPtr hMem);

        static CpuPowerManager()
        {
            GetActivePowerProfile();
        }
        public static Guid? GetActivePowerProfile(bool refresh = false)
        {
            if ((refresh || GUID_ACTIVE_SCHEME == Guid.Empty) && PowerGetActiveScheme(IntPtr.Zero, out IntPtr schemePtr) == 0)
            {
                GUID_ACTIVE_SCHEME = Marshal.PtrToStructure<Guid>(schemePtr);
                Marshal.FreeHGlobal(schemePtr);
            }

            return GUID_ACTIVE_SCHEME;
        }

        public static bool SetGuid(Guid type, int? acValue = null, int? dcValue = null)
        {
            if (acValue.HasValue && (acValue.Value < 0 || acValue.Value > 100))
                return false;
            if (dcValue.HasValue && (dcValue.Value < 0 || dcValue.Value > 100))
                return false;

            uint result;
            bool success = true;

            if (acValue.HasValue)
            {
                result = PowerWriteACValueIndex(
                    IntPtr.Zero,
                    ref GUID_ACTIVE_SCHEME,
                    ref GUID_PROCESSOR_SETTINGS_SUBGROUP,
                    ref type,
                    (uint)acValue.Value
                );
                if (result != 0) success = false;
            }

            if (dcValue.HasValue && success)
            {
                result = PowerWriteDCValueIndex(
                    IntPtr.Zero,
                    ref GUID_ACTIVE_SCHEME,
                    ref GUID_PROCESSOR_SETTINGS_SUBGROUP,
                    ref type,
                    (uint)dcValue.Value
                );
                if (result != 0) success = false;
            }

            if (success)
            {
                result = PowerSetActiveScheme(IntPtr.Zero, ref GUID_ACTIVE_SCHEME);
                if (result != 0) success = false;
            }
            return success;
        }

        public static uint SetGuidByState(Guid type,uint value)
        {
            // 检测当前是否使用电池
            PowerLineStatus powerStatus = SystemInformation.PowerStatus.PowerLineStatus;
            bool onBattery = (powerStatus == PowerLineStatus.Offline);

            uint result;
            if (onBattery)
            {
                // 使用电池，只设置 DC 模式
                result = PowerWriteDCValueIndex(
                    IntPtr.Zero,
                    ref GUID_ACTIVE_SCHEME,
                    ref GUID_PROCESSOR_SETTINGS_SUBGROUP,
                    ref type,
                    value
                );
                Console.WriteLine($"电池模式，{type.ToString()} = {value}");
            } else {
                // 插电使用，只设置 AC 模式
                result = PowerWriteACValueIndex(
                    IntPtr.Zero,
                    ref GUID_ACTIVE_SCHEME,
                    ref GUID_PROCESSOR_SETTINGS_SUBGROUP,
                    ref type,
                    value
                );
                Console.WriteLine($"插电模式，{type.ToString()} = {value}");
            }

            if (result == 0) {
                result = PowerSetActiveScheme(IntPtr.Zero, ref GUID_ACTIVE_SCHEME);
            }
            return result;
        }

        //读取当前电源状态下的 EPP 值
        public static uint GetGuidByState(Guid type,out uint value)
        {
            
            // 检测当前是否使用电池
            PowerLineStatus powerStatus = SystemInformation.PowerStatus.PowerLineStatus;
            bool onBattery = (powerStatus == PowerLineStatus.Offline);

            uint result;
            if (onBattery)
            {
                // 使用电池，只设置 DC 模式
                result = PowerReadDCValueIndex(
                    IntPtr.Zero,
                    ref GUID_ACTIVE_SCHEME,
                    ref GUID_PROCESSOR_SETTINGS_SUBGROUP,
                    ref type,
                    out value
                );
                Console.WriteLine($"电池模式，{type} = {value}");
            } else {
                // 插电使用，只设置 AC 模式
                result = PowerReadACValueIndex(
                    IntPtr.Zero,
                    ref GUID_ACTIVE_SCHEME,
                    ref GUID_PROCESSOR_SETTINGS_SUBGROUP,
                    ref type,
                    out value
                );
                Console.WriteLine($"插电模式，{type} = {value}");
            }

            return result;
        }
    }
}
