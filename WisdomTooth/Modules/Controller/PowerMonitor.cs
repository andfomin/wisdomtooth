using System;
using System.Runtime.InteropServices;

namespace MediaCurator.Controller
{
    public class PowerMonitor : IDisposable
    {
        public const int WM_POWERBROADCAST = 0x0218;

        [DllImport(@"User32", EntryPoint = "RegisterPowerSettingNotification", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid, Int32 Flags);

        [DllImport(@"User32", EntryPoint = "UnregisterPowerSettingNotification", CallingConvention = CallingConvention.StdCall)]
        private static extern bool UnregisterPowerSettingNotification(IntPtr handle);

        /* Power setting GUIDs are defined in WinNT.h in the Vista Platform SDK */
        private Guid GUID_ACDC_POWER_SOURCE = new Guid("5D3E9A59-E9D5-4B00-A6BD-FF34FF516548");
        private Guid GUID_BATTERY_PERCENTAGE_REMAINING = new Guid("A7AD8041-B45A-4CAE-87A3-EECBB468A9E1");
        private Guid GUID_IDLE_BACKGROUND_TASK = new Guid("515C31D8-F734-163D-A0FD-11A08C91E8F1");
        private Guid GUID_POWERSCHEME_PERSONALITY = new Guid("245D8541-3943-4422-B025-13A784F679B7");

        private const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;

        private const int PBT_APMSUSPEND = 0x0004;  /* Immediately before suspending */
        private const int PBT_APMRESUMEAUTOMATIC = 0x0012;  /* The system is resuming from sleep or hibernation, does not indicate whether a user is present */
        private const int PBT_APMRESUMESUSPEND = 0x0007;  /* If the system detects any user activity after broadcasting PBT_APMRESUMEAUTOMATIC, it will broadcast a PBT_APMRESUMESUSPEND */
        private const int PBT_APMPOWERSTATUSCHANGE = 0x000A;  /* Indicates a system power status change */
        private const int PBT_POWERSETTINGCHANGE = 0x8013;  /* Power setting change */

        /* This structure is sent when the PBT_POWERSETTINGSCHANGE message is sent. */
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct POWERBROADCAST_SETTING
        {
            public Guid PowerSetting;
            public int DataLength;
            ////public byte Data;
        }

        /* All power schemes map to one of these personalities */
        private static Guid GUID_MAX_POWER_SAVINGS = new Guid("A1841308-3541-4FAB-BC81-F71556F20B4A");  /* Power Saver */
        private static Guid GUID_MIN_POWER_SAVINGS = new Guid("8C5E7FDA-E8BF-4A96-9A85-A6E23A8C635C");  /* High Performance */
        private static Guid GUID_TYPICAL_POWER_SAVINGS = new Guid("381B4222-F694-41F0-9685-FF5BB260DF2E");  /* Automatic */

        public enum PowerPersonality
        {
            Unknown,
            HighPerformance,
            PowerSaver,
            Automatic
        }

        /// <summary>
        /// Gets the Guid relating to the currently active power scheme.
        /// </summary>
        /// <param name="rootPowerKey">Reserved for future use, this must be set to IntPtr.Zero</param>
        /// <param name="activePolicy">Returns a Guid referring to the currently active power scheme.</param>
        [DllImport("powrprof.dll")]
        private static extern void PowerGetActiveScheme(
            IntPtr UserRootPowerKey,
            [MarshalAs(UnmanagedType.LPStruct)]
            out Guid ActivePolicyGuid);

        [DllImport("Kernel32.dll", EntryPoint = "LocalFree")]
        private static extern IntPtr LocalFree(ref Guid guid);

        private enum SYSTEM_POWER_CONDITION
        {
            PoAc = 0,  /* External power source */
            PoDc = 1,  /* Power from built-in batteries */
            PoHot = 2,  /* Short-term power source such as a UPS device */
            PoConditionMaximum = 3  /* Out of range value */
        };

        private IntPtr hPowerSource;
        private IntPtr hBatteryRemaining;
        private IntPtr hBackgroundTask;
        private IntPtr hPowerScheme;

        private bool isDisposed;

        ~PowerMonitor()
        {
            Dispose(false);
        }

        public void RegisterForNotifications(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
            {
                throw new ArgumentNullException("4215650");
            }

            UnregisterForNotifications();

            hPowerSource = RegisterPowerSettingNotification(handle, ref GUID_ACDC_POWER_SOURCE, DEVICE_NOTIFY_WINDOW_HANDLE);
            hBatteryRemaining = RegisterPowerSettingNotification(handle, ref GUID_BATTERY_PERCENTAGE_REMAINING, DEVICE_NOTIFY_WINDOW_HANDLE);
            hBackgroundTask = RegisterPowerSettingNotification(handle, ref GUID_IDLE_BACKGROUND_TASK, DEVICE_NOTIFY_WINDOW_HANDLE);
            hPowerScheme = RegisterPowerSettingNotification(handle, ref GUID_POWERSCHEME_PERSONALITY, DEVICE_NOTIFY_WINDOW_HANDLE);
        }

        public void PowerBroadcastMessageHandler(int Msg, IntPtr WParam, IntPtr LParam)
        {
            if (Msg == WM_POWERBROADCAST)
            {
                switch ((int)WParam)
                {
                    case PBT_APMSUSPEND:
                        break;
                    case PBT_APMRESUMEAUTOMATIC:
                        break;
                    case PBT_APMPOWERSTATUSCHANGE:
                        break;
                    case PBT_POWERSETTINGCHANGE:
                        PowerSettingChange(LParam);
                        break;
                    default:
                        break;
                }
            }
        }

        private void UnregisterForNotifications()
        {
            if (hPowerSource != IntPtr.Zero)
            {
                UnregisterPowerSettingNotification(hPowerSource);
                hPowerSource = IntPtr.Zero;
            }

            if (hBatteryRemaining != IntPtr.Zero)
            {
                UnregisterPowerSettingNotification(hBatteryRemaining);
                hBatteryRemaining = IntPtr.Zero;
            }

            if (hBackgroundTask != IntPtr.Zero)
            {
                UnregisterPowerSettingNotification(hBackgroundTask);
                hBackgroundTask = IntPtr.Zero;
            }

            if (hPowerScheme != IntPtr.Zero)
            {
                UnregisterPowerSettingNotification(hPowerScheme);
                hPowerScheme = IntPtr.Zero;
            }
        }

        private void PowerSettingChange(IntPtr LParam)
        {
            POWERBROADCAST_SETTING ps = (POWERBROADCAST_SETTING)Marshal.PtrToStructure(LParam, typeof(POWERBROADCAST_SETTING));
            IntPtr pData = (IntPtr)(LParam.ToInt64() + Marshal.SizeOf(ps));

            if (ps.PowerSetting == GUID_ACDC_POWER_SOURCE && ps.DataLength == Marshal.SizeOf(typeof(int)))
            {
                PowerSourceChange((int)Marshal.PtrToStructure(pData, typeof(int)));
            }
            else if (ps.PowerSetting == GUID_BATTERY_PERCENTAGE_REMAINING && ps.DataLength == Marshal.SizeOf(typeof(int)))
            {
                BatteryPercentageChange((int)Marshal.PtrToStructure(pData, typeof(int)));
            }
            else if (ps.PowerSetting == GUID_IDLE_BACKGROUND_TASK)
            {
                /* The system is busy. This indicates that the system will not be moving into an idle state in the near future and that the current time is a good time for components to perform background or idle tasks that would otherwise prevent the computer from entering an idle state. There is no notification when the system is able to move into an idle state. The idle background task notification does not indicate whether a user is present at the computer. The Data member has no information and can be ignored. */
            }
            else if (ps.PowerSetting == GUID_POWERSCHEME_PERSONALITY && ps.DataLength == Marshal.SizeOf(typeof(Guid)))
            {
                PowerSchemeChange((Guid)Marshal.PtrToStructure(pData, typeof(Guid)));
            }
        }

        private void PowerSourceChange(int value)
        {
            switch ((SYSTEM_POWER_CONDITION)value)
            {
                case SYSTEM_POWER_CONDITION.PoAc:
                    break;
                case SYSTEM_POWER_CONDITION.PoDc:
                    break;
                case SYSTEM_POWER_CONDITION.PoHot:
                    break;
                default:
                    break;
            }
        }

        private void BatteryPercentageChange(int value)
        {
            throw new NotImplementedException();
        }

        private void PowerSchemeChange(Guid value)
        {
            PowerPersonality personality = GuidToEnum(value);
        }

        private static PowerPersonality GuidToEnum(Guid value)
        {
            if (value == GUID_MIN_POWER_SAVINGS)
            {
                return PowerPersonality.HighPerformance;
            }
            else if (value == GUID_MAX_POWER_SAVINGS)
            {
                return PowerPersonality.PowerSaver;
            }
            else if (value == GUID_TYPICAL_POWER_SAVINGS)
            {
                return PowerPersonality.Automatic;
            }
            else
            {
                return PowerPersonality.Unknown;
            }
        }

        /// <summary>
        /// Gets a value that indicates the current power scheme. 
        /// GUID_MIN_POWER_SAVINGS, GUID_MAX_POWER_SAVINGS, GUID_TYPICAL_POWER_SAVINGS, any unknown
        /// </summary>
        /// <exception cref="System.PlatformNotSupportedException">Requires Vista/Windows Server 2008.</exception>
        /// <value>A <see cref="PowerPersonality"/> value.</value>
        public static PowerPersonality ActivePowerScheme
        {
            get
            {
                Guid guid;
                PowerGetActiveScheme(IntPtr.Zero, out guid);
                try
                {
                    return GuidToEnum(guid);
                }
                finally
                {
                    LocalFree(ref guid);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                UnregisterForNotifications();
            }
            isDisposed = true;
        }
    }
}
