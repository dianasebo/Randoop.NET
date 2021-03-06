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
using Randoop.RandoopContracts;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Text;

namespace Randoop
{

    public enum IsNull { Unknown, Yes, No };

    public class ResultElementExecutionProperties
    {
        public IsNull isNull = IsNull.Unknown;
    }

    /// <summary>
    /// A plan is the data structure that represents a test.
    /// 
    /// A plan represents a state transformation and the
    /// values (objects and primitives) resulting from the transformation.
    /// 
    /// You can also think of a plan as a DAG that represents
    /// a set of constructor/method calls and their dependencies.
    /// 
    /// The tests that Randoop creates are string representations
    /// of plans.
    /// 
    /// WARNING: THIS CODE IS VERY CONFUSING! We should simplify
    /// the data structure. If you have questions about it,
    /// ask Shuvendu Lahiri (shuvendu).
    /// 
    /// </summary>
    public class Plan
    {
        /// <summary>
        /// A unique number, different for every plan ever created.
        /// </summary>
        public readonly long UniqueId;

        public static long UniqueIdCounter = 0;

        /// <summary>
        /// The transfomer specifies the state change for this plan.
        /// </summary>
        public readonly Transformer transformer;

        /// <summary>
        /// The plans to be executed before this plan's transformation.
        /// </summary>
        public readonly Plan[] parentPlans;

        /// <summary>
        /// The mappings from transformation parameter indices to the
        /// relevant parent plan results that provides the parameters.
        /// </summary>
        public readonly ParameterChooser[] parameterChoosers;

        public Exception exceptionThrown = null;

        public string ClassName;

        public ContractState CanGenerateContractAssertion;

        private readonly int treeNodes;

        private readonly bool[] activeTupleElements;

        private readonly ResultElementExecutionProperties[] resultElementProperties;

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            foreach (Plan p in parentPlans)
            {
                b.Append(p.ToString());
                b.AppendLine();
            }
            b.Append("plan " + UniqueId + " transformer=" + transformer.ToString() + " parents=[ ");
            foreach (Plan p in parentPlans)
            {
                b.Append(p.UniqueId + " ");
            }
            b.Append("], inputs=[ ");
            foreach (ParameterChooser pc in parameterChoosers)
            {
                b.Append(pc.ToString() + " ");
            }
            b.Append("]");
            return b.ToString();
        }

        public int NumTupleElements
        {
            get
            {
                return activeTupleElements.Length;
            }
        }

        public long TestCaseId
        {
            get
            {
                return UniqueId;
            }
        }

        public ResultElementExecutionProperties GetResultElementProperty(int i)
        {
            Util.Assert(i >= 0 && i < resultElementProperties.Length);
            return resultElementProperties[i];
        }

        public bool IsActiveTupleElement(int i)
        {
            Util.Assert(i >= 0 && i < activeTupleElements.Length);
            return activeTupleElements[i];
        }

        public void SetActiveTupleElement(int i, bool value)
        {
            Util.Assert(i >= 0 && i < activeTupleElements.Length);
            Util.Assert(transformer is ConstructorCallTransformer ? i != 0 : true);
            activeTupleElements[i] = value;
        }

        public void SetResultElementProperty(int i, ResultElementExecutionProperties value)
        {
            Util.Assert(i >= 0 && i < resultElementProperties.Length);
            Util.Assert(transformer is ConstructorCallTransformer ? i != 0 : true);
            resultElementProperties[i] = value;
        }

        public int numTimesExecuted = 0;
        public double executionTimeAccum = 0;

        public double AverageExecutionTime
        {
            get
            {
                return executionTimeAccum / numTimesExecuted;
            }

        }

        public double TotalExecutionTime
        {
            get
            {
                return executionTimeAccum;
            }

        }

        /// <summary>
        /// Checks that the fparameterChoosers mapping is consistent
        /// with the result tuples of the parent plans and the
        /// parameters of the transformer.
        /// </summary>
        public void RepOk()
        {
            for (int i = 0; i < parameterChoosers.Length; i++)
            {
                ParameterChooser c = parameterChoosers[i];
                Util.Assert(c.planIndex >= 0 && c.planIndex < parentPlans.Length);
                Util.Assert(c.resultIndex >= 0 &&
                    c.resultIndex < parentPlans[c.planIndex].transformer.TupleTypes.Length);
                Util.Assert(parentPlans[c.planIndex].transformer.TupleTypes[c.resultIndex]
                    .Equals(transformer.ParameterTypes[i]));
            }
        }

