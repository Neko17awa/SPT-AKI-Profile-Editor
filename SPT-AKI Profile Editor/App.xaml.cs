﻿using ControlzEx.Theming;
using MahApps.Metro.Controls.Dialogs;
using SPT_AKI_Profile_Editor.Core;
using SPT_AKI_Profile_Editor.Core.ProfileClasses;
using SPT_AKI_Profile_Editor.Helpers;
using System;
using System.Collections.Generic;
using System.Windows;

namespace SPT_AKI_Profile_Editor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>

    public partial class App : Application
    {
        public static RelayCommand CloseApplication => new(obj => Current.Shutdown());
        public static IDialogCoordinator DialogCoordinator { get; set; }

        public static Worker Worker { get; set; }

        public static void ChangeTheme() => ThemeManager.Current.ChangeTheme(Current, AppData.AppSettings.ColorScheme);

        public static void OpenContainerWindow(object obj)
        {
            if (obj == null || obj is not InventoryItem item)
                return;
            ContainerWindow window = new(item);
            window.Show();
        }

        public static void CloseContainerWindows(List<string> idsList)
        {
            foreach (Window window in Current.Windows)
                if (window is ContainerWindow containerWindow && idsList.Contains(containerWindow.ItemId))
                    containerWindow.Close();
        }

        public static void HandleException(Exception exception)
        {
            string text = $"Exception Message: {exception.Message}. | StackTrace: {exception.StackTrace}";
            Logger.Log(text);
            MessageBox.Show(text, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Application_Startup(object s, StartupEventArgs e) => Current.DispatcherUnhandledException += (sender, args) => HandleException(args.Exception);
    }
}