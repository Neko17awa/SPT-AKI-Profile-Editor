﻿using Newtonsoft.Json;
using SPT_AKI_Profile_Editor.Core.HelperClasses;
using System;
using System.Linq;

namespace SPT_AKI_Profile_Editor.Core.ProfileClasses
{
    public class HideoutArea : BindableEntity
    {
        private int type;
        private int level;

        [JsonConstructor]
        public HideoutArea(int type, int level)
        {
            this.type = type;
            this.level = level;
        }

        [JsonProperty("type")]
        public int Type
        {
            get => type;
            set
            {
                type = value;
                OnPropertyChanged(nameof(Type));
            }
        }

        [JsonProperty("level")]
        public int Level
        {
            get => level;
            set
            {
                level = Math.Min(value, MaxLevel);
                OnPropertyChanged(nameof(Level));
                var areaInfo = AppData.ServerDatabase.HideoutAreaInfos.FirstOrDefault(x => x.Type == type);
                if (!string.IsNullOrEmpty(areaInfo?.Id))
                {
                    var childAreaType = AppData.ServerDatabase.HideoutAreaInfos.FirstOrDefault(x => x.ParentArea == areaInfo.Id)?.Type;
                    if (childAreaType != null)
                    {
                        var childArea = AppData.Profile.Characters?.Pmc?.Hideout?.Areas.FirstOrDefault(x => x.Type == childAreaType);
                        if (childArea != null)
                            childArea.Level = level;
                    }
                }
            }
        }

        [JsonIgnore]
        public string LocalizedName => AppData.ServerDatabase.LocalesGlobal.ContainsKey($"hideout_area_{Type}_name") ?
            AppData.ServerDatabase.LocalesGlobal[$"hideout_area_{Type}_name"] :
            $"hideout_area_{Type}_name";

        [JsonIgnore]
        public int MaxLevel => GetMaxLevel();

        private int GetMaxLevel()
        {
            var areaInfo = AppData.ServerDatabase?.HideoutAreaInfos?.Where(x => x.Type == Type).FirstOrDefault();
            return areaInfo != null ? areaInfo.Stages.Count - 1 : 0;
        }
    }
}