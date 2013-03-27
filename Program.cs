using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

namespace OpenTray
{
    static class Program
    {
        static NotifyIcon _tray;
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            _tray = new NotifyIcon();
            _tray.ContextMenuStrip = new ContextMenuStrip();
            _tray.Click += new EventHandler(_tray_Click);
            _tray.Icon = new System.Drawing.Icon(Properties.Resources.media_optical_cdrom, new System.Drawing.Size(32, 32));
            _tray.Visible = true;
            _tray.Text = "OpenTray v" + Assembly.GetAssembly(Type.GetType("OpenTray.Program")).GetName().Version;
            // Add CDROM Drives
            DriveInfo[] Drives = DriveInfo.GetDrives();
            foreach (DriveInfo drive in Drives)
            {
                if (drive.DriveType == DriveType.CDRom)
                {
                    ToolStripMenuItem _drive = new ToolStripMenuItem();
                    _drive.Text = drive.Name;
                    _drive.Click += new EventHandler(_drive_Click);
                    _drive.Image = Properties.Resources.media_optical_audio_mount_3;
                    _tray.ContextMenuStrip.Items.Add(_drive);
                }
            }
            // Add exit option
            ToolStripMenuItem _exit = new ToolStripMenuItem("Exit");
            _exit.Click += new EventHandler(_exit_Click);
            _exit.Image = Properties.Resources.application_exit_3;
            _tray.ContextMenuStrip.Items.Add(_exit);
            Application.Run();
            _tray.Dispose();
        }

        static void _tray_Click(object sender, EventArgs e)
        {
            _tray.ContextMenuStrip.Show(Cursor.Position.X, Cursor.Position.Y);
        }

        static void _exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        static void _drive_Click(object sender, EventArgs e)
        {
            cEjectMedia myEjectMedia = new cEjectMedia();
            myEjectMedia.EjectMedia(sender.ToString().Substring(0, 2));
        }
    }
}
