using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace omd.src.data
{
    public class ThresholdItem
    {
        public int amount;
        public bool exact;
        public string message;
        public string command;

        public ThresholdItem(int amount, bool exact, string message, string command)
        {
            this.amount = amount;
            this.exact = exact;
            this.message = message;
            this.command = command;
        }

        public string GetMessage(int amount, string nickname, string message)
        {
            string formattedMessage = this.message;
            formattedMessage = formattedMessage.Replace("%name%", nickname);
            formattedMessage = formattedMessage.Replace("%amount%", amount.ToString());
            formattedMessage = formattedMessage.Replace("%message%", message);
            return formattedMessage;
        }

        public string GetMessage(Reward reward)
        {
            return GetMessage(reward.amount, reward.name, reward.message);
        }

        public string GetCommand()
        {
            return command;
        }

        public void RunCommands()
        {
            List<string> commands = GetCommand().Split(';').ToList();
            foreach(string cmd in commands)
            {
                OMD.clientApi.SendChatMessage(cmd);
            }
        }

    }
}
