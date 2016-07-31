using DiabloInterface.ChatServer;
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
        private ChatClient _client;
        public ChatControl()
        {
            InitializeComponent();
            Name = "Chat Control";
        }

        public ChatControl(ChatClient client)
        {
            InitializeComponent();
            _client = client;
            Name = "Chat Control";
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            _client.Connect();
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            _client.LeaveChannel(txtChannel.Text);
            _client.Disconnect();
        }

        private void btnSendSimpleMessage_Click(object sender, EventArgs e)
        {
            _client.SendMessage(txtSimpleMessage.Text, txtChannel.Text);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _client.JoinChannel(txtChannel.Text);
        }
    }
}
