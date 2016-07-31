using ChatSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiabloInterface.ChatServer
{
    public class ChannelManager
    {

        public ChatClient Client { get; set; }

        public string Channel { get; set; }

        private Dictionary<string, GenericCommand> _Commands;
        private Dictionary<string, DateTime> _LastUsed;

        public ChannelManager(ChatClient client, string channel)
        {
            Client = client;
            Channel = channel;

            _Commands = new Dictionary<string, GenericCommand>();
            _LastUsed = new Dictionary<string, DateTime>();

            AddCommand("!wr", new GenericCommand()
            {
                Action = PrintStringCommand,
                Parameter = "The world record for Hitman Absolution Expert/Max Ratings is... oh who cares, it is a terrible game anyway and only fools run it!"
            });

            AddCommand("!whydoesitsuck", new GenericCommand()
            {
                Action = PrintStringCommand,
                Parameter = "My thoughts on Absolution: http://forums.eidosgames.com/showthread.php?t=153432"
            });

            
        }

        public void HandleMessage(PrivateMessage msg) 
        {
           // see if we have something specific, if not pass it to global.
            ParseForCommand(msg);
        }

        private void ParseForCommand(PrivateMessage msg)
        {
            if (msg.Message.StartsWith("!"))
            {
                // we have a potential command

                string[] split = msg.Message.Split(' ');
                string command = split[0];

                if (_Commands.ContainsKey(command))
                {
                    //command found, check it wasnt called in last 30

                    if (_LastUsed.ContainsKey(command))
                    {
                        if ((DateTime.Now - _LastUsed[command]).TotalSeconds < 35)
                        {
                            Console.WriteLine("Not executing command, less than 35 seconds passed");
                        }
                        else
                        {
                            _LastUsed[command] = DateTime.Now;
                            _Commands[command].Execute();
                        }
                    }
                    else
                    {
                        _LastUsed.Add(command, DateTime.Now);
                        _Commands[command].Execute();
                    }
                }
            }
        }

        private void SendMessage(string message)
        {
            Client.SendMessage(message, Channel);
        }

        public void AddCommand(string command, GenericCommand comm)
        {
            _Commands.Add(command, comm);
        }

        public void AddSimpleCommand(string comm, string param)
        {
            _Commands.Add(comm, new GenericCommand() { Action = PrintStringCommand, Parameter = param });
        }

        void PrintStringCommand(object msg)
        {
            string message = (string)msg;
            Client.SendMessage(message, Channel);
        }
        
        public void Suspend()
        {
            // need to stop any timeds stuff etc
        }
    }

}
