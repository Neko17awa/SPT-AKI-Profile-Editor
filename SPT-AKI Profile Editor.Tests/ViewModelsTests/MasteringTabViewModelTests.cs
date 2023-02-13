﻿using NUnit.Framework;
using SPT_AKI_Profile_Editor.Core;
using SPT_AKI_Profile_Editor.Tests.Hepers;
using SPT_AKI_Profile_Editor.Views;
using System.Linq;

namespace SPT_AKI_Profile_Editor.Tests.ViewModelsTests
{
    internal class MasteringTabViewModelTests
    {
        [OneTimeSetUp]
        public void Setup() => TestHelpers.LoadDatabase();

        [Test]
        public void CanInitialize()
        {
            MasteringTabViewModel viewModel = new(null, null, null);
            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel.MaxSkillsValue, Is.EqualTo(AppData.ServerDatabase.ServerGlobals.Config.MaxProgressValue));
            Assert.That(viewModel.SetAllPmcSkillsValue, Is.EqualTo(0f));
            Assert.That(viewModel.SetAllScavSkillsValue, Is.EqualTo(0f));
            Assert.That(viewModel.SetAllPmsSkillsCommand, Is.Not.Null);
            Assert.That(viewModel.SetAllScavSkillsCommand, Is.Not.Null);
            Assert.That(viewModel.OpenSettingsCommand, Is.Not.Null);
        }

        [Test]
        public void CantSetAllPmcSkillsValueGreatherThanInServerDatabase()
        {
            MasteringTabViewModel viewModel = new(null, null, null)
            {
                SetAllPmcSkillsValue = float.MaxValue
            };
            Assert.That(viewModel.SetAllPmcSkillsValue, Is.EqualTo(AppData.ServerDatabase.ServerGlobals.Config.MaxProgressValue));
        }

        [Test]
        public void CantSetAllScavSkillsValueGreatherThanInServerDatabase()
        {
            MasteringTabViewModel viewModel = new(null, null, null)
            {
                SetAllScavSkillsValue = float.MaxValue
            };
            Assert.That(viewModel.SetAllScavSkillsValue, Is.EqualTo(AppData.ServerDatabase.ServerGlobals.Config.MaxProgressValue));
        }

        [Test]
        public void CanExecuteSetAllPmsSkillsCommand()
        {
            AppData.AppSettings.AutoAddMissingMasterings = true;
            AppData.Profile.Load(TestHelpers.profileFile);
            MasteringTabViewModel viewModel = new(null, null, null)
            {
                SetAllPmcSkillsValue = 160f
            };
            viewModel.SetAllPmsSkillsCommand.Execute(null);
            Assert.That(AppData.Profile.Characters.Pmc.Skills.Mastering.All(x => x.Progress == 160f), Is.True);
        }

        [Test]
        public void CanExecuteSetAllScavSkillsCommand()
        {
            AppData.AppSettings.AutoAddMissingMasterings = true;
            AppData.Profile.Load(TestHelpers.profileFile);
            MasteringTabViewModel viewModel = new(null, null, null)
            {
                SetAllScavSkillsValue = 160f
            };
            viewModel.SetAllScavSkillsCommand.Execute(null);
            Assert.That(AppData.Profile.Characters.Scav.Skills.Mastering.All(x => x.Progress == 160f), Is.True);
        }

        [Test]
        public void CanExecuteOpenSettingsCommand()
        {
            TestsDialogManager dialogManager = new();
            MasteringTabViewModel viewModel = new(dialogManager, null, null);
            viewModel.OpenSettingsCommand.Execute(null);
            Assert.That(dialogManager.SettingsDialogOpened, Is.True);
        }
    }
}