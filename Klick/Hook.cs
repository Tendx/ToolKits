using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Klick;

public class Hook
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(WindowsHookType hookType, HookProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    private static extern int CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern IntPtr GetModuleHandle([MarshalAs(UnmanagedType.LPWStr)] string lpModuleName);

    private readonly Func<KeyboardDllHooks, bool, bool> _onKeyMessage;
    private IntPtr _hook = 0;

    public Hook(Func<KeyboardDllHooks, bool, bool> onKeyMessage) => _onKeyMessage = onKeyMessage;

    public void Init()
    {
        using var process = Process.GetCurrentProcess();
        using var module = process.MainModule;
        var hModule = GetModuleHandle(module!.ModuleName);
        _hook = SetWindowsHookEx(WindowsHookType.WH_KEYBOARD_LL, LowLevelKeyboardProc, hModule, 0);
    }

    public void Release()
    {
        if (_hook == 0)
            return;
        if (UnhookWindowsHookEx(_hook))
            _hook = 0;
    }

    private int LowLevelKeyboardProc(int code, IntPtr wParam, IntPtr lParam)
    {
        if (code >= 0)
        {
            var message = Marshal.PtrToStructure<KeyboardDllHooks>(lParam);
            var pressed = wParam == (int)WindowsMessage_Keyboard.WM_KEYDOWN || wParam == (int)WindowsMessage_Keyboard.WM_SYSKEYDOWN;
            if (message is not null && _onKeyMessage(message, pressed))
            {
                if (pressed)
                {
                    var inputs = new[]
                    {
                        new Input()
                        {
                            type = InputType.Mouse,
                            DummyUnionName = new() { mi = new() { dwFlags = MouseEventFlags.MOUSEEVENTF_LEFTDOWN } }
                        }
                    };
                    SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
                }
                else
                {
                    var inputs = new[]
                    {
                        new Input()
                        {
                            type = InputType.Mouse,
                            DummyUnionName = new() { mi = new() { dwFlags = MouseEventFlags.MOUSEEVENTF_LEFTUP } }
                        }
                    };
                    SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
                }
                code = -1;
            }
        }
        return CallNextHookEx(_hook, code, wParam, lParam);
    }
}
