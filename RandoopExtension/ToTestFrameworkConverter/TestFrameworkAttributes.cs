namespace RandoopExtension.ToTestFrameworkConverter
{
    public interface TestFrameworkAttributes
    {
        string ClassAttribute { get; }
        string TestAttribute { get; }
        string ExpectedExceptionAttribute(string exception);

    }
}
