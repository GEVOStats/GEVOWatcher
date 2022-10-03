
using System.Diagnostics;
using System.Management;

namespace GEVOWatcher
{
    internal static class Program
    {
        private const string gameName = "EvoGameSteam-Win64-Shipping";

        private static readonly ManagementEventWatcher gameStartWatcher = new($"SELECT * FROM __InstanceCreationEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_Process' AND TargetInstance.Name = '{gameName}.exe'");
        private static readonly ManagementEventWatcher gameStopWatcher = new($"SELECT  * FROM __InstanceDeletionEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_Process' AND TargetInstance.Name = '{gameName}.exe'");
        private static bool gameRunning = Process.GetProcesses().Any((x) => x.ProcessName == gameName);
        private static void StartWatching()
        {
            gameStartWatcher.EventArrived += new EventArrivedEventHandler(GameStarted);
            gameStopWatcher.EventArrived += new EventArrivedEventHandler(GameStopped);
            gameStartWatcher.Start();
            gameStopWatcher.Start();
        }

        private static void GameStarted(object sender, EventArrivedEventArgs e) => gameRunning = true;

        private static void GameStopped(object sender, EventArrivedEventArgs e)
        {
            gameRunning = false;
            Sync();
        }

        private static void Sync()
        {
            if (!gameRunning || DialogResult.Yes == MessageBox.Show("The game is currently running.\nSyncing now will cause issues, do you still want to sync?", "Sync Now?", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            {
                Process.Start("GEVOTicket.exe").WaitForExit();
            }
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (NotifyIcon icon = new())
            {
                icon.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                icon.ContextMenuStrip = new ContextMenuStrip()
                {
                    Items =
                    {
                        new ToolStripMenuItem("Sync Now", null, (s, e) => { Sync(); }),
                        new ToolStripMenuItem("Settings", null, (s, e) => { MessageBox.Show("Currently Unsupported.", "Error", MessageBoxButtons.OK,MessageBoxIcon.Error); }),
                        new ToolStripMenuItem("Exit", null, (s, e) => { Application.Exit(); })
                    }
                };
                icon.Visible = true;

                Application.Run();
                icon.Visible = false;
            }
            StartWatching();
        }
    }
}