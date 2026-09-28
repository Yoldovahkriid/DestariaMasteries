using MasteryLibrary.src.Core.Abilities;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace DestariaMasteries.src.Mechanist
{
    public class StormChaser : Ability
    {
        public override string Code => "StormChaser";

        public override AbilityResult Execute(AbilityContext context)
        {
            var api = context.API;
            var player = context.Player;

            var tempStabSys = api.ModLoader.GetModSystem<SystemTemporalStability>();
            if (tempStabSys == null)
            {
                return AbilityResult.FailureResult("Temporal stability system not found.");
            }

            var data = tempStabSys.StormData;
            string message;

            if (data.nowStormActive)
            {
                double daysLeft = data.stormActiveTotalDays - api.World.Calendar.TotalDays;
                message = Lang.Get(data.nextStormStrength + " Storm still active for {0:0.##} days", daysLeft);
            }
            else
            {
                double nextStormDaysLeft = data.nextStormTotalDays - api.World.Calendar.TotalDays;
                message = Lang.Get("temporalstorm-cmd-daysleft", nextStormDaysLeft);
            }

            player.SendMessage(GlobalConstants.GeneralChatGroup, message, EnumChatType.Notification);

            return AbilityResult.SuccessResult(message);
        }
    }
}
