namespace RandoopExtension.ToTestFrameworkConverter
{
    class NUnit : TestFrameworkAttributes
    {
        public string ClassAttribute => "[TestFixture]";

        public string TestAttribute => "[Test]";

        public string ExpectedExceptionAttribute(string exception) => "[ExpectedException(typeof(" + exception + "))]";
    }
}
