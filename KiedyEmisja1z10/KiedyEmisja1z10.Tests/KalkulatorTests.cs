using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KiedyEmisja1z10.Tests
{
    [TestClass]
    public class KalkulatorTests
    {
        private Kalkulator? _kalkulator;

        [TestInitialize]
        public void TestInitialize()
        {
            _kalkulator = new Kalkulator();
        }

        [TestMethod]
        public void Test_Seria_129_Odcinek_17()
        {
            Assert.IsNotNull(_kalkulator);

            var wynik = _kalkulator.Oblicz(129, 17);

            Assert.AreEqual<int>(2022, wynik.DataEmisjiNajwczesniejsza.Year);
        }
    }
}
