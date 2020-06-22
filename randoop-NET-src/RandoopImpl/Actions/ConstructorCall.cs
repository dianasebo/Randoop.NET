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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Text;

namespace Randoop
{

    public class ConstructorCallTransformer : Transformer
    {
        public readonly ConstructorInfo constructor;
        public readonly ConstructorInfo coverageInfo;

        public int timesExecuted = 0;
        public double executionTimeAccum = 0;

        private static Dictionary<ConstructorInfo, ConstructorCallTransformer> cachedTransformers =
            new Dictionary<ConstructorInfo, ConstructorCallTransformer>();


        public static ConstructorCallTransformer Get(ConstructorInfo field)
        {
            ConstructorCallTransformer t;
            cachedTransformers.TryGetValue(field, out t);
            if (t == null)
                cachedTransformers[field] = t = new ConstructorCallTransformer(field);
            return t;
        }

        public override string MemberName
        {
            get
            {
                return constructor.DeclaringType.FullName;
            }
        }

        public override int TupleIndexOfIthInputParam(int i)
        {
            if (i < 0 && i > resultTypes.Length - 1) throw new ArgumentException("index out of range.");
            return (i + 1); // skip new object.
        }

        private readonly Type[] resultTypes;

        private bool[] defaultActiveResultTypes;

        public override Type[] TupleTypes
        {
            get { return resultTypes; }
        }

        public override bool[] DefaultActiveTupleTypes
        {
            get { return defaultActiveResultTypes; }
        }

