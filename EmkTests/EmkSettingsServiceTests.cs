using System;
using System.Linq;
using Emk;
using Emk.Services;
using Emk.Views;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmkTests
{
    [TestClass]
    public class EmkSettingsServiceTests
    {
        [TestMethod]
        public void LoadSettings()
        {
            var data = Factory.LoadSettings();

            //SettingsService srv = new SettingsService();
            //var res = srv.LoadSettings();
            Assert.IsTrue(data.Any());
        }

        [TestMethod]
        public void SaveSettings()
        {
            SettingsService srv = new SettingsService();
            var res = srv.LoadSettings();
            srv.SaveSettings(res);
            Assert.IsTrue(true);
        }

        [TestMethod]
        //[Ignore]
        public void OpenForm()
        {
            EmkSettingsForm form = new EmkSettingsForm();
            form.ShowDialog();
            Assert.IsTrue(true);
        }
    }
}
