using System.Reflection;

namespace Klick;

internal static class Program
{
    private static Icon _icon0 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream($"{nameof(Klick)}.touch0.ico")!);
    private static Icon _icon1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream($"{nameof(Klick)}.touch1.ico")!);
    private static NotifyIcon _notify = new() { Icon = _icon1, Visible = true, ContextMenuStrip = new() };
    private static bool _pressed = false;
    private static Hook _hook = new((message, pressed) =>
    {
        if (message.vkCode == VirtualKey.RMENU && _pressed != pressed)
        {
            _notify.Icon = pressed ? _icon0 : _icon1;
            _pressed = pressed;
            return true;
        }
        return false;
    });

    [STAThread]
    static void Main()
    {
        if (_notify.ContextMenuStrip!.Items.Add("ÆôÓÃ") is ToolStripMenuItem enable)
        {
            enable.CheckedChanged += (s, e) =>
            {
                if (enable.Checked)
                    _hook.Init();
                else
                    _hook.Release();
            };
            enable.CheckOnClick = true;
            enable.Checked = true;
        }
        if (_notify.ContextMenuStrip.Items.Add("ÍË³ö") is ToolStripMenuItem exit)
        {
            exit.Click += (s, e) =>
            {
                _hook.Release();
                Application.Exit();
            };
        }
        Application.Run();
    }
}