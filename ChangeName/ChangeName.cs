using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace ChangeName
{
    [ApiVersion(2, 1)]
    public class ChangeName : TerrariaPlugin
    {
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        public override string Name => "ChangeName";
        public override string Author => "Simon311";
        public override string Description => "Changing names";

        private readonly Dictionary<string, string> oldNames = new();

        public ChangeName(Main game) : base(game)
        {
            Order = -1;
        }

        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command("changenames", ChanName, "chname"));
            Commands.ChatCommands.Add(new Command("oldnames", OldName, "oldname"));
            Commands.ChatCommands.Add(new Command("selfname", SelfName, "selfname"));
            Commands.ChatCommands.Add(new Command("tshock.canchat", Chat, "chat"));
        }

        private void ChanName(CommandArgs args)
        {
            if (args.Player == null || args.Parameters.Count < 2)
            {
                args.Player?.SendErrorMessage("Invalid syntax! Proper syntax: /chname [player] [newname]");
                return;
            }

            var foundPlayer = TSPlayer.FindByNameOrID(args.Parameters[0]);
            if (foundPlayer.Count != 1)
            {
                args.Player?.SendErrorMessage(foundPlayer.Count == 0 ? "Invalid player!" : $"More than one ({foundPlayer.Count}) player matched!");
                return;
            }

            var plr = foundPlayer[0];
            var newName = args.Parameters[1];
            var hidden = args.Parameters.Count > 2;

            string oldName = plr.TPlayer.name;
            if (!hidden)
                TShock.Utils.Broadcast($"{args.Player.Name} has changed {oldName}'s name to {newName}.", Color.DeepPink);
            else
                args.Player.SendMessage($"You have secretly changed {oldName}'s name to {newName}.", Color.DeepPink);

            plr.TPlayer.name = newName;
            oldNames[newName] = oldName;
        }

        private void SelfName(CommandArgs args)
        {
            if (args.Player == null || args.Parameters.Count < 1)
            {
                args.Player?.SendErrorMessage("Invalid syntax! Proper syntax: /selfname [newname]");
                return;
            }

            var plr = args.Player;
            var newName = string.Join(" ", args.Parameters).Trim();

            if (newName.Length < 2)
            {
                plr.SendMessage("A name must be at least 2 characters long.", Color.DeepPink);
                return;
            }

            if (newName.Length > 20)
            {
                plr.SendMessage("A name must not be longer than 20 characters.", Color.DeepPink);
                return;
            }

            if (TShock.Players.Any(player => player != null && player.Name == newName))
            {
                plr.SendMessage("This name is taken by another player.", Color.DeepPink);
                return;
            }

            string oldName = plr.TPlayer.name;
            plr.TPlayer.name = newName;
            oldNames[newName] = oldName;
            TShock.Utils.Broadcast($"{oldName} has changed his name to {newName}.", Color.DeepPink);
        }

        private void OldName(CommandArgs args)
        {
            if (args.Player == null || args.Parameters.Count < 1)
            {
                args.Player?.SendErrorMessage("Invalid syntax! Proper syntax: /oldname [player]");
                return;
            }

            var name = string.Join(" ", args.Parameters);
            if (oldNames.TryGetValue(name, out string oldName))
                args.Player.SendMessage($"{name}'s old name is {oldName}.", Color.DeepPink);
            else
                args.Player.SendMessage($"{name}'s name has not been changed.", Color.DeepPink);
        }

        private void Chat(CommandArgs args)
        {
            if (args.Player == null || args.Parameters.Count < 1)
            {
                args.Player?.SendErrorMessage("Invalid syntax! Proper syntax: /chat [message]");
                return;
            }

            var text = string.Join(" ", args.Parameters);
            var tsplr = args.Player;
            if (!tsplr.mute)
            {
                var chatFormat = string.Format(TShock.Config.Settings.ChatFormat, tsplr.Group.Name, tsplr.Group.Prefix, tsplr.Name, tsplr.Group.Suffix, text);
                TShock.Utils.Broadcast(chatFormat, tsplr.Group.R, tsplr.Group.G, tsplr.Group.B);
            }
            else
            {
                tsplr.SendErrorMessage("You are muted!");
            }
        }
    }
}
