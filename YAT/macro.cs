﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YAT
{
    public partial class macro : UserControl
    {
        public macro()
        {
            InitializeComponent();
        }

        private void macro_Load(object sender, EventArgs e)
        {

        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            //send the data now
            Program.g_mainView.SendCommand(txtCommand.Text);
        }

        private void chkSendCommand_CheckedChanged(object sender, EventArgs e)
        {

        }

        public void SendIfChecked()
        {
            if(chkSendCommand.Checked == true)
            {
                btnSend.PerformClick();
            }
        }
    }
}
