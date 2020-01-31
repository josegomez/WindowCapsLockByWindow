using BrandonIsAWhinyBitch.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace BrandonIsAWhinyBitch
{
    public partial class frmCaps : Form
    {
        WinEventDelegate dele = null;
        List<WindowList> listOfWindows;
        
        public frmCaps()
        {
            InitializeComponent();
            listOfWindows = new List<WindowList>();
            if (!string.IsNullOrEmpty(Settings.Default.SavedWindows))
            {
                listOfWindows = Newtonsoft.Json.JsonConvert.DeserializeObject< List<WindowList>>(Settings.Default.SavedWindows);
                foreach(var x in listOfWindows)
                {
                    chkListWindows.Items.Add(x.Window, x.Checked);
                }
            }
            txtContains.Text = Settings.Default.Contains;
            dele = new WinEventDelegate(WinEventProc);
            IntPtr m_hhook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, dele, 0, 0, WINEVENT_OUTOFCONTEXT);

        }

        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const uint EVENT_SYSTEM_FOREGROUND = 3;

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags,UIntPtr dwExtraInfo);
        private string GetActiveWindowTitle()
        {
            const int nChars = 256;
            IntPtr handle = IntPtr.Zero;
            StringBuilder Buff = new StringBuilder(nChars);
            handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

        public void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            string WindowTitle = GetActiveWindowTitle()?.Trim();
            if (WindowTitle == null)
                WindowTitle = "";


            if (!chkListWindows.Items.Contains(WindowTitle))
            {
                bool capsOn= false;
                if(!string.IsNullOrEmpty(txtContains.Text))
                    foreach (string s in txtContains.Text.ToUpper().Split(','))
                    {
                        if (WindowTitle.ToUpper().Contains(s))
                            capsOn = true;
                    }
                if (!string.IsNullOrEmpty(WindowTitle))
                {
                    listOfWindows.Add(new WindowList { Checked = capsOn, Window = WindowTitle });
                    chkListWindows.Items.Add(WindowTitle, capsOn);
                }
            }
            
            
                if(Keyboard.GetKeyStates(Key.CapsLock) == KeyStates.Toggled)
                {
                    if(!chkListWindows.CheckedItems.Contains(WindowTitle))
                    {
                            ToggleCapsLock();
                    }
                }
                else
                {
                    if (chkListWindows.CheckedItems.Contains(WindowTitle))
                        ToggleCapsLock();
                }
            
        }

        public void ToggleCapsLock()
        {
            const int KEYEVENTF_EXTENDEDKEY = 0x1;
            const int KEYEVENTF_KEYUP = 0x2;
            keybd_event(0x14, 0x45, KEYEVENTF_EXTENDEDKEY, (UIntPtr)0);
            keybd_event(0x14, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP,
            (UIntPtr)0);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Default.Contains = txtContains.Text;
            var s = Newtonsoft.Json.JsonConvert.SerializeObject(this.listOfWindows.Where(w=>w.Checked).ToList());
            Settings.Default.SavedWindows = s;
            Settings.Default.Save();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            this.chkListWindows.Items.Clear();
            this.listOfWindows.Clear();
        }

        private void chkListWindows_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            foreach(var windowListItem in this.listOfWindows)
            {
                if(windowListItem.Window.Equals(chkListWindows.Items[e.Index]))
                {
                    windowListItem.Checked = e.NewValue == CheckState.Checked;
                }
            }
        }
    }
}
