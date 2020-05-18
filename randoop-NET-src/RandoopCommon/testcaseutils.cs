//*********************************************************
//
//    Copyright (c) Microsoft. All rights reserved.
//    This code is licensed under the Apache License, Version 2.0.
//    THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//    ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//    IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//    PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************



using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Common
{
    /// <summary>
    /// A collection of static methods that perform operations
    /// on TestCases.
    /// </summary>
    public static class TestCaseUtils
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static Collection<FileInfo> CollectFilesEndingWith(string endString, params string[] fileNames)
        {
            List<FileInfo> retval = new List<FileInfo>();
            foreach (string fileName in fileNames)
            {
                if (Directory.Exists(fileName))
                {
                    retval.AddRange(CollectFilesEndingWith(endString, new DirectoryInfo(fileName)));
                }
                else if (File.Exists(fileName) && fileName.EndsWith(endString))
                {
                    retval.Add(new FileInfo(fileName));
                }
            }
            return new Collection<FileInfo>(retval);
        }


        public static Collection<FileInfo> CollectFilesEndingWith(string endString, DirectoryInfo resultsDir)
        {
            Collection<DirectoryInfo> allDirs = new Collection<DirectoryInfo> { resultsDir };
            foreach (DirectoryInfo di in resultsDir.GetDirectories("*", SearchOption.AllDirectories))
            {
                allDirs.Add(di);
            }

            Collection<FileInfo> retval = new Collection<FileInfo>();
            foreach (DirectoryInfo di in allDirs)
            {
                foreach (FileInfo fi in di.GetFiles())
                {
                    if (fi.Name.EndsWith(endString))
                        retval.Add(fi);
                }
            }
            return retval;
        }


        public static void ReproduceBehavior(Collection<FileInfo> tests)
        {
            foreach (FileInfo oneTest in tests)
            {
                if (!Reproduce(oneTest))
                {
                    Logger.Debug("Test " + oneTest + " NOT reproducible.");
                }
            }
        }

        private static bool Reproduce(FileInfo oneTest)
        {
            TestCase test = new TestCase(oneTest);
            TestCase.RunResults results = test.RunExternal();
            if (results.behaviorReproduced)
                return true;
            return false;
        }

        public static void Minimize(Collection<FileInfo> testPaths)
        {
            foreach (FileInfo oneTest in testPaths)
            {
                int linesRemoved = Minimize(oneTest);
                if (linesRemoved > 0)
                    Logger.Debug("Test " + oneTest + ": removed " + linesRemoved + " lines.");
            }
        }

        public static int Minimize(FileInfo testPath)
        {
            TestCase testFile = new TestCase(testPath);

            testFile.WriteToFile(testPath + ".nonmin");

            int linesRemoved = 0;

            //            Logger.Debug(testFile);

            for (int linePos = testFile.NumTestLines - 1; linePos >= 0; linePos--)
            {
                string oldLine = testFile.RemoveLine(linePos);

                if (!testFile.RunExternal().behaviorReproduced)
                {
                    testFile.AddLine(linePos, oldLine);
                }
                else
                {
                    linesRemoved++;
                }
            }

            testFile.WriteToFile(testPath);
            return linesRemoved;
        }
        public static Collection<FileInfo> RemoveNonReproducibleErrors(Collection<FileInfo> partitionedTests)
        {
            Collection<FileInfo> reproducibleTests = new Collection<FileInfo>();
            foreach (FileInfo test in partitionedTests)
            {
                TestCase testCase = new TestCase(test);

                // Don't attempt to reproduce non-error-revealing tests (to save time).
                if (!IsErrorRevealing(testCase))
                {
                    reproducibleTests.Add(test);
                    continue;
                }

                TestCase.RunResults runResults = testCase.RunExternal();
                if (runResults.behaviorReproduced)
                    reproducibleTests.Add(test);
                else
                {
                    if (!runResults.compilationSuccessful)
                    {
                        //Logger.Debug("@@@COMPILATIONFAILED:");
                        //foreach (CompilerError err in runResults.compilerErrors)
                        //    Logger.Debug("@@@" + err);
                    }
                    test.Delete();
                }
            }
            return reproducibleTests;
        }

        public static Dictionary<TestCase.ExceptionDescription, Collection<FileInfo>>
          ClassifyTestsByMessage(Collection<FileInfo> tests)
        {
            Dictionary<TestCase.ExceptionDescription, Collection<FileInfo>> testsByMessage =
                new Dictionary<TestCase.ExceptionDescription, Collection<FileInfo>>();

            foreach (FileInfo t in tests)
            {
                TestCase tc;
                try
                {
                    tc = new TestCase(t);
                }
                catch (TestCase.TestCaseParseException e)
                {
                    Logger.Error("Problem parsing test {0}. (Ignoring test.)", t.FullName);
                    Logger.Error(Util.SummarizeException(e, ""));
                    continue;
                }

                Collection<FileInfo> l;

                if (!testsByMessage.ContainsKey(tc.exception))
                {
                    l = new Collection<FileInfo>();
                    testsByMessage[tc.exception] = l;
                }
                else
                {
                    l = testsByMessage[tc.exception];
                }

                l.Add(t);
            }
            return testsByMessage;
        }

        private static bool IsErrorRevealing(TestCase tc)
        {
            TestCase.ExceptionDescription d;

            d = TestCase.ExceptionDescription.GetDescription(typeof(System.AccessViolationException));
            if (d.Equals(tc.exception))
                return true;

            d = TestCase.ExceptionDescription.GetDescription(typeof(System.NullReferenceException));
            if (d.Equals(tc.exception))
                return true;

            d = TestCase.ExceptionDescription.GetDescription(typeof(System.NullReferenceException));
            if (d.Equals(tc.exception))
                return true;

            return false;
        }

    }

}
