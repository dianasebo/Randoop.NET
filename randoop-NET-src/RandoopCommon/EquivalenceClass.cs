using System;

namespace Common
{
    public class EquivalenceClass
    {
        public readonly TestCase representative;

        public EquivalenceClass(TestCase testCase)
        {
            representative = testCase ?? throw new ArgumentNullException();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is EquivalenceClass other))
                return false;
            if (!ThrowTheSameException(other))
                return false;
            if (!EndWithTheSameMethodCall(other)) 
                return false;
            return true;
        }

        private bool EndWithTheSameMethodCall(EquivalenceClass other)
        {
            return representative.lastAction.Equals(other.representative.lastAction);
        }

        private bool ThrowTheSameException(EquivalenceClass other)
        {
            return representative.exception.Equals(other.representative.exception);
        }

        public override int GetHashCode()
        {
            return representative.lastAction.GetHashCode() + 3 ^ representative.exception.GetHashCode();
        }

        public override string ToString()
        {
            return "<equivalence class lastAction=\"" + representative.lastAction
            + "\" exception=\"" + representative.exception + "\">";
        }
    }
}
