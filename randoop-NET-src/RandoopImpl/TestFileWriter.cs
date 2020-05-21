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


using Common;
using System;
using System.IO;

namespace Randoop
{

    public interface ITestFileWriter
    {
        void Move(Plan p, Exception exceptionThrown);
        void MoveNormalTermination(Plan p);
        void Remove(Plan p);
        void WriteTest(Plan p);
        void RemoveTempDir();
    }

    internal class TestWriterHelperMethods
    {
        public static void WritePlanToFile(Plan p, string fileName, Type exceptionThrown, string className)
        {
            TestCase code;
            bool writeTest = true; //xiao.qu@us.abb.com -- added for avoid writing "dummy" code
            try
            {
                code = p.ToTestCase(exceptionThrown, true, className); //xiao.qu@us.abb.com -- if the middle parameter is set to "true", print plan for debugging
            }
            catch (Exception)
            {
                code = TestCase.Dummy(className);
                writeTest = false;
            }

            if (writeTest)
                code.WriteToFile(fileName);
        }
    }

    /// <summary>
    /// A test writer that writes all tests (plans) to the same
    /// directory.
    /// </summary>
    public class SingleDirTestWriter : ITestFileWriter
    {
        private DirectoryInfo outputDir;

        private string testPrefix;

        public SingleDirTestWriter(DirectoryInfo di, string testPrefix)
        {
            outputDir = di;
            if (!outputDir.Exists)
            {
                outputDir.Create();
            }
            if (testPrefix == null)
                this.testPrefix = "RandoopTest";
            else
                this.testPrefix = testPrefix;
        }

        public void Move(Plan p, Exception exceptionThrown)
        {
            string className = this.testPrefix + p.TestCaseId;
            string fileName = outputDir + "\\" + className + ".cs";

            string savedFileName = fileName + ".saved";
            File.Copy(fileName, savedFileName);
            new FileInfo(fileName).Delete();
            TestWriterHelperMethods.WritePlanToFile(p, fileName, exceptionThrown.GetType(), className);
            new FileInfo(savedFileName).Delete();
        }

        public void MoveNormalTermination(Plan p)
        {
            // Don't move anywhere.
        }

        public void Remove(Plan plan)
        {
            string className = testPrefix + plan.TestCaseId;
            string fileName = outputDir + "\\" + className + ".cs";
            new FileInfo(fileName).Delete();
        }

        public void WriteTest(Plan plan)
        {
            string className = testPrefix + plan.TestCaseId;
            string fileName = outputDir + "\\" + className + ".cs";
            TestWriterHelperMethods.WritePlanToFile(plan, fileName, null, className);
        }

        public void RemoveTempDir()
        {
            // No temp dir to remove.
        }
    }

    public class ClassifyingTestFileWriter : ITestFileWriter
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private DirectoryInfo outputDir;

        private int numNormalTerminationPlansWritten;

        private DirectoryInfo normalTerminationCurrentDir;

        private string testPrefix;

        public ClassifyingTestFileWriter(DirectoryInfo di, string testPrefix)
        {
            this.outputDir = di;
            this.numNormalTerminationPlansWritten = 0;
            DirectoryInfo tempDir = new DirectoryInfo(outputDir + "\\temp");
            Logger.Debug("Creating directory: " + tempDir.FullName);
            tempDir.Create();

            if (testPrefix == null)
                this.testPrefix = "RandoopTest";
            else
                this.testPrefix = testPrefix;
        }

        private void newSubDir()
        {
            // Find next index for normaltermination dir.
            int maxIndex = 0;
            foreach (DirectoryInfo di in outputDir.GetDirectories("normaltermination*"))
            {
                int dirIndex = int.Parse(di.Name.Substring("normaltermination".Length));
                if (dirIndex > maxIndex)
                    maxIndex = dirIndex;
            }
            this.normalTerminationCurrentDir = new DirectoryInfo(
                outputDir
                + "\\"
                + "normaltermination"
                + (maxIndex + 1));
            Util.Assert(!this.normalTerminationCurrentDir.Exists);
            this.normalTerminationCurrentDir.Create();
        }


        public void WriteTest(Plan p)
        {
            string className = this.testPrefix + p.TestCaseId;
            string fileName = outputDir + "\\" + "temp" + "\\" + className + ".cs";
            TestWriterHelperMethods.WritePlanToFile(p, fileName, null, className);
        }

        public void MoveNormalTermination(Plan p)
        {
            if (numNormalTerminationPlansWritten % 1000 == 0)
                newSubDir();

            string className = this.testPrefix + p.TestCaseId;
            string oldTestFileName = outputDir + "\\" + "temp" + "\\" + className + ".cs";
            string newTestFileName = normalTerminationCurrentDir + "\\" + className + ".cs";
            TestWriterHelperMethods.WritePlanToFile(p, newTestFileName, null, className);
            new FileInfo(oldTestFileName).Delete();
            numNormalTerminationPlansWritten++;
        }

        public void Move(Plan p, Exception exceptionThrown)
        {
            string dirName;
            MakeExceptionDirIfNotExists(exceptionThrown, out dirName);
            string className = this.testPrefix + p.TestCaseId;
            string oldTestFileName = outputDir + "\\" + "temp" + "\\" + className + ".cs";
            string newTestFileName = dirName + "\\" + className + ".cs";
            TestWriterHelperMethods.WritePlanToFile(p, newTestFileName, exceptionThrown.GetType(), className);
            new FileInfo(oldTestFileName).Delete();
        }

        private void MakeExceptionDirIfNotExists(Exception exceptionThrown, out string dirName)
        {
            string dirNameBase;
            if (exceptionThrown.Message.StartsWith("Randoop: an assertion was violated"))
            {
                dirNameBase = "AssertionViolations";
            }
            else
            {
                dirNameBase = exceptionThrown.GetType().FullName;
            }
            dirName = outputDir + "\\" + dirNameBase;
            DirectoryInfo d = new DirectoryInfo(dirName);
            if (!d.Exists)
                d.Create();
        }

        public void Remove(Plan p)
        {
            string className = this.testPrefix + p.TestCaseId;
            string oldTestFileName = outputDir + "\\" + "temp" + "\\" + className + ".cs";
            new FileInfo(oldTestFileName).Delete();
        }

        public void RemoveTempDir()
        {
            new DirectoryInfo(outputDir + "\\temp").Delete(true);
        }
    }



}