        public override Type[] ParameterTypes
        {
            get
            {
                ParameterInfo[] parameters = constructor.GetParameters();
                Type[] retval = new Type[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                    retval[i] = parameters[i].ParameterType;
                return retval;
            }
        }

        public override string ToString()
        {
            return constructor.DeclaringType.FullName + " constructor " + constructor.ToString();
        }

        public override bool Equals(object obj)
        {
            ConstructorCallTransformer c = obj as ConstructorCallTransformer;
            if (c == null)
                return false;
            return (this.constructor.Equals(c.constructor));
        }

        public override int GetHashCode()
        {
            return constructor.GetHashCode();
        }



        private ConstructorCallTransformer(ConstructorInfo constructor)
        {
            Util.Assert(constructor is ConstructorInfo);
            this.constructor = constructor as ConstructorInfo;
            this.coverageInfo = constructor;

            ParameterInfo[] pis = constructor.GetParameters();

            resultTypes = new Type[pis.Length + 1];
            defaultActiveResultTypes = new bool[pis.Length + 1];

            resultTypes[0] = constructor.DeclaringType;
            defaultActiveResultTypes[0] = true;

            for (int i = 0; i < pis.Length; i++)
            {
                resultTypes[i + 1] = pis[i].ParameterType;
                //if (pis[i].IsOut)
                //    defaultActiveResultTypes[i + 1] = true;
                //else
                defaultActiveResultTypes[i + 1] = false;
            }
        }

        public override string ToCSharpCode(ReadOnlyCollection<string> arguments, string newValueName, bool useRandoopContracts, ContractState contractStates
            )
        {
            // TODO assert that arguments.Count is correct.
            StringBuilder code = new StringBuilder();
            string retType =
                SourceCodePrinting.ToCodeString(constructor.DeclaringType);
            code.Append(retType + " " + newValueName + " = ");
            code.Append("new " + SourceCodePrinting.ToCodeString(constructor.DeclaringType));
            code.Append("(");
            for (int i = 0; i < arguments.Count; i++)
            {
                if (i > 0)
                    code.Append(" , ");

                // Cast.
                code.Append("(");
                code.Append(SourceCodePrinting.ToCodeString(ParameterTypes[i]));
                code.Append(")");
                code.Append(arguments[i]);
            }
            code.Append(");");

            var assertion = string.Empty;
            if (useRandoopContracts)
            {
                assertion = new RandoopContractAssertionGenerator().Compute(constructor, newValueName, contractStates);
            }

            code.Append(assertion);

            return code.ToString();
        }

        ////xiao.qu@us.abb.com adds for capture return value for regression assertion
        //public override string ToCSharpCode(ReadOnlyCollection<string> arguments, string newValueName, string return_val)
        //{
        //    return ToCSharpCode(arguments, newValueName);
        //}


        //TODO Diana: check for precondition violations if useRandoopContracts = true
        //TODO Diana: compute canGenerateContractAssertions
        public override bool Execute(out ResultTuple ret, ResultTuple[] results,
            Plan.ParameterChooser[] parameterMap, TextWriter executionLog, TextWriter debugLog, out bool preconditionViolated, out Exception exceptionThrown, out bool contractViolated, bool forbidNull, bool useRandoopContracts, out ContractState contractStates)
        {
            contractViolated = false;
            preconditionViolated = false;
            contractStates = new ContractState();

            long startTime = 0;
            Timer.QueryPerformanceCounter(ref startTime);

            object[] objects = new object[constructor.GetParameters().Length];
            // Get the actual objects from the results using parameterIndices;
            for (int i = 0; i < constructor.GetParameters().Length; i++)
            {
                Plan.ParameterChooser pair = parameterMap[i];
                objects[i] = results[pair.planIndex].tuple[pair.resultIndex];
            }

            preconditionViolated = false;
            if (useRandoopContracts)
            {
                try
                {
                    preconditionViolated = new RandoopContractsManager().PreconditionViolated(constructor, objects);
                }
                catch (InvalidRandoopContractException) { } //precondition is invalid, ignore it and proceed with execution

                if (preconditionViolated)
                {
                    ret = null;
                    exceptionThrown = null;
                    contractViolated = false;
                    return false;
                }
            }

            if (forbidNull)
                foreach (object o in objects)
                    Util.Assert(o != null);

            object newObject = null;

            CodeExecutor.CodeToExecute call =
                delegate () { newObject = constructor.Invoke(objects); };

            executionLog.WriteLine("execute constructor " + this.constructor.DeclaringType);
            debugLog.WriteLine("execute constructor " + this.constructor.DeclaringType); //xiao.qu@us.abb.com adds
            executionLog.Flush();

            timesExecuted++;
            bool retval = true;
            if (!CodeExecutor.ExecuteReflectionCall(call, debugLog, out exceptionThrown))
            {
                ret = null;


                if (exceptionThrown is AccessViolationException)
                {
                    //Logger.Debug("SECOND CHANCE AV!" + this.ToString());
                    //Logging.LogLine(Logging.GENERAL, "SECOND CHANCE AV!" + this.ToString());                    
                }

                //for exns we can ony add the class to faulty classes when its a guideline violation
                if (Util.GuidelineViolation(exceptionThrown.GetType()))
                {
                    PlanManager.numDistinctContractViolPlans++;

                    KeyValuePair<MethodBase, Type> k = new KeyValuePair<MethodBase, Type>(this.constructor, exceptionThrown.GetType());
                    if (!exnViolatingMethods.ContainsKey(k))
                    {
                        PlanManager.numContractViolatingPlans++;
                        exnViolatingMethods[k] = true;
                    }

                    //add this class to the faulty classes
                    contractExnViolatingClasses[constructor.GetType()] = true;
                    contractExnViolatingMethods[constructor] = true;
                }

                executionLog.WriteLine("execution failure."); //xiao.qu@us.abb.com adds
                return false;
            }
            else
                ret = new ResultTuple(constructor, newObject, objects);


            //check if the objects in the output tuple violated basic contracts
            if (ret != null)
            {
                foreach (object o in ret.tuple)
                {
                    if (o == null) continue;

                    bool toStrViol, hashCodeViol, equalsViol;
                    int count;
                    if (Util.ViolatesContracts(o, out count, out toStrViol, out hashCodeViol, out equalsViol))
                    {
                        contractViolated = true;
                        contractExnViolatingMethods[constructor] = true;

                        bool newcontractViolation = false;
                        PlanManager.numDistinctContractViolPlans++;

                        if (toStrViol)
                        {

                            if (!toStrViolatingMethods.ContainsKey(constructor))
                                newcontractViolation = true;
                            toStrViolatingMethods[constructor] = true;
                        }
                        if (hashCodeViol)
                        {
                            if (!hashCodeViolatingMethods.ContainsKey(constructor))
                                newcontractViolation = true;

                            hashCodeViolatingMethods[constructor] = true;
                        }
                        if (equalsViol)
                        {
                            if (!equalsViolatingMethods.ContainsKey(constructor))
                                newcontractViolation = true;

                            equalsViolatingMethods[constructor] = true;
                        }

                        if (newcontractViolation)
                            PlanManager.numContractViolatingPlans++;

                        //add this class to the faulty classes
                        contractExnViolatingClasses[constructor.DeclaringType] = true;

                        retval = false;
                    }

                }

                contractStates = new RandoopContractsManager().ValidateAssertionContracts(constructor, newObject);
            }

            long endTime = 0;
            Timer.QueryPerformanceCounter(ref endTime);
            executionTimeAccum += ((double)(endTime - startTime)) / ((double)(Timer.PerfTimerFrequency));

            if (contractViolated)  //xiao.qu@us.abb.com adds
                executionLog.WriteLine("contract violation."); //xiao.qu@us.abb.com adds

            return retval;
        }

        public override string Namespace
        {
            get
            {
                return this.constructor.DeclaringType.Namespace;
            }
        }

        public override ReadOnlyCollection<Assembly> Assemblies
        {
            get
            {
                return ReflectionUtils.GetRelatedAssemblies(this.constructor);
            }
        }

    }

}
