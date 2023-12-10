﻿using Newtonsoft.Json;
using NUnit.Framework;
using SPT_AKI_Profile_Editor.Core;
using SPT_AKI_Profile_Editor.Tests.Hepers;
using System;
using System.IO;
using System.Linq;

namespace SPT_AKI_Profile_Editor.Tests
{
    internal class BackupServiceTests
    {
        private BackupService backupService;

        [OneTimeSetUp]
        public void Setup() => backupService = new(Path.Combine(TestHelpers.appDataPath, "Backups"));

        [Test]
        public void BackupServiceIsNotNull() => Assert.That(backupService, Is.Not.Null);

        [Test]
        public void BackupCreatingCorrectly()
        {
            var expected = JsonConvert.DeserializeObject(File.ReadAllText(TestHelpers.profileFile));
            backupService.CreateBackup(TestHelpers.profileFile);
            backupService.LoadBackupsList(Path.GetFileNameWithoutExtension(TestHelpers.profileFile));
            var result = JsonConvert.DeserializeObject(File.ReadAllText(backupService.BackupList.First().Path));
            Assert.That(result.ToString(), Is.EqualTo(expected.ToString()));
            Assert.That(backupService.HasBackups, Is.True);
        }

        [Test]
        public void BackupListNotEmpty()
        {
            backupService.LoadBackupsList(Path.GetFileNameWithoutExtension(TestHelpers.profileFile));
            Assert.That(backupService.BackupList.Any(), Is.True);
            Assert.That(backupService.HasBackups, Is.True);
        }

        [Test]
        public void BackupRestoringCorrectly()
        {
            backupService.CreateBackup(TestHelpers.profileFile);
            backupService.LoadBackupsList(Path.GetFileNameWithoutExtension(TestHelpers.profileFile));
            var expected = JsonConvert.DeserializeObject(File.ReadAllText(backupService.BackupList.First().Path));
            string testPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "testBackup.json");
            BackupService.RestoreBackup(backupService.BackupList.First().Path, testPath);
            var result = JsonConvert.DeserializeObject(File.ReadAllText(testPath));
            Assert.That(result.ToString(), Is.EqualTo(expected.ToString()));
        }

        [Test]
        public void BackupRemovingCorrectly()
        {
            backupService.LoadBackupsList(Path.GetFileNameWithoutExtension(TestHelpers.profileFile));
            var expected = backupService.BackupList.Count();
            backupService.RemoveBackup(backupService.BackupList.Last().Path);
            backupService.LoadBackupsList(Path.GetFileNameWithoutExtension(TestHelpers.profileFile));
            Assert.That(backupService.BackupList.Count(), Is.Not.EqualTo(expected));
            Assert.That(backupService.HasBackups, Is.False);
        }
    }
}