﻿using Newtonsoft.Json;
using SPT_AKI_Profile_Editor.Core.Enums;
using SPT_AKI_Profile_Editor.Core.HelperClasses;

namespace SPT_AKI_Profile_Editor.Core.ProfileClasses
{
    public class ProfileCharacters : BindableEntity
    {
        private Character pmc;

        private Character scav;

        [JsonProperty("pmc")]
        public Character Pmc
        {
            get => pmc;
            set
            {
                pmc = value;
                OnPropertyChanged("Pmc");
            }
        }

        [JsonProperty("scav")]
        public Character Scav
        {
            get => scav;
            set
            {
                scav = value;
                OnPropertyChanged("Scav");
            }
        }

        public CharacterInventory GetInventory(StashEditMode editMode) => editMode switch
        {
            StashEditMode.Scav => Scav?.Inventory,
            _ => Pmc?.Inventory,
        };
    }
}