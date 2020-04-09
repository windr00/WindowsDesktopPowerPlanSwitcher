using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsPowerPlan
{
    partial class Icon : IDisposable
    {
        private NotifyIcon icon = new NotifyIcon();

        private ContextMenuStrip menu = new ContextMenuStrip();

        private void Initialize()
        {

            menu.Opening += Menu_Opening;
            icon.Icon = TrayIcon;
            icon.Text = "Windows Power Plan Switcher";
            icon.ContextMenuStrip = menu;
            icon.Visible = true;
            Menu_Opening(this, new CancelEventArgs(false)); //pseudo first click
        }

        string[] runProcess(string cmd, params string[] args)
        {
            string arg = "";
            foreach (var a in args)
            {
                arg += a + " ";
            }
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = cmd,
                    Arguments = arg,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            var lines = new List<string>();
            while (!proc.StandardOutput.EndOfStream)
            {
                lines.Add(proc.StandardOutput.ReadLine());
            }
            return lines.ToArray();
        }
        


        private void Menu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            menu.Items.Clear();
            var lines = runProcess("powercfg.exe", "-l");
            foreach (var line in lines)
            {
                if (line.Contains("GUID"))
                {
                    var foundGUID = line.Split(' ')[2];
                    var menuItem = new ToolStripMenuItem(line, null, (o, args) => { runProcess("powercfg.exe", "-s", foundGUID); });
                    if (line.Contains("*"))
                    {
                        menuItem.Checked = true;
                        menuItem.CheckState = CheckState.Checked;
                    }
                    else
                    {
                        menuItem.Checked = false;
                        menuItem.CheckState = CheckState.Unchecked;
                    }
                    menu.Items.Add(menuItem);
                }
            }
        }

        public void Dispose()
        {
            icon.Visible = false;
            icon?.Dispose();
        }
    }
}
