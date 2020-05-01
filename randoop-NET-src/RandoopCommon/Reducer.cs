using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Common
{
    public static class Reducer
    {
        public static Collection<FileInfo> Reduce(Collection<FileInfo> tests)
        {
            Dictionary<EquivalenceClass, TestCase> representatives = new Dictionary<EquivalenceClass, TestCase>();
            Dictionary<EquivalenceClass, FileInfo> representativesFileInfos = new Dictionary<EquivalenceClass, FileInfo>();


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


                EquivalenceClass partition = new EquivalenceClass(testCase);
                // If there are no representatives for this partition,
                // use testCase as the representative.
                if (!representatives.ContainsKey(partition))
                {
                    representatives[partition] = testCase;
                    representativesFileInfos[partition] = file;
                }
                // if testCase is smaller than the current representative,
                // use testCase as the representative.
                // Delete the old representative.
                else if (testCase.NumTestLines < representatives[partition].NumTestLines)
                {
                    //representativesFileInfos[partition].Delete();
                    representativesFileInfos[partition].MoveTo(representativesFileInfos[partition].FullName + ".reduced");

                    representatives[partition] = testCase;
                    representativesFileInfos[partition] = file;
                }
                // Representative is redundant and larger than current representative.
                // Delete representative.
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
