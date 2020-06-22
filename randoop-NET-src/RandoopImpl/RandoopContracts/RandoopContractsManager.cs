using CodingSeb.ExpressionEvaluator;
using RandoopContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Randoop.RandoopContracts
{
    public class RandoopContractsManager
    {
        public ContractState ValidateAssertionContracts(MethodInfo method, object receiver, object returnedValue)
        {
            ContractState contractState = new ContractState();
            if (MethodIsNonStaticAndNonVoid(method))
            {
                contractState.PostconditionState = ValidatePostcondition(method, returnedValue);
                contractState.InvariantState = ValidateInvariant(method.DeclaringType, receiver);
                contractState.StaticInvariantState = ValidateStaticInvariant(method.DeclaringType);
            }
            else if (MethodIsNonStaticAndVoid(method))
            {
                contractState.PostconditionState = ContractStateEnum.NotApplicable;
                contractState.InvariantState = ValidateInvariant(method.DeclaringType, receiver);
                contractState.StaticInvariantState = ValidateStaticInvariant(method.DeclaringType);
            }
            else if (MethodIsStaticAndNonVoid(method))
            {
                contractState.PostconditionState = ValidatePostcondition(method, returnedValue);
                contractState.InvariantState = ContractStateEnum.NotApplicable;
                contractState.StaticInvariantState = ValidateStaticInvariant(method.DeclaringType);
            }
            else if (MethodIsStaticAndVoid(method))
            {
                contractState.PostconditionState = ContractStateEnum.NotApplicable;
                contractState.InvariantState = ContractStateEnum.NotApplicable;
                contractState.StaticInvariantState = ValidateStaticInvariant(method.DeclaringType);
            }
            return contractState;
        }

        public ContractState ValidateAssertionContracts(ConstructorInfo constructor, object returnedValue)
        {
            ContractState contractState = new ContractState
            {
                PostconditionState = ValidatePostcondition(constructor, returnedValue),
                InvariantState = ValidateInvariant(constructor.DeclaringType, returnedValue),
                StaticInvariantState = ValidateStaticInvariant(constructor.DeclaringType)
            };

            return contractState;
        }

        private ContractStateEnum ValidatePostcondition(MethodBase method, object returnedValue)
        {
            if (!(method.GetCustomAttribute(typeof(Postcondition)) is Postcondition postcondition))
            {
                return ContractStateEnum.Missing;
            }

            var expressionEvaluator = new ExpressionEvaluator
            {
                Variables = new Dictionary<string, object>()
                {
                    {"output", returnedValue }
                }
            };

            try
            {
                expressionEvaluator.Evaluate<bool>(postcondition.Expression);
            }
            catch (Exception)
            {
                return ContractStateEnum.Invalid;
            }

            return ContractStateEnum.Ok;
        }

        private ContractStateEnum ValidateInvariant(Type declaringType, object receiver)
        {
            if (!(declaringType.GetCustomAttribute(typeof(Invariant)) is Invariant invariant))
            {
                return ContractStateEnum.Missing;
            }

            var variables = new Dictionary<string, object>();
            foreach (var fieldName in invariant.Fields)
            {
                var classField = declaringType.GetField(fieldName);
                if (classField == null)
                {
                    var getter = declaringType.GetMethod(ComputeGetterName(fieldName));
                    if (getter == null)
                    {
                        return ContractStateEnum.Invalid;
                    }
                    else
                    {
                        variables.Add(fieldName, getter.Invoke(receiver, Array.Empty<object>()));
                    }
                }
                else
                {
                    variables.Add(fieldName, classField.GetValue(classField.IsStatic ? null : receiver));
                }
            }

            var expressionEvaluator = new ExpressionEvaluator() { Variables = variables };

            try
            {
                expressionEvaluator.Evaluate<bool>(invariant.Expression);
            }
            catch (Exception)
            {
                return ContractStateEnum.Invalid;
            }

            return ContractStateEnum.Ok;
        }

        private ContractStateEnum ValidateStaticInvariant(Type declaringType)
        {
            var staticInvariant = declaringType.GetCustomAttribute(typeof(StaticInvariant)) as StaticInvariant;
            if (staticInvariant == null)
            {
                return ContractStateEnum.Missing;
            }

            var variables = new Dictionary<string, object>();
            foreach (var fieldName in staticInvariant.Fields)
            {
                var classField = declaringType.GetField(fieldName);
                if (classField == null)
                {
                    var getter = declaringType.GetMethod(ComputeGetterName(fieldName));
                    if (getter == null || getter.IsStatic == false)
                    {
                        return ContractStateEnum.Invalid;
                    }
                    else
                    {
                        variables.Add(fieldName, getter.Invoke(null, Array.Empty<object>()));
                    }
                }
                else
                {
                    if (classField.IsStatic == false)
                    {
                        return ContractStateEnum.Invalid;
                    }
                    else
                    {
                        variables.Add(fieldName, classField.GetValue(null));
                    }
                }

            }

            var expressionEvaluator = new ExpressionEvaluator()
            {
                Variables = variables
            };

            try
            {
                expressionEvaluator.Evaluate<bool>(staticInvariant.Expression);
            }
            catch (Exception)
            {
                return ContractStateEnum.Invalid;
            }

            return ContractStateEnum.Ok;
        }

        private bool MethodIsNonStaticAndNonVoid(MethodInfo method)
        {
            return method.IsStatic == false && method.ReturnType.Equals(typeof(void)) == false;
        }

        private bool MethodIsNonStaticAndVoid(MethodInfo method)
        {
            return method.IsStatic == false && method.ReturnType.Equals(typeof(void));
        }

        private bool MethodIsStaticAndNonVoid(MethodInfo method)
        {
            return method.IsStatic && method.ReturnType.Equals(typeof(void)) == false;
        }

        private bool MethodIsStaticAndVoid(MethodInfo method)
        {
            return method.IsStatic && method.ReturnType.Equals(typeof(void));
        }

        private static string ComputeGetterName(string field)
        {
            return "Get" + char.ToUpper(field[0]) + field.Substring(1);
        }

        public bool PreconditionViolated(MethodInfo method, object[] arguments)
        {
            var orderedArguments = arguments.Select(_ => _.ToString()).OrderByDescending(_ => _.Length).ToList();
            var methodParameterNames = method.GetParameters().Select(_ => _.Name).ToList();
            var precondition = method.GetCustomAttribute(typeof(Precondition)) as Precondition;

            if (precondition == null)
            {
                return false;
            }

            var computedExpression = precondition.Expression;

            for (int index = 0; index < orderedArguments.Count; index++)
            {
                var parameterName = methodParameterNames.SingleOrDefault(_ => _.Equals(orderedArguments[index]));
                if (parameterName == null)
                {
                    throw new InvalidRandoopContractException();
                }

                computedExpression = computedExpression.Replace(parameterName, orderedArguments[index]);
            }

            try
            {
                return new ExpressionEvaluator().Evaluate<bool>(computedExpression) == false;
            }
            catch (Exception)
            {
                throw new InvalidRandoopContractException();
            }
        }

        public bool PreconditionViolated(ConstructorInfo constructor, object[] arguments)
        {
            var orderedArguments = arguments.Select(_ => _.ToString()).OrderByDescending(_ => _.Length).ToList();
            var methodParameterNames = constructor.GetParameters().Select(_ => _.Name).ToList();
            var precondition = constructor.GetCustomAttribute(typeof(Precondition)) as Precondition;

            if (precondition == null)
            {
                return false;
            }

            var computedExpression = precondition.Expression;

            for (int index = 0; index < orderedArguments.Count; index++)
            {
                var parameterName = methodParameterNames.SingleOrDefault(_ => _.Equals(orderedArguments[index]));
                if (parameterName == null)
                {
                    throw new InvalidRandoopContractException();
                }

                computedExpression = computedExpression.Replace(parameterName, orderedArguments[index]);
            }

            try
            {
                return new ExpressionEvaluator().Evaluate<bool>(computedExpression) == false;
            }
            catch (Exception)
            {
                throw new InvalidRandoopContractException();
            }
        }
    }
}
