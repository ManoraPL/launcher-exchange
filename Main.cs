using System;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using ManoraLau.Properties;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Net;

namespace ManoraLau
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private readonly WebClient webClient = new WebClient();

        private void Form1_Load(object sender, EventArgs e)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "node .\\app.js";
            process.StartInfo = startInfo;
            process.Start();
        }

        //zrobmy tak, zeby okno sie ruszało, bo tak w miejscu to troche srednio
        private bool mouseDown;
        private Point lastLocation;

        private void Main_MouseDown(object sender, MouseEventArgs e) //x
        {
            mouseDown = true;
            lastLocation = e.Location;
        }

        private void Main_MouseMove(object sender, MouseEventArgs e) //y
        {
            if (mouseDown)
            {
                this.Location = new Point(
                    (this.Location.X - lastLocation.X) + e.X, (this.Location.Y - lastLocation.Y) + e.Y);

                this.Update();
            }
        }

        private void Main_MouseUp(object sender, MouseEventArgs e) //z
        {
            mouseDown = false;
        }


        private void launchButton_Click(object sender, EventArgs e)
        {
            string SPLASH = Path.Combine(Settings.Default.Path, "FortniteGame\\Content\\Splash\\Splash.bmp"); // splash musi być! | MaTiD
            try
            {
                File.Delete(SPLASH);
            }
            catch
            {
            }
            try
            {
                webClient.DownloadFile("https://cdn.discordapp.com/attachments/760877606584320032/830106377548202064/Splash.bmp", SPLASH);
            }
            catch
            {
            }
            string loginArgs = "-AUTH_LOGIN=unused -AUTH_PASSWORD=" + " -AUTH_TYPE=exchangecode";
            string mainArgs = " -epicapp=Fortnite -epicenv=Prod -epiclocale=en-us -epicportal -noeac -fromfl=be -fltoken=h1h4370717422124b232eFac -skippatchcheck";
            string Args = loginArgs += mainArgs; // nie chce syfu w kodzie | artii
            Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
            try
            {
                if (!this.IsValidPath(this.pathTextBox.Text))
                {
                    MessageBox.Show("You have specified the wrong Fortnite file location.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
            }
            catch
            {
                MessageBox.Show("You have specified the wrong Fortnite file location, make sure that you put great path", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            string FNClient = Path.Combine(this.pathTextBox.Text, "FortniteGame\\Binaries\\Win64\\FortniteClient-Win64-Shipping.exe");
            if (!File.Exists(FNClient))
            {
                MessageBox.Show("\"FortniteClient-Win64-Shipping.exe\" is not existing, where did you lose it? Lol", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            Settings.Default.Path = this.pathTextBox.Text;
            Settings.Default.Save();
            string DLL1 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ManoraSSL.dll");
            if (!File.Exists(DLL1))
            {
                MessageBox.Show("\"ManoraSSL.dll\" is missing, just put it in the folder where the launcher is.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand); //Jak to NASZ runtime, co ty gadasz lol
                return;
            }
            string DLL2 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Matchmaking.dll");
            if (!File.Exists(DLL2))
            {
                MessageBox.Show("\"Matchmaking.dll\" is missing, just put it in the folder where the launcher is.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            Main._clientProcess = new Process
            {
                StartInfo = new ProcessStartInfo(FNClient, Args)
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = false
                }
            };
            Main._clientProcess.Start();
            Task.Run(delegate ()
            {
                Main._clientProcess.WaitForInputIdle();
                Main.InternetSetOption(IntPtr.Zero, 39, IntPtr.Zero, 0);
                Main.InternetSetOption(IntPtr.Zero, 37, IntPtr.Zero, 0);
                this.Inject(DLL1);
                this.Inject(DLL2);
                Environment.Exit(0);
            });
            return;
        }          

        private static Process _clientProcess;

        private bool IsValidPath(string path)
        {
            if (new Regex("^[a-zA-Z]:\\\\$").IsMatch(path.Substring(0, 3)))
            {
                string text = new string(Path.GetInvalidPathChars());
                text += ":/?*\"";
                return !new Regex("[" + Regex.Escape(text) + "]").IsMatch(path.Substring(3, path.Length - 3));
            }
            return false;
        }

        private void Inject(string DllPath)
        {
            IntPtr hProcess = Win32.OpenProcess(1082, false, Main._clientProcess.Id);
            IntPtr procAddress = Win32.GetProcAddress(Win32.GetModuleHandle("kernel32.dll"), "LoadLibraryA");
            uint num = (uint)((DllPath.Length + 1) * Marshal.SizeOf(typeof(char)));
            IntPtr intPtr = Win32.VirtualAllocEx(hProcess, IntPtr.Zero, num, 12288U, 4U);
            UIntPtr uintPtr;
            Win32.WriteProcessMemory(hProcess, intPtr, Encoding.Default.GetBytes(DllPath), num, out uintPtr);
            Win32.CreateRemoteThread(hProcess, IntPtr.Zero, 0U, procAddress, intPtr, 0U, IntPtr.Zero);
        }

        [DllImport("wininet.dll")]
        public static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);


        private void pathTextBox_TextChanged_1(object sender, EventArgs e)
        {
            Settings.Default.Path = pathTextBox.Text;
            Settings.Default.Save();
        }

        private void button1_Click(object sender, EventArgs e) //guzik do Discorda better | MaTiD
        {
           System.Diagnostics.Process.Start("http://bit.ly/DiscordMaTiD");
        }

        private void launchButton_MouseDown(object sender, MouseEventArgs e)
        {

        }

        private void label4_Click_1(object sender, EventArgs e)
        {
        }



        private void button2_Click(object sender, EventArgs e)
        {
            string old_path = pathTextBox.Text;
            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                pathTextBox.Text = dialog.FileName;
                Settings.Default.Path = pathTextBox.Text;
                Settings.Default.Save();
            }
            else
            {
                pathTextBox.Text = old_path;
                Settings.Default.Path = pathTextBox.Text;
                Settings.Default.Save();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var text =
                "Manora, prywatny serwer w Fortnite, by MaTiD" + "\n\n" +
                "Launcher napisany przez: MaTiDa, Artiiego, oraz Sizzy" + "\n\n" +
                "Pamiętaj aby zarejestrować się na http://manora.tk" + "\n\n" +
                "Jeśli kupiłeś ten program, zostałeś oszukany, skontaktuj się z developerem.";
            MessageBox.Show(text);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
