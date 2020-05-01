using System;
using System.Collections.ObjectModel;

namespace Common
{
    public class SequenceBasedEquivalenceClass
    {
        public readonly TestCase representative;

        public SequenceBasedEquivalenceClass(TestCase testCase)
        {
            representative = testCase ?? throw new ArgumentNullException();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SequenceBasedEquivalenceClass other)) return false;

            string testPlans = getSequence(representative.testPlanCollection);
            string testPlans2 = getSequence(other.representative.testPlanCollection);

            if (testPlans.Contains(testPlans2))
                return true;
            if (testPlans2.Contains(testPlans))
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            return representative.lastAction.GetHashCode() + 3 ^ representative.exception.GetHashCode();
        }

        public string getSequence(Collection<string> testplans)
        {
            string testSequence = "";

            foreach (string planline in testplans)
            {
                string test = planline.Substring(0, planline.IndexOf("transformer"));
                testSequence += test;
            }

            return testSequence;
        }

        public override string ToString()
        {
            return "<equivalence class lastAction=\"" + representative.lastAction
            + "\" exception=\"" + representative.exception + "\">";
        }
    }
}
