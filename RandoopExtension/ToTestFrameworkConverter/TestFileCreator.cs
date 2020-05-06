using System;
using System.Collections.Generic;
using System.IO;

namespace RandoopExtension.ToTestFrameworkConverter
{
    //convert .cs test files to format of MSTest file
    public class TestFileCreator
    {
        private readonly Dictionary<TestFrameworkEnum, TestFrameworkAttributes> testFrameworkAttributes = new Dictionary<TestFrameworkEnum, TestFrameworkAttributes>
            {
                { TestFrameworkEnum.MSTest, new MSTest() },
                { TestFrameworkEnum.NUnit, new NUnit() }
            };

        public void Convert(string path, TestFrameworkEnum chosenFrameworkName)
        {
            var testFrameworkAttributes = this.testFrameworkAttributes[chosenFrameworkName];


            var sw = new StreamWriter(path + "\\temp.cs");
            var sw_1 = new StreamWriter(path + "\\temp2.cs");

            List<string> namespaces = CreateNamespaces();

            sw.WriteLine("\n" + testFrameworkAttributes.ClassAttribute);
            sw.WriteLine("public class RandoopTest");
            sw.WriteLine("{");

            sw_1.WriteLine("public class RandoopTest");
            sw_1.WriteLine("{");

            int numTest = 0;
            List<string> files = FileHelper.GetFiles(path);

            foreach (string testfile in files)
            {
                if (FileIsNotRandoopGenerated(testfile))
                    continue;

                if (FileContainsFaultyTestCases(path, testfile))
                    continue;

                //determine the type of test: normal, exceptions, or ...
                string testfileRelativePath = testfile.Substring(path.Length + 1);
                string testType = testfileRelativePath.Substring(0, testfileRelativePath.LastIndexOf("\\"));

                numTest++;
                var testFileReader = new StreamReader(testfile);

                string line;
                bool isStart = false;
                while ((line = testFileReader.ReadLine()) != null)
                {
                    if (IsUsingDirective(line))
                    {
                        line = line.Trim();
                        string namespaceName = GetNamespaceName(line);
                        if (namespaces.Contains(namespaceName) == false)
                        {
                            namespaces.Add(namespaceName);
                        }
                        else
                        {
                            continue; //this namespace has been added
                        }
                    }

                    //if (line.Contains("public static int Main"))
                    if (line.Contains("BEGIN TEST"))
                    {
                        //specify the type of test: normal, exceptions, or ...
                        sw.WriteLine("\r\n\t// Test Case Type: " + testType);
                        sw.WriteLine("\t" + testFrameworkAttributes.TestAttribute);
                        if (TestRaisesAnException(testType))
                        {
                            sw.WriteLine("\t" + testFrameworkAttributes.ExpectedExceptionAttribute(testType));
                        }

                        sw.WriteLine("\tpublic void TestMethod" + numTest.ToString() + "()");
                        sw.WriteLine("\t{");
                        isStart = true;

                        sw_1.WriteLine("\tpublic void TestMethod" + numTest.ToString() + "()");
                        sw_1.WriteLine("\t{");
                        sw_1.WriteLine("\t try{");
                    }

                    if (line.StartsWith("//") && !line.Contains("Regression assertion")) //lines with space before "//" are not counted
                        continue;

                    //if (line.Contains("END TEST"))
                    if (line.Trim().StartsWith("/*")) //skip the internal output of Randoop about test plans [11/06/2012]
                    {
                        //sw.WriteLine(line);
                        sw.WriteLine("      //END TEST");
                        sw.WriteLine("      return;");
                        sw.WriteLine("\t}\r\n");

                        //sw_1.WriteLine(line);
                        sw_1.WriteLine("      //END TEST");
                        sw_1.WriteLine("      return;");
                        sw_1.WriteLine("\t }");
                        sw_1.WriteLine("\t catch (System.Exception e)");
                        sw_1.WriteLine("\t {");
                        sw_1.WriteLine("\t  return;");
                        sw_1.WriteLine("\t }");
                        sw_1.WriteLine("\t}\r\n");

                        break;
                    }

                    if (isStart)
                    {
                        sw.WriteLine(line);
                        sw_1.WriteLine(line);
                    }
                }

                testFileReader.Close();

            }

            sw.WriteLine("}");
            sw_1.WriteLine("}");

            sw.Close();
            sw_1.Close();

            var sw2 = new StreamWriter(path + "\\RandoopTest.cs");
            var sr2 = new StreamReader(path + "\\temp.cs");

            foreach (string name in namespaces)
            {
                sw2.WriteLine("using " + name + ";");
            }

            sw2.WriteLine("");
            string line2;
            while ((line2 = sr2.ReadLine()) != null)
            {
                sw2.WriteLine(line2);

            }
            sr2.Close();
            sw2.Close();
        }

        private static bool TestRaisesAnException(string testType)
        {
            return testType.ToLower().Contains("exception");
        }

        private static bool IsUsingDirective(string line)
        {
            return line.Contains("using ");
        }

        private static string GetNamespaceName(string line)
        {
            var namespaceNameLength = line.Length - ("using ".Length + ";".Length);
            return line.Substring("using ".Length, namespaceNameLength).Trim();
        }

        private static bool FileContainsFaultyTestCases(string path, string testfile)
        {
            return testfile.Substring(path.Length + 1).Contains("temp");
        }

        private static bool FileIsNotRandoopGenerated(string testfile)
        {
            return (testfile.Contains("RandoopTest") && testfile.EndsWith(".cs")) || testfile.Contains("RandoopTest.cs") == false;
        }

        private static List<string> CreateNamespaces()
        {
            List<string> namespaces = new List<string>();
            namespaces.Add("System");
            namespaces.Add("System.Text");
            namespaces.Add("System.Collections.Generic");
            namespaces.Add("System.Linq");
            namespaces.Add("Microsoft.VisualStudio.TestTools.UnitTesting");
            return namespaces;
        }
    }

    static public class FileHelper
    {
        public static List<string> GetFiles(string root)
        {
            List<string> result = new List<string>();
            Stack<string> stack = new Stack<string>();

            stack.Push(root);

            while (stack.Count > 0)
            {
                string dir = stack.Pop();

                try
                {
                    result.AddRange(Directory.GetFiles(dir, "*.*"));

                    foreach (string childDir in Directory.GetDirectories(dir))
                    {
                        stack.Push(childDir);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return result;
        }
    }
}
