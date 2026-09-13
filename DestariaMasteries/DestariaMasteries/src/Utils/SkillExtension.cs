using MasteryLibrary.src.Core.Masteries.Data;
using MasteryLibrary.src.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DestariaMasteries.src.Utils
{
    public static class SkillExtensions
    {
        public static object[] GetScaledPercentArgs(this Skill skill, int level, params string[] statKeys)
        {
            var stats = skill.GetValueSafe<Dictionary<string, StatConfiguration>>("PassiveStats");
            var args = new object[statKeys.Length];

            for (int i = 0; i < statKeys.Length; i++)
            {
                float val = StatScalingUtil.GetScaledValue(stats?.GetValueOrDefault(statKeys[i]), level);
                args[i] = val * 100f;
            }

            return args;
        }

        public static object[] GetScaledPercentActiveArgs(this Skill skill, int level, params string[] statKeys)
        {
            var stats = skill.GetValueSafe<Dictionary<string, StatConfiguration>>("ActiveStats");
            var args = new object[statKeys.Length];

            for (int i = 0; i < statKeys.Length; i++)
            {
                float val = StatScalingUtil.GetScaledValue(stats?.GetValueOrDefault(statKeys[i]), level);
                args[i] = val * 100f;
            }

            return args;
        }
    }
}
