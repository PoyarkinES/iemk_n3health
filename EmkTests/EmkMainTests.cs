using System;
using Emk.Services;
using Emk.Views;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmkTests
{
    [TestClass]
    public class EmkMainTests
    {
        [TestMethod]
        public void RunSending()
        {
            TimeSpan.TryParse("21:00", out var t);
            EmkSendingService e = new EmkSendingService();
            e.Run();
        }

        [TestMethod]
        public void Run()
        {
            EmkMainForm e = new EmkMainForm();
            e.ShowDialog();
        }
        
    }
}