        /// <summary>
        /// Two plans are equal if their transformers and parent plans are
        /// equals, and their mapping from parent plan results to
        /// transformer parameters are the same.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            Plan o = obj as Plan;
            if (obj == null)
                return false;

            if (this == obj)
                return true;

            if (!transformer.Equals(o.transformer))
                return false;
            if (parentPlans.Length != o.parentPlans.Length)
                return false;
            for (int i = 0; i < parentPlans.Length; i++)
                if (!parentPlans[i].Equals(o.parentPlans[i]))
                    return false;
            if (parameterChoosers.Length != o.parameterChoosers.Length)
                return false;
            for (int i = 0; i < parameterChoosers.Length; i++)
                if (!parameterChoosers[i].Equals(o.parameterChoosers[i]))
                    return false;
            return true;
        }

        private int hashCodeCached = 0;
        private bool computedHashCode = false;

        public override int GetHashCode()
        {
            if (!computedHashCode)
            {
                hashCodeCached = ComputeHashCode();
            }
            return hashCodeCached;
        }

        private int ComputeHashCode()
        {
            int result = 7;
            result = 37 * result + transformer.GetHashCode();
            foreach (Plan p in parentPlans)
                result = 37 * result + p.GetHashCode();
            if (parentPlans.Length > 0)
            {
                foreach (ParameterChooser c in parameterChoosers)
                    result = 37 * result + c.GetHashCode();
            }
            return result;
        }

        /// <summary>
        /// Specifies a value among all result tuples in the parent plans.
        /// </summary>
        public class ParameterChooser
        {
            /// <summary>
            /// Index into array of parent plans.
            /// </summary>
            public int planIndex;

            /// <summary>
            /// For a given parent plan, index into the array of results for the plan.
            /// </summary>
            public int resultIndex;

            public override bool Equals(object obj)
            {
                ParameterChooser c = obj as ParameterChooser;
                if (c == null)
                    return false;
                return (planIndex == c.planIndex && resultIndex == c.resultIndex);
            }

            public override int GetHashCode()
            {
                return planIndex.GetHashCode() + resultIndex.GetHashCode();
            }

            public ParameterChooser(int planIndex, int resultIndex)
            {
                this.planIndex = planIndex;
                this.resultIndex = resultIndex;
            }

            public override string ToString()
            {
                return "(" + planIndex + " " + resultIndex + ")";
            }
        }



        protected Plan()
        {
        }

        private class SingletonDummyVoidPlan : Plan
        {
            public SingletonDummyVoidPlan()
                : base()
            {
            }

            public override bool Equals(object obj)
            {
                return obj == this;
            }

            public override int GetHashCode()
            {
                return 0;
            }


        }

        public static Plan DummyVoidPlan = new SingletonDummyVoidPlan();

        /// <summary>
        /// Creates a Plan.
        /// </summary>
        /// <param name="transfomer">The state transformer that this plan represents.</param>
        /// <param name="parentPlans">The parent plans for this plan.</param>
        /// <param name="parameterChoosers">The parameter choosers for the parent plans.</param>
        /// <param name="resultTypes">The types (and number of) results that executing this plan yields.</param>
        public Plan(Transformer transfomer, Plan[] parentPlans, ParameterChooser[] parameterChoosers)
        {
            CanGenerateContractAssertion = new ContractState();
            UniqueId = UniqueIdCounter++;
            transformer = transfomer;
            this.parentPlans = parentPlans;

            this.parameterChoosers = parameterChoosers;
            activeTupleElements = transformer.DefaultActiveTupleTypes;

            resultElementProperties = new ResultElementExecutionProperties[this.activeTupleElements.Length];
            for (int i = 0; i < resultElementProperties.Length; i++)
                resultElementProperties[i] = new ResultElementExecutionProperties();

            treeNodes = 1;
            foreach (Plan p in parentPlans)
                treeNodes += p.treeNodes;
        }

        /// <summary>
        /// Creates a plan that represents a constant value.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="constantValue"></param>
        /// <returns></returns>
        public static Plan Constant(Type t, object constantValue)
        {
            return new Plan(PrimitiveValueTransformer.Get(t, constantValue), new Plan[0], new ParameterChooser[0]);
        }


        /// <summary>
        /// Invokes the code that this plan represents.
        /// TODO: what's the difference between this and the other Execute method?
        /// </summary>
        /// <param name="executionResults"></param>
        /// <returns></returns>
        public bool ExecuteHelper(out ResultTuple executionResult, TextWriter executionLog,
            TextWriter debugLog, out bool preconditionViolated, out Exception exceptionThrown, out bool contractViolated,
            bool forbidNull, bool useRandoopContracts, out ContractState contractStates)
        {
            // Execute parent plans
            ResultTuple[] results1 = new ResultTuple[parentPlans.Length];
            for (int i = 0; i < parentPlans.Length; i++)
            {
                Plan plan = parentPlans[i];
                ResultTuple tempResults;
                if (!plan.ExecuteHelper(out tempResults, executionLog, debugLog, out preconditionViolated, out exceptionThrown, out contractViolated, forbidNull, useRandoopContracts, out contractStates))
                {
                    executionResult = null;
                    return false;
                }
                results1[i] = tempResults;
            }

            //// Execute
            if (!transformer.Execute(out executionResult, results1, parameterChoosers, executionLog, debugLog,
                out preconditionViolated, out exceptionThrown, out contractViolated, forbidNull, useRandoopContracts, out contractStates))
            {
                executionResult = null;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Invokes the code that this plan represents.
        /// </summary>
        /// <param name="executionResults"></param>
        /// <returns></returns>
        public bool Execute(out ResultTuple executionResult, TextWriter executionLog,
            TextWriter debugLog, out bool preconditionViolated, out Exception exceptionThrown,
            out bool contractViolated, bool forbidNull, bool useRandoopContracts)
        {
            numTimesExecuted++;
            long startTime = 0;
            Timer.QueryPerformanceCounter(ref startTime);

            // Execute parent plans
            ResultTuple[] results1 = new ResultTuple[parentPlans.Length];
            for (int i = 0; i < parentPlans.Length; i++)
            {
                Plan plan = parentPlans[i];
                ResultTuple tempResults;
                if (!plan.ExecuteHelper(out tempResults, executionLog, debugLog, out preconditionViolated, out exceptionThrown, out contractViolated, forbidNull, useRandoopContracts, out CanGenerateContractAssertion))
                {
                    executionResult = null;
                    return false;
                }
                results1[i] = tempResults;
            }

            // Execute.
            if (!transformer.Execute(out executionResult, results1, parameterChoosers, executionLog, debugLog,
                out preconditionViolated, out exceptionThrown, out contractViolated, forbidNull, useRandoopContracts, out CanGenerateContractAssertion))
            {
                executionResult = null;
                return false;
            }

            RecordExecutionTime(startTime);
            return true;
        }

        public TestCase ToTestCase(Type exceptionThrown, bool printPlanToString, string className, bool useRandoopContracts)
        {
            // Create imports list.
            Collection<string> imports = new Collection<string>();
            foreach (string nameSpace in GetNameSpaces())
                imports.Add("using " + nameSpace + ";");

            // Create test code list.
            Collection<string> testCode = new Collection<string>();
            foreach (string codeLine in CodeGenerator.AsCSharpCode(this, useRandoopContracts))
            {
                testCode.Add(codeLine);
            }

            if (printPlanToString)
            {
                testCode.Add("/*");
                testCode.Add(ToString());
                testCode.Add("*/");
            }

            // Create assemblies list.
            Collection<string> assemblies = GetAssemblies();

            // Create LastAction.
            ////xiao.qu@us.abb.com changes for capture last return value -- start 
            //string last_ret_val = "null";
            //string last_ret_type = "null";
            //if (this.transformer.ReturnValue.Count != 0)  //actions other than "MethodCall" do not have ReturnValue
            //{
            //   if (this.transformer.ReturnValue[this.transformer.ReturnValue.Count-1] != null)
            //    {
            //        last_ret_val = this.transformer.ReturnValue[this.transformer.ReturnValue.Count - 1].ToString().Replace("\n", "\\n").Replace("\r", "\\r");
            //        last_ret_type = this.transformer.ReturnValue[this.transformer.ReturnValue.Count - 1].GetType().ToString();
            //    }
            //}
            ////xiao.qu@us.abb.com changes for capture last return value -- end

            TestCase.LastAction lastAction =
                new TestCase.LastAction(TestCase.LastAction.LASTACTION_MARKER
            //    + this.transformer.ToString() //xiao.qu@us.abb.com changes for capture last return value
            //    + " type: " + last_ret_type
            //    + " val: " + last_ret_val);
            + transformer.ToString());

            // Create exception.
            TestCase.ExceptionDescription exception;
            if (exceptionThrown == null)
                exception = TestCase.ExceptionDescription.NoException();
            else
                exception = TestCase.ExceptionDescription.GetDescription(exceptionThrown);

            // Put everything together, return TestCase.
            return new TestCase(lastAction, exception, imports, assemblies, className, testCode);
        }

        private Collection<string> GetNameSpaces()
        {
            CollectReferencedAssembliesVisitor v = new CollectReferencedAssembliesVisitor();
            Visit(v);
            Collection<string> ret = new Collection<string>();
            foreach (string t in v.GetDeclaringTypes())
                ret.Add(t);
            return ret;
        }

        private Collection<string> GetAssemblies()
        {
            CollectReferencedAssembliesVisitor v = new CollectReferencedAssembliesVisitor();
            Visit(v);
            Collection<string> ret = new Collection<string>();
            foreach (string t in v.GetAssemblies())
                ret.Add(t);
            return ret;
        }


        private void RecordExecutionTime(long startTime)
        {
            long endTime = 0;
            Timer.QueryPerformanceCounter(ref endTime);

            double executionTime = (endTime - startTime) / ((double)Timer.PerfTimerFrequency);

            executionTimeAccum += executionTime;

        }

        #region IWeightedElement Members

        public double Weight()
        {
            return 1.0 / (1.0 + treeNodes);
        }

        #endregion


        // TODO this is weird: A plan does not visit a visitor--it's the other way around!         
        public void Visit(IVisitor a)
        {
            a.Accept(this);
            a.Accept(transformer);
            foreach (Plan pp in parentPlans)
                pp.Visit(a);
        }
    }



    /// <summary>
    /// Collects the objects and primitives resulting from an execution
    /// as an array of objects.
    /// </summary>
    public class ResultTuple
    {
        public readonly object[] tuple;

        public ResultTuple(object receiver)
        {
            tuple = new object[1];
            tuple[0] = receiver;
        }

        public ResultTuple(ConstructorInfo constructor, object newObject, object[] results)
        {
            CheckParams(constructor, newObject, results);

            tuple = new object[results.Length + 1];
            tuple[0] = newObject;
            for (int i = 0; i < results.Length; i++)
                tuple[i + 1] = results[i];
        }

        public ResultTuple(MethodInfo method, object receiver, object returnValue, object[] results)
        {
            CheckParams(method, receiver, results);

            tuple = new object[results.Length + 2];
            tuple[0] = receiver;
            tuple[1] = returnValue;
            for (int i = 0; i < results.Length; i++)
                tuple[i + 2] = results[i];
        }

        private void CheckParams(MethodBase method, object receiver, object[] results)
        {
            ParameterInfo[] parameters = method.GetParameters();
            //Util.Assert(receiver != null && method.DeclaringType.IsInstanceOfType(receiver));
            for (int i = 0; i < results.Length; i++)
            {
                Type t = parameters[i].ParameterType;
                if (t.IsPrimitive)
                    Util.Assert(results[i] != null);
                else
                {
                    //Util.Assert(results[i] == null || t.IsInstanceOfType(results[i]));
                }
            }
        }

        public ResultTuple(Type t, object[] results)
        {
            Util.Assert(results.Length == 1);
            tuple = new object[1];
            tuple[0] = results[0];
        }

        public ResultTuple(ArrayOrArrayListBuilderTransformer arrayBuilder, object resultingArray)
        {
            tuple = new object[1];
            tuple[0] = resultingArray;
        }
    }


}

