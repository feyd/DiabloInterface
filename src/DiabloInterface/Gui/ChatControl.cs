using DiabloInterface.ChatServer;
using DiabloInterface.D2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiabloInterface.Gui
{
    public partial class ChatControl : Form
    {
        private ChatClient _Client;
        private D2DataReader _Reader;
        public ChatControl()
        {
            InitializeComponent();
            Name = "Chat Control";
        }

        public ChatControl(D2DataReader reader)
        {
            InitializeComponent();
            Name = "Chat Control";
            _Reader = reader;

            txtServer.Text = Properties.ChatClientSettings.Default.Server;
            txtUsername.Text = Properties.ChatClientSettings.Default.User;
            txtPass.Text = Properties.ChatClientSettings.Default.Password;
            txtChannel.Text = Properties.ChatClientSettings.Default.Channel;
        }


        void InitializeChatClient()
        {
            _Client = new ChatClient(
                    new ChatConfig()
                    {
                        Server = txtServer.Text, 
                        User = txtUsername.Text, 
                        Password = txtPass.Text,
                        Channel = txtChannel.Text
                    },
                    _Reader
                    );
        }

        private void DisplayMessage(object sender, string msg)
        {
            txtOutput.BeginInvoke((MethodInvoker)delegate { txtOutput.AppendText(msg + System.Environment.NewLine); });
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            InitializeChatClient();
            _Client.MessageHandler += DisplayMessage;
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

    public class ChatConfig
    {
        public string Server { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Channel { get; set; }

    }
}
