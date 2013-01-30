using System;
using System.Windows.Forms;
using System.Diagnostics;

using System.IO;

namespace KeyClone
{
    public partial class WowKeyCloneForm : Form
    {
        private Interop _interop;
        private KeyHandler _keyHandler;

        public WowKeyCloneForm()
        {
            InitializeComponent();
            _keyHandler = new KeyHandler();
            _interop = new Interop(_keyHandler);            
        }

        private void trayContextExit_Click(object sender, EventArgs e)
        {
            this.Close();
            Application.Exit();
        }
    }
}
