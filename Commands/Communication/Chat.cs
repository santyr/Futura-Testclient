using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class ChatCommand: Command
    {	
        public ChatCommand(TestClient testClient)
		{
			Name = "chatSet";
			Description = "Turn Chatting on and off.";
		}

        public override string Execute(string[] args, LLUUID fromAgentID)
		{
			if (!Active)
			{
				Active = true;
                Client.Self.OnChat += new AgentManager.ChatCallback(Self_OnChat);
				return "Chatting is now on.";
			}
			else
			{
				Active = false;
                Client.Self.OnChat -= new AgentManager.ChatCallback(Self_OnChat);
				return "Chatting is now off.";
			}
		}

		public void Self_OnChat(string message, ChatAudibleLevel audible, ChatType type, 
            ChatSourceType sourcetype, string fromName, LLUUID id, LLUUID ownerid, LLVector3 position)
		{
			Console.WriteLine(fromName+":" + message);
			
			if (message.Length > 0 && message.ToLower().Contains(Client.Self.FirstName.ToLower()) && Client.Self.AgentID != id) {
				WebRequest request = WebRequest.Create("http://www.mr-technicl.com/slfutura.php?nick="+ fromName + "&message="+ message);
				WebResponse response = request.GetResponse();
				StreamReader input = new StreamReader(response.GetResponseStream());
				string say = input.ReadToEnd();
				input.Close();
				libsecondlife.Utilities.Realism.Chat(Client, say, ChatType.Normal, 25);
			}
		}
	}
}