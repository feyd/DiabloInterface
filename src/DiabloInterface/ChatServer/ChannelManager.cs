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

        private List<string> validSlots = new List<string>()
        {
            "helm",
            "body",
            "amulet",
            "rings",
            "belt",
            "gloves",
            "boots",
            "weapon1",
            "offhand1",
            "weapon2",
            "offhand2"
        };

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
                // we have a potential command
                string command = msg.Message.Substring(1, msg.Message.IndexOf(' '));
                string message = msg.Message.Substring(msg.Message.IndexOf(' ') + 1);

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
                // need to send a how to use to chat
                return;
            }

            string slot = message.Substring(0, message.IndexOf(' '));

            if (!validSlots.Contains(slot))
            {
                // need to send a how to use to chat
                return;
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
                Client.SendMessage(item.ItemName, Channel);
            }
        }


        public void Suspend()
        {
            // need to stop any timeds stuff etc
        }
    }

}
