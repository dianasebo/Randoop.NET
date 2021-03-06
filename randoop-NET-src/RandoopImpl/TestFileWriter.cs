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
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Randoop
{

    public interface TestFileWriter
    {
        void Move(Plan p, Exception exceptionThrown);
        void MoveNormalTermination(Plan p);
        void Remove(Plan p);
        void WriteTest(Plan p);
    }

    internal class TestWriterHelperMethods
    {
        public static void WritePlanToFile(Plan p, string fileName, Type exceptionThrown, string className, bool useRandoopContracts)
        {
            TestCase code;
            bool writeTest = true; //xiao.qu@us.abb.com -- added for avoid writing "dummy" code
            try
            {
                code = p.ToTestCase(exceptionThrown, true, className, useRandoopContracts); //xiao.qu@us.abb.com -- if the middle parameter is set to "true", print plan for debugging
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
    public class SingleDirectoryTestFileWriter : TestFileWriter
    {
        private DirectoryInfo outputDir;
        private bool useRandoopContracts;

        public SingleDirectoryTestFileWriter(DirectoryInfo di, bool useRandoopContracts)
        {
            outputDir = di;
            if (!outputDir.Exists)
            {
                outputDir.Create();
            }
            this.useRandoopContracts = useRandoopContracts;
        }

        public void Move(Plan p, Exception exceptionThrown)
        {
            string className = p.ClassName + p.TestCaseId;
            string fileName = outputDir + "\\" + className + ".cs";

            string savedFileName = fileName + ".saved";
            File.Copy(fileName, savedFileName);
            new FileInfo(fileName).Delete();
            TestWriterHelperMethods.WritePlanToFile(p, fileName, exceptionThrown.GetType(), className, useRandoopContracts);
            new FileInfo(savedFileName).Delete();
        }

        public void MoveNormalTermination(Plan p)
        {
            // Don't move anywhere.
        }

        public void Remove(Plan plan)
        {
            string className = plan.ClassName + plan.TestCaseId;
            string fileName = outputDir + "\\" + className + ".cs";
            new FileInfo(fileName).Delete();
        }

        public void WriteTest(Plan plan)
        {
            string className = plan.ClassName + plan.TestCaseId;
            string fileName = outputDir + "\\" + className + ".cs";
            TestWriterHelperMethods.WritePlanToFile(plan, fileName, null, className, useRandoopContracts);
        }
    }

    public class ClassifyingByBehaviorTestFileWriter : TestFileWriter
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private DirectoryInfo outputDir;
        private bool useRandoopContracts;
        private int numNormalTerminationPlansWritten;

        private DirectoryInfo normalTerminationCurrentDir;

        public ClassifyingByBehaviorTestFileWriter(DirectoryInfo di, bool useRandoopContracts)
        {
            outputDir = di;
            this.useRandoopContracts = useRandoopContracts;
            numNormalTerminationPlansWritten = 0;
            DirectoryInfo tempDir = new DirectoryInfo(outputDir + "\\temp");
            Logger.Debug("Creating directory: " + tempDir.FullName);
            tempDir.Create();
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
            normalTerminationCurrentDir = new DirectoryInfo(
                outputDir
                + "\\"
                + "normaltermination"
                + (maxIndex + 1));
            Util.Assert(!normalTerminationCurrentDir.Exists);
            normalTerminationCurrentDir.Create();
        }


        public void WriteTest(Plan p)
        {
            string testClassName = p.ClassName + p.TestCaseId;
            string fileName = outputDir + "\\" + "temp" + "\\" + testClassName + ".cs";
            TestWriterHelperMethods.WritePlanToFile(p, fileName, null, testClassName, useRandoopContracts);
        }

        public void MoveNormalTermination(Plan p)
        {
            if (numNormalTerminationPlansWritten % 1000 == 0)
                newSubDir();

            string testClassName = p.ClassName + p.TestCaseId;
            string oldTestFileName = outputDir + "\\" + "temp" + "\\" + testClassName + ".cs";
            string newTestFileName = normalTerminationCurrentDir + "\\" + testClassName + ".cs";
            TestWriterHelperMethods.WritePlanToFile(p, newTestFileName, null, testClassName, useRandoopContracts);
            new FileInfo(oldTestFileName).Delete();
            numNormalTerminationPlansWritten++;
        }

        public void Move(Plan p, Exception exceptionThrown)
        {
            string dirName;
            MakeExceptionDirIfNotExists(exceptionThrown, out dirName);
            string testClassName = p.ClassName + p.TestCaseId;
            string oldTestFileName = outputDir + "\\" + "temp" + "\\" + testClassName + ".cs";
            string newTestFileName = dirName + "\\" + testClassName + ".cs";
            TestWriterHelperMethods.WritePlanToFile(p, newTestFileName, exceptionThrown.GetType(), testClassName, useRandoopContracts);
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
            string testClassName = p.ClassName + p.TestCaseId;
            string oldTestFileName = outputDir + "\\" + "temp" + "\\" + testClassName + ".cs";
            new FileInfo(oldTestFileName).Delete();
        }
    }

    public class ClassifyingByClassTestFileWriter : TestFileWriter
    {
        private DirectoryInfo outputDir;
        private bool useRandoopContracts;
        private IList<DirectoryInfo> classDirectories;

        public ClassifyingByClassTestFileWriter(DirectoryInfo di, bool useRandoopContracts)
        {
            outputDir = di;
            if (!outputDir.Exists)
            {
                outputDir.Create();
            }
            this.useRandoopContracts = useRandoopContracts;
            classDirectories = new List<DirectoryInfo>();
            DirectoryInfo tempDir = new DirectoryInfo(outputDir + "\\temp");
            tempDir.Create();
        }

        public void Move(Plan p, Exception exceptionThrown)
        {
            var correspondingDirectory = DirectoryAlreadyExists(p.ClassName) ? classDirectories.Single(_ => _.Name == p.ClassName) : CreateNewDirectory(p.ClassName);
            string testClassName = p.ClassName + p.TestCaseId;
            string oldTestFileName = outputDir + "\\" + "temp" + "\\" + testClassName + ".cs";
            string newTestFileName = correspondingDirectory + "\\" + testClassName + ".cs";
            TestWriterHelperMethods.WritePlanToFile(p, newTestFileName, exceptionThrown.GetType(), testClassName, useRandoopContracts);
            new FileInfo(oldTestFileName).Delete();
        }

        public void MoveNormalTermination(Plan p)
        {
            var correspondingDirectory = DirectoryAlreadyExists(p.ClassName) ? classDirectories.Single(_ => _.Name == p.ClassName) : CreateNewDirectory(p.ClassName);
            string testClassName = p.ClassName + p.TestCaseId;
            string oldTestFileName = outputDir + "\\" + "temp" + "\\" + testClassName + ".cs";
            string newTestFileName = correspondingDirectory + "\\" + testClassName + ".cs";
            TestWriterHelperMethods.WritePlanToFile(p, newTestFileName, null, testClassName, useRandoopContracts);
            new FileInfo(oldTestFileName).Delete();
        }

        public void Remove(Plan p)
        {
            string testClassName = p.ClassName + p.TestCaseId;
            string oldTestFileName = outputDir + "\\" + "temp" + "\\" + testClassName + ".cs";
            new FileInfo(oldTestFileName).Delete();
        }

        public void WriteTest(Plan p)
        {
            string testClassName = p.ClassName + p.TestCaseId;
            string fileName = outputDir + "\\" + "temp" + "\\" + testClassName + ".cs";
            TestWriterHelperMethods.WritePlanToFile(p, fileName, null, testClassName, useRandoopContracts);
        }

        private bool DirectoryAlreadyExists(string className)
        {
            return classDirectories.Any(_ => _.Name == className);
        }

        private DirectoryInfo CreateNewDirectory(string className)
        {
            var dirName = outputDir + "\\" + className;
            DirectoryInfo newDirectory = new DirectoryInfo(dirName);
            newDirectory.Create();
            classDirectories.Add(newDirectory);
            return newDirectory;
        }
    }

}
