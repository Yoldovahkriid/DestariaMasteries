using MasteryLibrary.src.Core.Masteries.Data;
using MasteryLibrary.src.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DestariaMasteries.src.Utils
{
    public static class SkillExtensions
    {
        public static object[] GetScaledPercentArgs(this Skill skill, int level, params string[] statKeys)
            => skill.GetScaledPercentArgsInternal("PassiveStats", level, statKeys);

        public static object[] GetScaledPercentActiveArgs(this Skill skill, int level, params string[] statKeys)
            => skill.GetScaledPercentArgsInternal("ActiveStats", level, statKeys);

        private static object[] GetScaledPercentArgsInternal(this Skill skill, string statGroup, int level, string[] statKeys)
        {
            var stats = skill.GetValueSafe<Dictionary<string, StatConfiguration>>(statGroup);

            return statKeys
                .Select(key => (object)(StatScalingUtil.GetScaledValue(stats?.GetValueOrDefault(key), level) * 100f))
                .ToArray();
        }
    }
}
