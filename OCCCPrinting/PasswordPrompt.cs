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

namespace OCCCPrinting
{

    public partial class PasswordPrompt : Form
    {
        public string StudentId { get; set; }
        public string Password { get; set; }
        public PasswordPrompt()
        {
            InitializeComponent();
            SetForegroundWindow(Handle.ToInt32());
        }

        private void btPrint_Click(object sender, EventArgs e)
        {
            StudentId = tbStudentId.Text;
            Password = tbPassword.Text;
            this.Close();
        }

        [DllImport("User32.dll")]
        public static extern Int32 SetForegroundWindow(int hWnd);
    }
}
