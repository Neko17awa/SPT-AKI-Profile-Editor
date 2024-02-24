﻿namespace SPT_AKI_Profile_Editor.Core.HelperClasses
{
    public class GridFilters : BindableEntity
    {
        private SkillsTab skillsTab;

        private MasteringTab masteringTab;

        private string clothingNameFilter;

        private StashTab stashTab;

        private CleaningFromModsTab cleaningFromModsTab;

        public GridFilters()
        {
            SkillsTab = new();
            MasteringTab = new();
            StashTab = new();
            CleaningFromModsTab = new();
        }

        public SkillsTab SkillsTab
        {
            get => skillsTab;
            set
            {
                skillsTab = value;
                OnPropertyChanged("SkillsTab");
            }
        }

        public MasteringTab MasteringTab
        {
            get => masteringTab;
            set
            {
                masteringTab = value;
                OnPropertyChanged("MasteringTab");
            }
        }

        public string ClothingNameFilter
        {
            get => clothingNameFilter;
            set
            {
                clothingNameFilter = value;
                OnPropertyChanged("ClothingNameFilter");
            }
        }

        public StashTab StashTab
        {
            get => stashTab;
            set
            {
                stashTab = value;
                OnPropertyChanged("StashTab");
            }
        }

        public CleaningFromModsTab CleaningFromModsTab
        {
            get => cleaningFromModsTab;
            set
            {
                cleaningFromModsTab = value;
                OnPropertyChanged("CleaningFromModsTab");
            }
        }

        public void Clear()
        {
            SkillsTab = new();
            MasteringTab = new();
            StashTab = new();
            ClothingNameFilter = string.Empty;
            CleaningFromModsTab = new();
        }
    }
}