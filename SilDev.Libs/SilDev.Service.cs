
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region '

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace SilDev
{
    /// <summary>Requirements:
    /// <para><see cref="SilDev.Convert"/>.cs</para>
    /// <para><see cref="SilDev.Crypt"/>.cs</para>
    /// <para><see cref="SilDev.Log"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class Service
    {
        [SuppressUnmanagedCodeSecurity]
        private static class SafeNativeMethods
        {
            [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerA", BestFitMapping = false, SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Ansi)]
            internal static extern IntPtr OpenSCManager([MarshalAs(UnmanagedType.LPStr)]string lpMachineName, [MarshalAs(UnmanagedType.LPStr)]string lpDatabaseName, ServiceManagerRights dwDesiredAccess);

            [DllImport("advapi32.dll", EntryPoint = "OpenServiceA", BestFitMapping = false, SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Ansi)]
            internal static extern IntPtr OpenService(IntPtr hSCManager, [MarshalAs(UnmanagedType.LPStr)]string lpServiceName, ServiceRights dwDesiredAccess);

            [DllImport("advapi32.dll", EntryPoint = "CreateServiceA", BestFitMapping = false, SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Ansi)]
            internal static extern IntPtr CreateService(IntPtr hSCManager, [MarshalAs(UnmanagedType.LPStr)]string lpServiceName, [MarshalAs(UnmanagedType.LPStr)]string lpDisplayName, ServiceRights dwDesiredAccess, int dwServiceType, ServiceBootFlag dwStartType, ServiceError dwErrorControl, [MarshalAs(UnmanagedType.LPStr)]string lpBinaryPathName, [MarshalAs(UnmanagedType.LPStr)]string lpLoadOrderGroup, IntPtr lpdwTagId, [MarshalAs(UnmanagedType.LPStr)]string lpDependencies, [MarshalAs(UnmanagedType.LPStr)]string lp, [MarshalAs(UnmanagedType.LPStr)]string lpPassword);

            [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
            internal static extern int CloseServiceHandle(IntPtr hSCObject);

            [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
            internal static extern int QueryServiceStatus(IntPtr hService, SERVICE_STATUS lpServiceStatus);

            [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
            internal static extern int DeleteService(IntPtr hService);

            [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
            internal static extern int ControlService(IntPtr hService, ServiceControl dwControl, SERVICE_STATUS lpServiceStatus);

            [DllImport("advapi32.dll", EntryPoint = "StartServiceA", SetLastError = true, CharSet = CharSet.Ansi)]
            internal static extern int StartService(IntPtr hService, int dwNumServiceArgs, int lpServiceArgVectors);
        }

        private static IntPtr OpenSCManager(ServiceManagerRights serviceRights)
        {
            IntPtr scman = SafeNativeMethods.OpenSCManager(null, null, serviceRights);
            try
            {
                if (scman == IntPtr.Zero)
                    throw new ApplicationException("Could not connect to service control manager.");
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
            return scman;
        }

        [Flags]
        public enum ServiceManagerRights
        {
            Connect = 0x1,
            CreateService = 0x2,
            EnumerateService = 0x4,
            Lock = 0x8,
            QueryLockStatus = 0x10,
            ModifyBootConfig = 0x20,
            StandardRightsRequired = 0xF0000,
            AllAccess = (StandardRightsRequired | Connect | CreateService | EnumerateService | Lock | QueryLockStatus | ModifyBootConfig)
        }

        [Flags]
        public enum ServiceRights
        {
            QueryConfig = 0x00001,
            ChangeConfig = 0x00002,
            QueryStatus = 0x00004,
            EnumerateDependants = 0x00008,
            Start = 0x00010,
            Stop = 0x00020,
            PauseContinue = 0x00040,
            Interrogate = 0x00080,
            UserDefinedControl = 0x00100,
            Delete = 0x10000,
            StandardRightsRequired = 0xF0000,
            AllAccess = (StandardRightsRequired | QueryConfig | ChangeConfig | QueryStatus | EnumerateDependants | Start | Stop | PauseContinue | Interrogate | UserDefinedControl)
        }

        public enum ServiceBootFlag
        {
            Start = 0x0,
            SystemStart = 0x1,
            AutoStart = 0x2,
            DemandStart = 0x3,
            Disabled = 0x4
        }

        public enum ServiceState
        {
            Unknown = -1,
            NotFound = 0,
            Stop = 1,
            Run = 2,
            Stopping = 3,
            Starting = 4,
        }

        public enum ServiceControl
        {
            Stop = 0x1,
            Pause = 0x2,
            Continue = 0x3,
            Interrogate = 0x4,
            Shutdown = 0x5,
            ParamChange = 0x6,
            NetBindAdd = 0x7,
            NetBindRemove = 0x8,
            NetBindEnable = 0x9,
            NetBindDisable = 0xA
        }

        public enum ServiceError
        {
            Ignore = 0x0,
            Normal = 0x1,
            Severe = 0x2,
            Critical = 0x3
        }

        private const int STANDARD_RIGHTS_REQUIRED = 0xF0000;
        private const int SERVICE_WIN32_OWN_PROCESS = 0x00010;

        [StructLayout(LayoutKind.Sequential)]
        internal class SERVICE_STATUS
        {
            internal int dwServiceType = 0;
            internal ServiceState dwCurrentState = 0;
            internal int dwControlsAccepted = 0;
            internal int dwWin32ExitCode = 0;
            internal int dwServiceSpecificExitCode = 0;
            internal int dwCheckPoint = 0;
            internal int dwWaitHint = 0;
        }

        public static void Install(string serviceName, string displayName, string path, string args = "")
        {
            IntPtr scman = OpenSCManager(ServiceManagerRights.Connect | ServiceManagerRights.CreateService);
            try
            {
                IntPtr service = SafeNativeMethods.OpenService(scman, serviceName, ServiceRights.QueryStatus | ServiceRights.Start);
                if (service == IntPtr.Zero)
                    service = SafeNativeMethods.CreateService(scman, serviceName, displayName, ServiceRights.QueryStatus | ServiceRights.Start, SERVICE_WIN32_OWN_PROCESS, ServiceBootFlag.AutoStart, ServiceError.Normal, $"{path} {args}".TrimEnd(), null, IntPtr.Zero, null, null, null);
                if (service == IntPtr.Zero)
                    throw new ApplicationException("Failed to install service.");
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
            finally
            {
                SafeNativeMethods.CloseServiceHandle(scman);
            }
        }

        public static void Install(string serviceName, string path) =>
            Install(serviceName, serviceName, path, string.Empty);

        public static void Uninstall(string serviceName)
        {
            IntPtr scman = OpenSCManager(ServiceManagerRights.Connect);
            try
            {
                IntPtr service = SafeNativeMethods.OpenService(scman, serviceName, ServiceRights.StandardRightsRequired | ServiceRights.Stop | ServiceRights.QueryStatus);
                if (service == IntPtr.Zero)
                    throw new ApplicationException("Service not installed.");
                try
                {
                    Stop(service);
                    int ret = SafeNativeMethods.DeleteService(service);
                    if (ret == 0)
                    {
                        int error = Marshal.GetLastWin32Error();
                        throw new ApplicationException("Could not delete service " + error);
                    }
                }
                finally
                {
                    SafeNativeMethods.CloseServiceHandle(service);
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
            finally
            {
                SafeNativeMethods.CloseServiceHandle(scman);
            }
        }

        public static bool Exists(string serviceName)
        {
            IntPtr scman = OpenSCManager(ServiceManagerRights.Connect);
            try
            {
                IntPtr service = SafeNativeMethods.OpenService(scman, serviceName, ServiceRights.QueryStatus);
                if (service == IntPtr.Zero)
                    return false;
                SafeNativeMethods.CloseServiceHandle(service);
                return true;
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
            finally
            {
                SafeNativeMethods.CloseServiceHandle(scman);
            }
            return false;
        }

        public static void Start(string serviceName)
        {
            IntPtr scman = OpenSCManager(ServiceManagerRights.Connect);
            try
            {
                IntPtr hService = SafeNativeMethods.OpenService(scman, serviceName, ServiceRights.QueryStatus |
                ServiceRights.Start);
                if (hService == IntPtr.Zero)
                    throw new ApplicationException("Could not open service.");
                try
                {
                    Start(hService);
                }
                finally
                {
                    SafeNativeMethods.CloseServiceHandle(hService);
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
            finally
            {
                SafeNativeMethods.CloseServiceHandle(scman);
            }
        }

        public static void Stop(string serviceName)
        {
            IntPtr scman = OpenSCManager(ServiceManagerRights.Connect);
            try
            {
                IntPtr hService = SafeNativeMethods.OpenService(scman, serviceName, ServiceRights.QueryStatus |
                ServiceRights.Stop);
                if (hService == IntPtr.Zero)
                    throw new ApplicationException("Could not open service.");
                try
                {
                    Stop(hService);
                }
                finally
                {
                    SafeNativeMethods.CloseServiceHandle(hService);
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
            finally
            {
                SafeNativeMethods.CloseServiceHandle(scman);
            }
        }

        private static void Start(IntPtr hService)
        {
            SafeNativeMethods.StartService(hService, 0, 0);
            WaitForStatus(hService, ServiceState.Starting, ServiceState.Run);
        }

        private static void Stop(IntPtr hService)
        {
            SERVICE_STATUS status = new SERVICE_STATUS();
            SafeNativeMethods.ControlService(hService, ServiceControl.Stop, status);
            WaitForStatus(hService, ServiceState.Stopping, ServiceState.Stop);
        }

        public static ServiceState GetStatus(string serviceName)
        {
            IntPtr scman = OpenSCManager(ServiceManagerRights.Connect);
            try
            {
                IntPtr hService = SafeNativeMethods.OpenService(scman, serviceName, ServiceRights.QueryStatus);
                if (hService == IntPtr.Zero)
                    return ServiceState.NotFound;
                try
                {
                    return GetStatus(hService);
                }
                finally
                {
                    SafeNativeMethods.CloseServiceHandle(scman);
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
            finally
            {
                SafeNativeMethods.CloseServiceHandle(scman);
            }
            return ServiceState.NotFound;
        }

        private static ServiceState GetStatus(IntPtr hService)
        {
            SERVICE_STATUS ssStatus = new SERVICE_STATUS();
            try
            {
                if (SafeNativeMethods.QueryServiceStatus(hService, ssStatus) == 0)
                    throw new ApplicationException("Failed to query service status.");
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
            return ssStatus.dwCurrentState;
        }

        private static bool WaitForStatus(IntPtr hService, ServiceState WaitStatus, ServiceState DesiredStatus)
        {
            SERVICE_STATUS status = new SERVICE_STATUS();
            try
            {
                int dwOldCheckPoint;
                int dwStartTickCount;

                SafeNativeMethods.QueryServiceStatus(hService, status);
                if (status.dwCurrentState == DesiredStatus)
                    return true;
                dwStartTickCount = Environment.TickCount;
                dwOldCheckPoint = status.dwCheckPoint;

                while (status.dwCurrentState == WaitStatus)
                {
                    int dwWaitTime = status.dwWaitHint / 10;
                    dwWaitTime = dwWaitTime < 1000 ? 1000 : dwWaitTime > 10000 ? 10000 : dwWaitTime;
                    System.Threading.Thread.Sleep(dwWaitTime);
                    if (SafeNativeMethods.QueryServiceStatus(hService, status) == 0)
                        break;
                    if (status.dwCheckPoint > dwOldCheckPoint)
                    {
                        dwStartTickCount = Environment.TickCount;
                        dwOldCheckPoint = status.dwCheckPoint;
                    }
                    else
                    {
                        if (Environment.TickCount - dwStartTickCount > status.dwWaitHint)
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
            return status.dwCurrentState == DesiredStatus;
        }
    }
}

#endregion
