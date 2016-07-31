using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatSharp;
using System.Threading;
using DiabloInterface.D2;

namespace DiabloInterface.ChatServer
{
    public class GenericCommand
    {
        public Action<string, object> Action { get; set; }
        public Object Parameter { get; set; }

        public void Execute(string message)
        {
            Action(message, Parameter);
        }
    }

    public class ChatClient
    {
        public class SimpleCommand
        {
            public string Message { get; set; } 

        }
        public string Server {get; set;}
        public string User {get; set;}
        public string Password {get; set;}
        public string Nick {get; set;}
        public string ActiveChannel { get; set; }

        private ChannelManager manager;

        private IrcClient _Client;
        private IrcUser _User;
        private Thread _ClientThread;
        private D2DataReader _DataReader;

        public ChatClient(string server, string user, string password, string nick, string channel, D2DataReader dataReader)
        {
            Server = server;
            User = user;
            Password = password;
            Nick = nick;
            ActiveChannel = channel;
            _DataReader = dataReader;
        }

        public void Connect()
        {            
            _User = new IrcUser(Nick, User, Password);
            _Client = new IrcClient(Server, _User);

            _Client.NetworkError += Client_NetworkError;
            _Client.RawMessageRecieved += Client_RawMessageRecieved;
            _Client.RawMessageSent += Client_RawMessageSent;
            _Client.UserMessageRecieved += Client_UserMessageRecieved;
            _Client.ChannelMessageRecieved += Client_ChannelMessageRecieved;

            _ClientThread = new Thread(ThreadInitialiser);
            _ClientThread.Start();
        }

        public void Disconnect()
        {
            _Client.Quit();
            _ClientThread.Abort();
        }

        public void ThreadInitialiser()
        {
            _Client.ConnectAsync();
        }

        void Client_ChannelMessageRecieved(object sender, ChatSharp.Events.PrivateMessageEventArgs e)
        {

            string channel = e.PrivateMessage.Source;
            manager.HandleMessage(e.PrivateMessage);

            string message = string.Format("CM: {0} <{1}> {2}", channel, e.PrivateMessage.User.Nick, e.PrivateMessage.Message);
            Console.WriteLine(message);
            OnMessage(message);
        }

        void Client_UserMessageRecieved(object sender, ChatSharp.Events.PrivateMessageEventArgs e)
        {

            OnMessage("UM:" + e.PrivateMessage.Message);

            if (e.PrivateMessage.Message.StartsWith(".join "))
                _Client.Channels.Join(e.PrivateMessage.Message.Substring(6));
            else if (e.PrivateMessage.Message.StartsWith(".list "))
            {
                var channel = _Client.Channels[e.PrivateMessage.Message.Substring(6)];
                var list = channel.Users.Select(u => u.Nick).Aggregate((a, b) => a + "," + b);
                _Client.SendMessage(list, e.PrivateMessage.User.Nick);
            }
            else if (e.PrivateMessage.Message.StartsWith(".whois "))
                _Client.WhoIs(e.PrivateMessage.Message.Substring(7), null);
            else if (e.PrivateMessage.Message.StartsWith(".raw "))
                _Client.SendRawMessage(e.PrivateMessage.Message.Substring(5));
            else if (e.PrivateMessage.Message.StartsWith(".mode "))
            {
                var parts = e.PrivateMessage.Message.Split(' ');
                _Client.ChangeMode(parts[1], parts[2]);
            }
        }

        void Client_RawMessageSent(object sender, ChatSharp.Events.RawMessageEventArgs e)
        {
            Console.WriteLine(">> {0}", e.Message);
            OnMessage(">> " + e.Message);
        }

        void Client_RawMessageRecieved(object sender, ChatSharp.Events.RawMessageEventArgs e)
        {
            Console.WriteLine("<< {0}", e.Message);
            OnMessage(">> " + e.Message);
        }

        void Client_NetworkError(object sender, ChatSharp.Events.SocketErrorEventArgs e)
        {
            Console.WriteLine("Error: " + e.SocketError);
        }

        public event EventHandler<string> MessageHandler;
        protected internal virtual void OnMessage(string e)
        {
            if (MessageHandler != null) MessageHandler(this, e);
        }

        public void SendMessage(string message, string channel)
        {
            _Client.SendMessage(message, new[]{channel});
        }


        public void JoinChannel(string p)
        {
            _Client.JoinChannel(p);
            ChannelManager mgr = new ChannelManager(this, p, _DataReader);
        }

        public void LeaveChannel(string channel)
        {
            // lets just pause it, as we may rejoin and will save rebuilding data from disk
            // it wont get any messages as we left channel.

            _Client.PartChannel(channel);
            manager.Suspend();
        }

        public void AddSimpleCommand(string channel, string command, string output)
        {
            manager.AddSimpleCommand(command, output);
        }
    }
}
