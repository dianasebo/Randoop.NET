using RandoopContracts;

namespace RandoopImplTest.RandoopContracts
{
    public class PostconditionTestClass
    {
        public int NoPostconditionMethod() => 0;

        [Postcondition("output + 5")]
        public int InvalidPostconditionMethod_NotBool() => 0;

        [Postcondition("Ana are mere.")]
        public int InvalidPostconditionMethod_NoOutput() => 0;

        [Postcondition("output > 0")]
        public int ValidPostconditionMethod() => 0;
    }

    public class NoInvariantTestClass
    {
        public int Method() => 0;
    }

    [Invariant("Ana are mere")]
    [StaticInvariant("Ana are mere")]
    public class InvalidInvariantTestClass
    {
        public int Method() => 0;
    }

    [Invariant("privateField + PublicField > 0", "privateField", "PublicField")]
    [StaticInvariant("privateStaticField == 0", "privateStaticField")]
    public class InvalidInvariantTestClass_AccessingPrivates
    {
        public int PublicField;
        private int privateField;
        private static int privateStaticField;

        public int Method() => 0;
    }

    [StaticInvariant("PublicField + PublicStaticField > 0", "PublicField", "PublicStaticField")]
    public class InvalidStaticInvariant_AccessingInstanceFields
    {
        public int PublicField;
        public static int PublicStaticField;

        public int Method() => 0;
    }

    [Invariant("PublicField + PublicStaticField > -1", "PublicField", "PublicStaticField")]
    [StaticInvariant("PublicStaticField == 0", "PublicStaticField")]
    public class ValidInvariantTestClass
    {
        public int PublicField;
        public static int PublicStaticField;

        public int Method() => 0;
    }
}
