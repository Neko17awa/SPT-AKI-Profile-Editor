﻿using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SPT_AKI_Profile_Editor.Core.Enums
{
    public enum QuestType
    {
        [EnumMember(Value = "Standart")]
        Standart,

        [EnumMember(Value = "Weekly")]
        Weekly,

        [EnumMember(Value = "Daily")]
        Daily,

        [EnumMember(Value = "Daily_Savage")]
        Daily_Savage,

        [EnumMember(Value = "Unknown")]
        Unknown
    }

    public static class QuestTypeExtension
    {
        public static List<QuestStatus> GetAvailableStatuses(this QuestType type) => type switch
        {
            QuestType.Standart => AppData.AppSettings.standartQuestStatuses,
            _ => AppData.AppSettings.repeatableQuestStatuses,
        };

        public static string LocalizedName(this QuestType type) => type switch
        {
            QuestType.Daily => AppData.AppLocalization.GetLocalizedString("tab_quests_daily_group"),
            QuestType.Weekly => AppData.AppLocalization.GetLocalizedString("tab_quests_weekly_group"),
            QuestType.Daily_Savage => AppData.AppLocalization.GetLocalizedString("tab_quests_daily_savage_group"),
            QuestType.Unknown => AppData.AppLocalization.GetLocalizedString("tab_quests_unknown_group"),
            _ => AppData.AppLocalization.GetLocalizedString("tab_quests_standart_group")
        };
    }
}