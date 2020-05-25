using Microsoft.VisualStudio.TestTools.UnitTesting;
using Randoop.RandoopContracts;
using System.Reflection;

namespace RandoopImplTest.RandoopContracts
{
    [TestClass]
    public class RandoopContractsManagerTest
    {
        private RandoopContractsManager contractsManager;
        private MethodInfo method;
        private object receiver;
        private int returnValue = 5;

        [TestInitialize]
        public void SetUp()
        {
            contractsManager = new RandoopContractsManager();
        }

        [TestMethod]
        public void ValidateAssertionContractsTest_Postcondition()
        {
            receiver = new PostconditionTestClass();

            method = typeof(PostconditionTestClass).GetMethod("NoPostconditionMethod");
            Assert.IsFalse(contractsManager.ValidateAssertionContracts(method, receiver, returnValue).FromPostcondition);

            method = typeof(PostconditionTestClass).GetMethod("InvalidPostconditionMethod_NotBool");
            Assert.IsFalse(contractsManager.ValidateAssertionContracts(method, receiver, returnValue).FromPostcondition);

            method = typeof(PostconditionTestClass).GetMethod("InvalidPostconditionMethod_NoOutput");
            Assert.IsFalse(contractsManager.ValidateAssertionContracts(method, receiver, returnValue).FromPostcondition);

            method = typeof(PostconditionTestClass).GetMethod("ValidPostconditionMethod");
            Assert.IsTrue(contractsManager.ValidateAssertionContracts(method, receiver, returnValue).FromPostcondition);
        }

        [TestMethod]
        public void ValidateAssertionContractsTest_Invariant()
        {
            receiver = new NoInvariantTestClass();
            method = typeof(NoInvariantTestClass).GetMethod("Method");
            Assert.IsFalse(contractsManager.ValidateAssertionContracts(method, receiver, returnValue).FromInvariant);

            receiver = new InvalidInvariantTestClass();
            method = typeof(InvalidInvariantTestClass).GetMethod("Method");
            Assert.IsFalse(contractsManager.ValidateAssertionContracts(method, receiver, returnValue).FromInvariant);

            receiver = new InvalidInvariantTestClass_AccessingPrivates();
            method = typeof(InvalidInvariantTestClass_AccessingPrivates).GetMethod("Method");
            Assert.IsFalse(contractsManager.ValidateAssertionContracts(method, receiver, returnValue).FromInvariant);

            receiver = new ValidInvariantTestClass();
            method = typeof(ValidInvariantTestClass).GetMethod("Method");
            Assert.IsTrue(contractsManager.ValidateAssertionContracts(method, receiver, returnValue).FromInvariant);
        }

        [TestMethod]
        public void ValidateAssertionContractsTest_StaticInvariant()
        {
            receiver = new NoInvariantTestClass();
            method = typeof(NoInvariantTestClass).GetMethod("Method");
            Assert.IsFalse(contractsManager.ValidateAssertionContracts(method, receiver, returnValue).FromStaticInvariant);

            receiver = new InvalidInvariantTestClass();
            method = typeof(InvalidInvariantTestClass).GetMethod("Method");
            Assert.IsFalse(contractsManager.ValidateAssertionContracts(method, receiver, returnValue).FromStaticInvariant);

            receiver = new InvalidInvariantTestClass_AccessingPrivates();
            method = typeof(InvalidInvariantTestClass_AccessingPrivates).GetMethod("Method");
            Assert.IsFalse(contractsManager.ValidateAssertionContracts(method, receiver, returnValue).FromStaticInvariant);

            receiver = new InvalidStaticInvariant_AccessingInstanceFields();
            method = typeof(InvalidStaticInvariant_AccessingInstanceFields).GetMethod("Method");
            Assert.IsFalse(contractsManager.ValidateAssertionContracts(method, receiver, returnValue).FromStaticInvariant);

            receiver = new ValidInvariantTestClass();
            method = typeof(ValidInvariantTestClass).GetMethod("Method");
            Assert.IsTrue(contractsManager.ValidateAssertionContracts(method, receiver, returnValue).FromStaticInvariant);
        }
    }
}
