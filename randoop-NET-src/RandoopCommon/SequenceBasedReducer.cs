using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Common
{
    public static class SequenceBasedReducer
    {
        public static Collection<FileInfo> Reduce(Collection<FileInfo> tests)
        {
            Dictionary<SequenceBasedEquivalenceClass, TestCase> representatives = new Dictionary<SequenceBasedEquivalenceClass, TestCase>();
            Dictionary<SequenceBasedEquivalenceClass, FileInfo> representativesFileInfos = new Dictionary<SequenceBasedEquivalenceClass, FileInfo>();


            foreach (FileInfo file in tests)
            {
                TestCase testCase;
                try
                {
                    testCase = new TestCase(file);
                }
                catch (Exception)
                {
                    // File does not contain a valid test case, or
                    // test case is malformed.
                    continue;
                }


                SequenceBasedEquivalenceClass partition = new SequenceBasedEquivalenceClass(testCase);
                // If there are no representatives for this partition,
                // use testCase as the representative.
                if (!representatives.ContainsKey(partition))
                {
                    representatives[partition] = testCase;
                    representativesFileInfos[partition] = file;
                }
                // if testCase is larger than the current representative (the current test sequence is a subset),
                // use testCase as the representative.
                // Delete the old representative.
                else if (testCase.NumTestLines > representatives[partition].NumTestLines)
                {
                    //representativesFileInfos[partition].Delete();
                    representativesFileInfos[partition].MoveTo(representativesFileInfos[partition].FullName + ".reduced");
                    representatives[partition] = testCase;
                    representativesFileInfos[partition] = file;
                }
                // sequence of testCase is a subset of the sequence of current representative.
                // Delete testCase.
                else
                {
                    //file.Delete();
                    file.MoveTo(file.FullName + ".reduced");
                }
            }

            List<FileInfo> retval = new List<FileInfo>();
            retval.AddRange(representativesFileInfos.Values);
            return new Collection<FileInfo>(retval);
        }
    }
}
