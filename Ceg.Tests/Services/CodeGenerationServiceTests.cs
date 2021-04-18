using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Ceg.Services.Tests
{
    [TestClass]
    public class CodeGenerationServiceTests
    {
        private readonly CodeGenerationService _service = new CodeGenerationService();

        [TestMethod]
        public void NormalizeLabelTest_DifferentPatterns()
        {
            Assert.AreEqual("Account", _service.NormalizeLabel("Account", "fallback"), false);
            Assert.AreEqual("ReConnect", _service.NormalizeLabel("ReConnect", "fallback"), false);
            Assert.AreEqual("ClientBroker", _service.NormalizeLabel("Client Broker", "fallback"), false);
            Assert.AreEqual("ClientBroker", _service.NormalizeLabel("Client/Broker", "fallback"), false);
            Assert.AreEqual("ClientOrBroker", _service.NormalizeLabel("Client or Broker", "fallback"), false);
            Assert.AreEqual("Doctors", _service.NormalizeLabel("Doctor's", "fallback"), false);
            Assert.AreEqual("IntegrationFailedCreate", _service.NormalizeLabel("Integration Failed (create)", "fallback"), false);
            Assert.AreEqual("SemiAnnual", _service.NormalizeLabel("SemiAnnual", "fallback"), false);
            Assert.AreEqual("SemiAnnual", _service.NormalizeLabel("Semi-Annual", "fallback"), false);
            Assert.AreEqual("SemiAnnual", _service.NormalizeLabel("Semi---Annual", "fallback"), false);
            Assert.AreEqual("SemiAnnual", _service.NormalizeLabel("Semi - Annual", "fallback"), false);
            Assert.AreEqual("AMinus", _service.NormalizeLabel("A-", "fallback"), false);
            Assert.AreEqual("AaMinusMinus", _service.NormalizeLabel("Aa--", "fallback"), false);
            Assert.AreEqual("BBPlusPlusPlus", _service.NormalizeLabel("B-B+++", "fallback"), false);
            Assert.AreEqual("_2_10_Net_30", _service.NormalizeLabel("2% 10, Net 30", "fallback"), false);
            Assert.AreEqual("Address_1_AddressType", _service.NormalizeLabel("Address 1: Address Type", "fallback"), false);
            Assert.AreEqual("Address_20_Street", _service.NormalizeLabel("Address: 20 street", "fallback"), false);
            Assert.AreEqual("Foo_5_10_20_Bar", _service.NormalizeLabel("Foo 5 10 20 Bar", "fallback"), false);
            Assert.AreEqual("_DEPRECATED_2019_R05_SunriseTimeUnits", _service.NormalizeLabel("[DEPRECATED - 2019.R05] Sunrise Time Units", "fallback"), false);
            Assert.AreEqual("_DEPRECATED_SunriseTimeUnits", _service.NormalizeLabel("[DEPRECATED] Sunrise Time Units", "fallback"), false);
        }

        [TestMethod]
        public void NormalizeLabelTest_InvalidFallback()
        {
            Assert.ThrowsException<ArgumentNullException>(() => _service.NormalizeLabel(null, null));
            Assert.ThrowsException<ArgumentNullException>(() => _service.NormalizeLabel(null, string.Empty));
            Assert.ThrowsException<ArgumentException>(() => _service.NormalizeLabel(null, "5bears"));
            Assert.ThrowsException<ArgumentException>(() => _service.NormalizeLabel(null, "Three bears"));
            Assert.ThrowsException<ArgumentException>(() => _service.NormalizeLabel(null, "ThreeBears&FiveWolves"));
        }
    }
}