﻿using SPT_AKI_Profile_Editor.Core.ServerClasses;
using SPT_AKI_Profile_Editor.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace SPT_AKI_Profile_Editor.Views
{
    public class AllItemsDialogViewModel : ClosableDialogViewModel
    {
        public AllItemsDialogViewModel(RelayCommand addCommand, object context) : base(context)
        {
            AddCommand = addCommand;
        }

        public static IEnumerable<AddableItem> AddableItems
            => ServerDatabase?.ItemsDB?.Values
            .Where(x => x.Properties.StackMaxSize > 0)
            .Select(x => TarkovItem.CopyFrom(x));

        public RelayCommand AddCommand { get; set; }
    }
}