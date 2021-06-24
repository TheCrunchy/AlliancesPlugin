﻿using Sandbox.Game.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch.Commands;
using Torch.Commands.Permissions;
using VRage.Game.ModAPI;
using AlliancesPlugin.Alliances;
using Sandbox.Game.Screens.Helpers;
using VRageMath;
using System.IO;
using Sandbox.Engine.Multiplayer;
using Torch.Mod.Messages;
using Torch.Mod;

namespace AlliancesPlugin
{
   public class MiscCommands : CommandModule
    {
        public static Dictionary<long, DateTime> distressCooldowns = new Dictionary<long, DateTime>();
        public static Dictionary<long, int> distressAmounts = new Dictionary<long, int>();
        [Command("distress", "distress signals")]
        [Permission(MyPromoteLevel.None)]
        public void distress(string reason = "")
        {


            if (Context.Player == null)
            {
                Context.Respond("no no console no distress");
                return;
            }


            IMyFaction playerFac = FacUtils.GetPlayersFaction(Context.Player.Identity.IdentityId);
            if (playerFac == null)
            {
                Context.Respond("You dont have a faction.");
                return;
            }
            if (reason != "")
            {
                reason = Context.RawArgs;
            }
            MyFaction fac = MySession.Static.Factions.GetPlayerFaction(Context.Player.IdentityId);
            if (fac == null)
            {
                Context.Respond("A faction is required to use alliance features.");
                return;

            }

            if (distressCooldowns.TryGetValue(Context.Player.IdentityId, out DateTime time))
            {
                if (DateTime.Now < time)
                {
                    Context.Respond(AllianceCommands.GetCooldownMessage(time));
                    return;
                }
                else
                {
                    distressCooldowns[Context.Player.IdentityId] = DateTime.Now.AddSeconds(30);
                }
            }
            else
            {
                distressCooldowns.Add(Context.Player.IdentityId, DateTime.Now.AddSeconds(30));

            }
            Alliance alliance = AlliancePlugin.GetAllianceNoLoading(fac);
            if (alliance != null)
            {
                if (distressAmounts.ContainsKey(Context.Player.IdentityId)) {
                    distressAmounts[Context.Player.IdentityId] += 1;
                    AllianceChat.SendChatMessage(alliance.AllianceId, "Distress Signal", CreateGps(Context.Player.Character.GetPosition(), Color.Yellow, 600, Context.Player.Character.DisplayName + " " + distressAmounts[Context.Player.IdentityId], reason).ToString(), true, 0);
                }
                else {
                    AllianceChat.SendChatMessage(alliance.AllianceId, "Distress Signal", CreateGps(Context.Player.Character.GetPosition(), Color.Yellow, 600, Context.Player.Character.DisplayName, reason).ToString(), true, 0);
                    distressAmounts.Add(Context.Player.IdentityId, 1);
                }
          
            }
        }
        FileUtils utils = new FileUtils();
        [Command("al chat", "toggle alliance chat")]
        [Permission(MyPromoteLevel.None)]
        public void DoAllianceChat(string message = "")
        {
            MyFaction fac = MySession.Static.Factions.GetPlayerFaction(Context.Player.IdentityId);
            if (fac == null)
            {
                Context.Respond("Only factions can be in alliances.");
                return;
            }
            PlayerData data;
            if (File.Exists(AlliancePlugin.path + "//PlayerData//" + Context.Player.SteamUserId + ".xml"))
            {

                data = utils.ReadFromXmlFile<PlayerData>(AlliancePlugin.path + "//PlayerData//" + Context.Player.SteamUserId + ".xml");
            }
            else
            {
                data = new PlayerData();
            }
            Alliance alliance = AlliancePlugin.GetAlliance(fac);
            if (AllianceChat.PeopleInAllianceChat.ContainsKey(Context.Player.SteamUserId))
            {
                data.InAllianceChat = false;
                AllianceChat.PeopleInAllianceChat.Remove(Context.Player.SteamUserId);
                Context.Respond("Leaving alliance chat.", Color.Red);
                utils.WriteToXmlFile<PlayerData>(AlliancePlugin.path + "//PlayerData//" + Context.Player.SteamUserId + ".xml", data);
                return;
            }
            if (alliance != null)
            {
                {
                    data.InAllianceChat = true;
                    AllianceChat.PeopleInAllianceChat.Add(Context.Player.SteamUserId, alliance.AllianceId);
                    Context.Respond("Entering alliance chat.", Color.Cyan);
                    utils.WriteToXmlFile<PlayerData>(AlliancePlugin.path + "//PlayerData//" + Context.Player.SteamUserId + ".xml", data);
                }
            }
            else
            {
                Context.Respond("You must be in an alliance to use alliance chat.");
            }
        }
        [Command("tags", "output tags")]
        [Permission(MyPromoteLevel.None)]
        public void DoTags()
        {

            Dictionary<String, String> tagsAndNames = new Dictionary<string, string>();
            Dictionary<String, String> friends = new Dictionary<string, string>();
            Dictionary<String, String> neutrals = new Dictionary<string, string>();
            Dictionary<String, String> alliances = new Dictionary<String, string>();

            foreach (MyPlayer player in MySession.Static.Players.GetOnlinePlayers())
            {
                string name = MyMultiplayer.Static.GetMemberName(player.Id.SteamId);
                MyIdentity identity = AlliancePlugin.GetIdentityByNameOrId(player.Id.SteamId.ToString());
                if (FacUtils.GetPlayersFaction(player.Identity.IdentityId) != null)
                {
                        IMyFaction playerFac = null;
                        if (FacUtils.GetPlayersFaction(Context.Player.Identity.IdentityId) != null)
                        {

                            playerFac = FacUtils.GetPlayersFaction(Context.Player.Identity.IdentityId);
                        }
                        if (playerFac == null)
                        {
                            Context.Respond("Make a faction. This command does not work without being in a faction.");
                            return;
                        }
                        if (FacUtils.GetPlayersFaction(player.Identity.IdentityId) == null)
                        {
                            continue;
                        }
                        Alliance alliance = AlliancePlugin.GetAllianceNoLoading(FacUtils.GetPlayersFaction(player.Identity.IdentityId) as MyFaction);
                        if (alliance != null)
                        {
                            if (alliances.ContainsKey(alliance.name))
                            {
                                alliances.TryGetValue(alliance.name, out String temp);


                                if (FacUtils.GetPlayersFaction(player.Identity.IdentityId).IsFounder(player.Identity.IdentityId))
                                {
                                    temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName + " (Founder)";
                                }
                                else
                                {
                                    if (FacUtils.GetPlayersFaction(player.Identity.IdentityId).IsLeader(player.Identity.IdentityId))
                                    {
                                        temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName + " (Leader)";
                                    }

                                    else
                                    {
                                        temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName;
                                    }
                                }
                                alliances.Remove(alliance.name);
                                alliances.Add(alliance.name, temp);
                            }
                            else
                            {
                                String temp = "";
                                if (FacUtils.GetPlayersFaction(player.Identity.IdentityId).IsFounder(player.Identity.IdentityId))
                                {
                                    temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName + " (Founder)";
                                }
                                else
                                {
                                    if (FacUtils.GetPlayersFaction(player.Identity.IdentityId).IsLeader(player.Identity.IdentityId))
                                    {
                                        temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName + " (Leader)";
                                    }

                                    else
                                    {
                                        temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName;
                                    }
                                }

                               alliances.Add(alliance.name, temp);
                            }
                        }
                        else
                        {
                            if (friends.ContainsKey(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag))
                            {
                                friends.TryGetValue(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag, out String temp);


                                if (FacUtils.GetPlayersFaction(player.Identity.IdentityId).IsFounder(player.Identity.IdentityId))
                                {
                                    temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName + " (Founder)";
                                }
                                else
                                {
                                    if (FacUtils.GetPlayersFaction(player.Identity.IdentityId).IsLeader(player.Identity.IdentityId))
                                    {
                                        temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName + " (Leader)";
                                    }

                                    else
                                    {
                                        temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName;
                                    }
                                }
                                friends.Remove(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag);
                                friends.Add(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag, temp);
                            }
                            if (MySession.Static.Factions.AreFactionsEnemies(playerFac.FactionId, FacUtils.GetPlayersFaction(player.Identity.IdentityId).FactionId))
                            {
                                if (tagsAndNames.ContainsKey(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag))
                                {
                                    tagsAndNames.TryGetValue(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag, out String temp);


                                    if (FacUtils.GetPlayersFaction(player.Identity.IdentityId).IsFounder(player.Identity.IdentityId))
                                    {
                                        temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName + " (Founder)";
                                    }
                                    else
                                    {
                                        if (FacUtils.GetPlayersFaction(player.Identity.IdentityId).IsLeader(player.Identity.IdentityId))
                                        {
                                            temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName + " (Leader)";
                                        }

                                        else
                                        {
                                            temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName;
                                        }
                                    }
                                    tagsAndNames.Remove(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag);
                                    tagsAndNames.Add(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag, temp);
                                }
                                else
                                {
                                    String temp = "";
                                    if (FacUtils.GetPlayersFaction(player.Identity.IdentityId).IsFounder(player.Identity.IdentityId))
                                    {
                                        temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName + " (Founder)";
                                    }
                                    else
                                    {
                                        if (FacUtils.GetPlayersFaction(player.Identity.IdentityId).IsLeader(player.Identity.IdentityId))
                                        {
                                            temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName + " (Leader)";
                                        }

                                        else
                                        {
                                            temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName;
                                        }
                                    }

                                    tagsAndNames.Add(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag, temp);
                                }
                            }
                            else
                            {
                                if (neutrals.ContainsKey(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag))
                                {
                                    neutrals.TryGetValue(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag, out String temp);


                                    if (FacUtils.GetPlayersFaction(player.Identity.IdentityId).IsFounder(player.Identity.IdentityId))
                                    {
                                        temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName + " (Founder)";
                                    }
                                    else
                                    {
                                        if (FacUtils.GetPlayersFaction(player.Identity.IdentityId).IsLeader(player.Identity.IdentityId))
                                        {
                                            temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName + " (Leader)";
                                        }

                                        else
                                        {
                                            temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName;
                                        }
                                    }
                                    neutrals.Remove(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag);
                                    neutrals.Add(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag, temp);
                                }
                                else
                                {
                                    String temp = "";
                                    if (FacUtils.GetPlayersFaction(player.Identity.IdentityId).IsFounder(player.Identity.IdentityId))
                                    {
                                        temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName + " (Founder)";
                                    }
                                    else
                                    {
                                        if (FacUtils.GetPlayersFaction(player.Identity.IdentityId).IsLeader(player.Identity.IdentityId))
                                        {
                                            temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName + " (Leader)";
                                        }

                                        else
                                        {
                                            temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName;
                                        }
                                    }

                                    neutrals.Add(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag, temp);
                                }
                            }
                        }
                    
                }


            }
            var sb = new StringBuilder();
           
            foreach (KeyValuePair<String, String> keys in alliances)
            {
                sb.Append("\n " + keys.Key);
                sb.Append(keys.Value);

            }
            sb.Append("\n At War");
            foreach (KeyValuePair<String, String> keys in tagsAndNames)
            {

                sb.Append(keys.Value);

            }
            sb.Append("\n ");
            sb.Append("\n Friends");
            foreach (KeyValuePair<String, String> keys in friends)
            {

                sb.Append(keys.Value);

            }
            sb.Append("\n ");
            sb.Append("\n Neutral");
            foreach (KeyValuePair<String, String> keys in neutrals)
            {

                sb.Append(keys.Value);

            }
 
                DialogMessage m = new DialogMessage("Tags of online players", "", sb.ToString());
                ModCommunication.SendMessageTo(m, Context.Player.SteamUserId);

    

        }
        private MyGps CreateGps(Vector3D Position, Color gpsColor, int seconds, String Nation, String Reason)
        {

            MyGps gps = new MyGps
            {
                Coords = Position,
                Name = Nation + " - Distress Signal ",
                DisplayName = Nation + " - Distress Signal ",
                GPSColor = gpsColor,
                IsContainerGPS = true,
                ShowOnHud = true,
                DiscardAt = new TimeSpan(0, 0, seconds, 0),
                Description = "Nation Distress Signal \n" + Reason,
            };
            gps.UpdateHash();


            return gps;
        }
    }
   

}
