﻿using SPT_AKI_Profile_Editor.Core.ServerClasses.Hideout;
using SPT_AKI_Profile_Editor.Helpers;

namespace SPT_AKI_Profile_Editor.Core.ProfileClasses
{
    public class CharacterHideoutProduction
    {
        public CharacterHideoutProduction(HideoutProduction production, bool added)
        {
            Production = production;
            Added = added;
        }

        public HideoutProduction Production { get; set; }

        public bool Added { get; set; }

        public string ProductLocalizedName
            => AppData.ServerDatabase.LocalesGlobal.ContainsKey(Production.EndProduct.Name())
            ? AppData.ServerDatabase.LocalesGlobal[Production.EndProduct.Name()]
            : Production.EndProduct;
    }
}