namespace RandoopExtension.ToTestFrameworkConverter
{
    class MSTest : TestFrameworkAttributes
    {
        public string ClassAttribute => "[TestClass]";

        public string TestAttribute => "[TestMethod]";

        public string ExpectedExceptionAttribute(string exception) => "[ExpectedException(typeof(" + exception + "))]";
    }
}
