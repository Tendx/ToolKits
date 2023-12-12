using System.Diagnostics;
using System.Runtime.InteropServices;
using static Win32Helper;

if (args.Length == 0)
    return;

if (Environment.UserName == "Administrator")
    return;

var inGroup = false;
NetUserGetLocalGroups(string.Empty, Environment.UserName, 0, 0, out IntPtr bufPtr, 1024, out int entriesRead, out int totalEntries);
if (entriesRead <= 0)
    return;
var iter = bufPtr;
for (int i = 0; i < entriesRead; i++)
{
    var group = Marshal.PtrToStructure<LOCALGROUP_USERS_INFO_0>(iter);
    //Console.WriteLine(group.groupname);
    if (string.Compare(group.groupname, args[0], true) == 0)
    {
        inGroup = true;
        break;
    }
    iter += Marshal.SizeOf(typeof(LOCALGROUP_USERS_INFO_0));
}
NetApiBufferFree(bufPtr);
if (!inGroup)
    return;

var isRdp = false;
var process = Process.GetCurrentProcess();
if (!WTSQuerySessionInformation(IntPtr.Zero, process.SessionId, WTS_INFO_CLASS.WTSWinStationName, out var pBuffer, out var _))
    return;
if (Marshal.PtrToStringAnsi(pBuffer)?.StartsWith("RDP-tcp", StringComparison.OrdinalIgnoreCase) == true)
    isRdp = true;
WTSFreeMemory(pBuffer);
if (!isRdp)
    return;

if (Process.GetProcesses().Any(p => p.SessionId == process.SessionId && p.ProcessName == "userinit"))
    ExitWindowsEx(0, 0);

class Win32Helper
{
    #region group
    [DllImport("netapi32.dll")]
    public extern static int NetUserGetLocalGroups([MarshalAs(UnmanagedType.LPWStr)] string servername, [MarshalAs(UnmanagedType.LPWStr)] string username, int level, int flags, out IntPtr bufptr, int prefmaxlen, out int entriesread, out int totalentries);

    [DllImport("netapi32.dll")]
    public extern static int NetApiBufferFree(IntPtr Buffer);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct LOCALGROUP_USERS_INFO_0
    {
        public string groupname;
    }
    #endregion

    #region session
    [DllImport("Wtsapi32.dll")]
    public static extern bool WTSQuerySessionInformation(IntPtr pServer, int iSessionID, WTS_INFO_CLASS oInfoClass, out IntPtr pBuffer, out uint iBytesReturned);

    [DllImport("wtsapi32.dll")]
    public static extern void WTSFreeMemory(IntPtr pMemory);

    public enum WTS_INFO_CLASS
    {
        WTSInitialProgram,
        WTSApplicationName,
        WTSWorkingDirectory,
        WTSOEMId,
        WTSSessionId,
        WTSUserName,
        WTSWinStationName,
        WTSDomainName,
        WTSConnectState,
        WTSClientBuildNumber,
        WTSClientName,
        WTSClientDirectory,
        WTSClientProductId,
        WTSClientHardwareId,
        WTSClientAddress,
        WTSClientDisplay,
        WTSClientProtocolType,
        WTSIdleTime,
        WTSLogonTime,
        WTSIncomingBytes,
        WTSOutgoingBytes,
        WTSIncomingFrames,
        WTSOutgoingFrames,
        WTSClientInfo,
        WTSSessionInfo,
        WTSConfigInfo,
        WTSValidationInfo,
        WTSSessionAddressV4,
        WTSIsRemoteSession
    }
    #endregion

    #region logoff
    [DllImport("user32")]
    public static extern bool ExitWindowsEx(uint uFlags, uint dwReason);
    #endregion
}
