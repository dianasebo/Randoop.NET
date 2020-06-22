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
            Assert.AreEqual(ContractStateEnum.Missing, contractsManager.ValidateAssertionContracts(method, receiver, returnValue).PostconditionState);

            method = typeof(PostconditionTestClass).GetMethod("InvalidPostconditionMethod_NotBool");
            Assert.AreEqual(ContractStateEnum.Invalid, contractsManager.ValidateAssertionContracts(method, receiver, returnValue).PostconditionState);

            method = typeof(PostconditionTestClass).GetMethod("InvalidPostconditionMethod_NoOutput");
            Assert.AreEqual(ContractStateEnum.Invalid, contractsManager.ValidateAssertionContracts(method, receiver, returnValue).PostconditionState);

            method = typeof(PostconditionTestClass).GetMethod("ValidPostconditionMethod");
            Assert.AreEqual(ContractStateEnum.Ok, contractsManager.ValidateAssertionContracts(method, receiver, returnValue).PostconditionState);
        }

        [TestMethod]
        public void ValidateAssertionContractsTest_Invariant()
        {
            receiver = new NoInvariantTestClass();
            method = typeof(NoInvariantTestClass).GetMethod("Method");
            Assert.AreEqual(ContractStateEnum.Missing, contractsManager.ValidateAssertionContracts(method, receiver, returnValue).InvariantState);

            receiver = new InvalidInvariantTestClass();
            method = typeof(InvalidInvariantTestClass).GetMethod("Method");
            Assert.AreEqual(ContractStateEnum.Invalid, contractsManager.ValidateAssertionContracts(method, receiver, returnValue).InvariantState);

            receiver = new InvalidInvariantTestClass_AccessingPrivates();
            method = typeof(InvalidInvariantTestClass_AccessingPrivates).GetMethod("Method");
            Assert.AreEqual(ContractStateEnum.Invalid, contractsManager.ValidateAssertionContracts(method, receiver, returnValue).InvariantState);

            receiver = new ValidInvariantTestClass();
            method = typeof(ValidInvariantTestClass).GetMethod("Method");
            Assert.AreEqual(ContractStateEnum.Ok, contractsManager.ValidateAssertionContracts(method, receiver, returnValue).InvariantState);
        }

        [TestMethod]
        public void ValidateAssertionContractsTest_StaticInvariant()
        {
            receiver = new NoInvariantTestClass();
            method = typeof(NoInvariantTestClass).GetMethod("Method");
            Assert.AreEqual(ContractStateEnum.Missing, contractsManager.ValidateAssertionContracts(method, receiver, returnValue).StaticInvariantState);

            receiver = new InvalidInvariantTestClass();
            method = typeof(InvalidInvariantTestClass).GetMethod("Method");
            Assert.AreEqual(ContractStateEnum.Invalid, contractsManager.ValidateAssertionContracts(method, receiver, returnValue).StaticInvariantState);

            receiver = new InvalidInvariantTestClass_AccessingPrivates();
            method = typeof(InvalidInvariantTestClass_AccessingPrivates).GetMethod("Method");
            Assert.AreEqual(ContractStateEnum.Invalid, contractsManager.ValidateAssertionContracts(method, receiver, returnValue).StaticInvariantState);

            receiver = new InvalidStaticInvariant_AccessingInstanceFields();
            method = typeof(InvalidStaticInvariant_AccessingInstanceFields).GetMethod("Method");
            Assert.AreEqual(ContractStateEnum.Invalid, contractsManager.ValidateAssertionContracts(method, receiver, returnValue).StaticInvariantState);

            receiver = new ValidInvariantTestClass();
            method = typeof(ValidInvariantTestClass).GetMethod("Method");
            Assert.AreEqual(ContractStateEnum.Ok, contractsManager.ValidateAssertionContracts(method, receiver, returnValue).StaticInvariantState);
        }
    }
}
