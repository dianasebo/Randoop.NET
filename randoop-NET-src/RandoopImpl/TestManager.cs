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
using System.IO;

namespace Randoop
{

    public class PlanManager
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        RandoopConfiguration config;

        // Counter used to print out a dot for every 1000 plans.
        private int addCounter = 0;

        private int redundantAdds = 0;

        public int Plans
        {
            get
            {
                return addCounter;
            }
        }

        public int RedundantAdds
        {
            get
            {
                return redundantAdds;
            }
        }

        public readonly PlanDataBase exceptionPlans;
        public readonly PlanDataBase builderPlans;
        public readonly PlanDataBase observerPlans;

        private ITestFileWriter testFileWriter;

        public static int numContractViolatingPlans = 0; //to count number of exception + contract violating plans
        public static int numDistinctContractViolPlans = 0; // counts the # of plans that violates contract (may violate same contract)

        public PlanManager(RandoopConfiguration config)
        {
            this.config = config;
            builderPlans = new PlanDataBase("builderPlans", config.typematchingmode);
            exceptionPlans = new PlanDataBase("exceptionThrowingPlans", config.typematchingmode);
            observerPlans = new PlanDataBase("observerPlans", config.typematchingmode);

            Plan.uniqueIdCounter = config.planstartid;

            switch (config.directoryStrategy)
            {
                case DirectoryStrategy.Single:
                    testFileWriter = new SingleDirTestWriter(new DirectoryInfo(config.outputdir), config.useRandoopContracts);
                    break;
                case DirectoryStrategy.ClassifyingByBehavior:
                    testFileWriter = new ClassifyingByBehaviorTestFileWriter(new DirectoryInfo(config.outputdir), config.useRandoopContracts);
                    break;
                case DirectoryStrategy.ClassifyingByClass:
                    testFileWriter = new ClassifyingByClassTestFileWriter(new DirectoryInfo(config.outputdir), config.useRandoopContracts);
                    break;
            }
        }

        /// <summary>
        /// Adds (if not already present) p to either fplanDB or fexceptionThrowingPlanDB,
        /// by executing the plan to determine if it throws exceptions.
        /// </summary>
        /// <param name="v"></param>
        public void AddMaybeExecutingIfNeeded(Plan plan, StatsManager stats)
        {
            if (builderPlans.Containsplan(plan))
            {
                redundantAdds++;
                stats.CreatedNew(CreationResult.Redundant);
            }
            else if (exceptionPlans.Containsplan(plan))
            {
                redundantAdds++;
                stats.CreatedNew(CreationResult.Redundant);
            }
            else
            {
                addCounter++;
                if (addCounter % 1000 == 0)
                {
                    Console.Write(".");
                }

                stats.CreatedNew(CreationResult.New);
                ResultTuple execResult;

                TextWriter writer = new StringWriter();
                ////xiao.qu@us.abb.com changes for debuglog
                //TextWriter writer
                //    = new StreamWriter("..\\..\\TestRandoopBare\\1ab1qkwm.jbd\\debug.log"); 

                Exception exceptionThrown;
                bool contractViolated;
                bool preconditionViolated;

                testFileWriter.WriteTest(plan);

                if (config.executionmode == ExecutionMode.DontExecute)
                {
                    builderPlans.AddPlan(plan);
                    stats.ExecutionResult("normal");
                }
                else
                {
                    Util.Assert(config.executionmode == ExecutionMode.Reflection);

                    //TextWriter executionLog = new StreamWriter(config.executionLog);
                    TextWriter executionLog = new StreamWriter(config.executionLog + addCounter.ToString() + ".log"); //xiao.qu@us.abb.com changes
                    executionLog.WriteLine("LASTPLANID:" + plan.uniqueId);

                    long startTime = 0;
                    Timer.QueryPerformanceCounter(ref startTime);

                    bool execSucceeded = plan.Execute(out execResult,
                        executionLog, writer, out preconditionViolated, out exceptionThrown,
                        out contractViolated, config.forbidnull,
                        config.useRandoopContracts);

                    long endTime = 0;
                    Timer.QueryPerformanceCounter(ref endTime);
                    TimeTracking.timeSpentExecutingTestedCode += (endTime - startTime);

                    executionLog.Close();

                    if (!execSucceeded)
                    {
                        if (preconditionViolated)
                        {
                            stats.ExecutionResult("precondition violated");
                        }

                        if (exceptionThrown != null)
                        {
                            stats.ExecutionResult(exceptionThrown.GetType().FullName);
                            plan.exceptionThrown = exceptionThrown;
                            testFileWriter.Move(plan, exceptionThrown);

                            if (exceptionThrown is AccessViolationException)
                            {
                                Logger.Error("SECOND-CHANCE ACCESS VIOLATION EXCEPTION.");
                                Environment.Exit(1);
                            }
                        }


                        //string exceptionMessage = writer.ToString(); //no use? xiao.qu@us.abb.com comments out

                        Util.Assert(plan.exceptionThrown != null || contractViolated || preconditionViolated);

                        if (config.monkey)
                        {
                            builderPlans.AddPlan(plan);
                        }
                        else if (exceptionThrown != null)
                        {
                            exceptionPlans.AddPlan(plan);
                        }
                    }
                    else
                    {
                        stats.ExecutionResult("normal");

                        if (config.outputnormalinputs)
                        {
                            testFileWriter.MoveNormalTermination(plan);
                        }
                        else
                        {
                            testFileWriter.Remove(plan);
                        }

                        // If forbidNull, then make inactive any result tuple elements that are null.
                        if (config.forbidnull)
                        {
                            Util.Assert(plan.NumTupleElements == execResult.tuple.Length);
                            for (int i = 0; i < plan.NumTupleElements; i++)
                                if (execResult.tuple[i] == null)
                                    plan.SetActiveTupleElement(i, false);
                            //Util.Assert(!allNull); What is the motivation behind this assertion?
                        }


                        //only allow the receivers to be arguments to future methods
                        if (config.forbidparamobj)
                        {
                            Util.Assert(plan.NumTupleElements == execResult.tuple.Length);
                            for (int i = 1; i < plan.NumTupleElements; i++)
                                plan.SetActiveTupleElement(i, false);

                        }

                        builderPlans.AddPlan(plan, execResult);
                    }
                }
            }
        }

        //TODO Diana: delete this maybe?
        //private void PrintPercentageExecuted()
        //{
        //    long endTime = 0;
        //    Timer.QueryPerformanceCounter(ref endTime);

        //    long totalGenerationTime = endTime - TimeTracking.generationStartTime;

        //    double ratioExecToGen =
        //        (double)TimeTracking.timeSpentExecutingTestedCode / (double)totalGenerationTime;
        //}
    }
}
