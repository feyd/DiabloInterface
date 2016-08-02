using ChatSharp;
using DiabloInterface.D2;
using DiabloInterface.Server;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
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
        private D2DataReader _DataReader;

        public ChannelManager(ChatClient client, string channel, D2DataReader reader)
        {
            Client = client;
            Channel = channel;
            _DataReader = reader;

            _Commands = new Dictionary<string, GenericCommand>();
            _LastUsed = new Dictionary<string, DateTime>();

            AddCommand("simpletest", new GenericCommand()
            {
                Action = PrintStringCommand,
                Parameter = "A simple test reply to a simple test command"
            });

            AddCommand("item", new GenericCommand()
            {
                Action = GetItemCommand
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
                // we have a potential command, see if there is any more text after
                string command;
                string message;

                if (msg.Message.IndexOf(' ') < 0)
                {
                    command = msg.Message.Substring(1);
                    message = String.Empty;
                }
                else
                {
                    command = msg.Message.Substring(1, msg.Message.IndexOf(' ')-1);
                    message = msg.Message.Substring(msg.Message.IndexOf(' ') + 1);
                }

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
                            _Commands[command].Execute(message);
                        }
                    }
                    else
                    {
                        _LastUsed.Add(command, DateTime.Now);
                        _Commands[command].Execute(message);
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

        void PrintStringCommand(string message, object parameter)
        {
            Client.SendMessage((string)parameter, Channel);
        }

        void GetItemCommand(string message, object parameter)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                //todo: proper help message
                string help = "To use the item command please specify a location: helm, etc etc";
                Client.SendMessage(help, Channel);
                return;
            }

            string slot;

            if (message.IndexOf(' ') < 0)
            {
                slot = message;
            }
            else
            {
                slot = message.Substring(0, message.IndexOf(' '));
            }

            //todo: need to move all this to async calls

            List<ItemResponse> response = new List<ItemResponse>();

            _DataReader.ItemSlotAction(Helpers.Utility.ParseItemLocation(slot),
                (itemReader, item) => {
                    ItemResponse data = new ItemResponse();
                    data.ItemName = itemReader.GetFullItemName(item);
                    data.Properties = itemReader.GetMagicalStrings(item);
                    response.Add(data);
                });

            foreach (var item in response)
            {
                string properties = string.Join(", ", item.Properties);
                if(String.IsNullOrWhiteSpace(properties))
                    Client.SendMessage($"{item.ItemName}: Nothing special", Channel);
                else
                    Client.SendMessage($"{item.ItemName}: {properties}", Channel);
            }
        }


        public void Suspend()
        {
            // need to stop any timeds stuff etc
        }
    }

}
