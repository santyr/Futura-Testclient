// Attack.cs created with MonoDevelop
// User: santyr at 3:31 PM 10/4/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
	public class AttackCommand: Command
	{
		
		public AttackCommand(TestClient testClient)
		{
			Name = "attack";
            Description = "Attacks a Target. Usage: attack [FirstName LastName] [start stop]";
		}
		
		public override string Execute(string[] args, LLUUID fromAgentID)
        {
            string target = String.Empty;
			target = args[0] + " " + args[1];
			
			if (target.Length > 0)
            {
                if (Attack(target, args))
                    return "Attacking " + target;
                else
                    return "Not attacking " + target + ".";
				Active = false;
            }
			else
            {
                return "No target specified. usage: Attack [FirstName LastName] [start stop]";
				Active = false;
            }
			
        }
		
		const float DISTANCE_BUFFER = 4.0f;
        uint targetLocalID = 0;

        bool Attack(string name, string[] args)
        {
			bool start = true;

            if (args[2].ToLower() == "stop")
                start = false;
			
            if (start)
            {
	            lock (Client.Network.Simulators)
	            {
	                for (int i = 0; i < Client.Network.Simulators.Count; i++)
	                {
	                    Avatar target = Client.Network.Simulators[i].ObjectsAvatars.Find(
	                        delegate(Avatar avatar)
	                        {
	                            return avatar.Name == name;
	                        }
	                    );

	                    if (target != null)
	                    {
	                        targetLocalID = target.LocalID;
	                        Active = true;
	                        return true;
	                    }
	                }
				}
            } else {
				Active = false;
				return false;
			}
			
			Active = false;
				return false;
        }
		
		public override void Think()
		{
            // Find the target position
            lock (Client.Network.Simulators)
            {
				for (int i = 0; i < Client.Network.Simulators.Count; i++)
                {
                    Avatar targetAv;

                    if (Client.Network.Simulators[i].ObjectsAvatars.TryGetValue(targetLocalID, out targetAv))
                    {
                        float distance = 0.0f;

                        if (Client.Network.Simulators[i] == Client.Network.CurrentSim)
                        {
                            distance = LLVector3.Dist(targetAv.Position, Client.Self.SimPosition);
                        }
                        else
                        {
                            // FIXME: Calculate global distances
                        }

                        if (distance > DISTANCE_BUFFER)
                        {
                            uint regionX, regionY;
                            Helpers.LongToUInts(Client.Network.Simulators[i].Handle, out regionX, out regionY);

                            double xTarget = (double)targetAv.Position.X + (double)regionX;
                            double yTarget = (double)targetAv.Position.Y + (double)regionY;
                            double zTarget = targetAv.Position.Z - 5f;

                            Client.Self.AutoPilot(xTarget, yTarget, zTarget);
                        }
						else
                        {
                            // We are in range of the target and moving, stop moving
                            Client.Self.AutoPilotCancel();
                        }
						
						Shoot(Client, targetAv.Position);
						
                    }
                }
            }
			base.Think();	
		}
		
		/// <summary>
        /// Aims at the specified position, enters mouselook, presses and
        /// releases the left mouse button, and leaves mouselook
        /// </summary>
        /// <param name="target">Target to shoot at</param>
        /// <returns></returns>
        public static bool Shoot(SecondLife client, LLVector3 target)
        {
            if (client.Self.Movement.TurnToward(target))
                return Shoot(client);
            else
                return false;
        }

        /// <summary>
        /// Enters mouselook, presses and releases the left mouse button, and leaves mouselook
        /// </summary>
        /// <returns></returns>
        public static bool Shoot(SecondLife client)
        {
            if (client.Settings.SEND_AGENT_UPDATES)
            {
                client.Self.Movement.Mouselook = true;
                client.Self.Movement.MLButtonDown = true;
                client.Self.Movement.SendUpdate();

                client.Self.Movement.MLButtonUp = true;
                client.Self.Movement.MLButtonDown = false;
                client.Self.Movement.FinishAnim = true;
                client.Self.Movement.SendUpdate();

                client.Self.Movement.Mouselook = false;
                client.Self.Movement.MLButtonUp = false;
                client.Self.Movement.FinishAnim = false;
                client.Self.Movement.SendUpdate();

                return true;
            }
            else
            {
                Logger.Log("Attempted Shoot but agent updates are disabled", Helpers.LogLevel.Warning, client);
                return false;
            }
        }
	}
}
