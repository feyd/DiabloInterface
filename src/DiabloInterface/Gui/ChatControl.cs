﻿using DiabloInterface.ChatServer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiabloInterface.Gui
{
    public partial class ChatControl : Form
    {
        private ChatClient _Client;
        public ChatControl()
        {
            InitializeComponent();
            Name = "Chat Control";
        }

        public ChatControl(ChatClient client)
        {
            InitializeComponent();
            _Client = client;
            Name = "Chat Control";
            _Client.MessageHandler += DisplayMessage;
        }

        private void DisplayMessage(object sender, string msg)
        {
            txtOutput.BeginInvoke((MethodInvoker)delegate { txtOutput.AppendText(msg + System.Environment.NewLine); });
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            _Client.Connect();
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            _Client.LeaveChannel(txtChannel.Text);
            _Client.Disconnect();
        }

        private void btnSendSimpleMessage_Click(object sender, EventArgs e)
        {
            _Client.SendMessage(txtSimpleMessage.Text, txtChannel.Text);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _Client.JoinChannel(txtChannel.Text);
        }
    }
}
