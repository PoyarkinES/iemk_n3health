using Emk.Domain.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmkTests.Domain
{
    [TestClass]
    public class EnumsTests
    {
        [TestMethod]
        public void SendingType_Values()
        {
            Assert.AreEqual(0, (int)SendingType.DaysBeforeNow);
            Assert.AreEqual(1, (int)SendingType.Interval);
        }

        [TestMethod]
        public void InternalDocType_Values()
        {
            Assert.AreEqual(198, (int)InternalDocType.DocConsultNote);
            Assert.AreEqual(-1, (int)InternalDocType.PdfConsultNote);
            Assert.AreEqual(2, (int)InternalDocType.DischargeSummary);
        }

        [TestMethod]
        public void PayType_Values()
        {
            Assert.AreEqual(0, (int)PayType.Unknown);
            Assert.AreEqual(1, (int)PayType.OMS);
            Assert.AreEqual(6, (int)PayType.Another);
        }

        [TestMethod]
        public void ServiceEnum_ConsentCode()
        {
            Assert.AreEqual(5752, (int)ServiceEnum.ConsentTransfPersData);
        }
    }
}
