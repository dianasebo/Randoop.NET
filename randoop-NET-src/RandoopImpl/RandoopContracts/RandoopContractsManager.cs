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
        public bool PreconditionViolated(MethodInfo method, object[] objects)
        {
            var arguments = objects.Select(_ => _.ToString()).OrderByDescending(_ => _.Length).ToList();
            var methodParameterNames = method.GetParameters().Select(_ => _.Name).ToList();
            var precondition = method.GetCustomAttribute(typeof(Precondition)) as Precondition;
            var computedExpression = precondition.Expression;

            for (int index = 0; index < arguments.Count; index++)
            {
                var parameterName = methodParameterNames.SingleOrDefault(_ => _.Equals(arguments[index]));
                if (parameterName == null)
                {
                    throw new InvalidRandoopContractException();
                }

                computedExpression = computedExpression.Replace(parameterName, arguments[index]);
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

        public ContractAssertion ValidateAssertionContracts(MethodInfo method, object receiver, object returnValue)
        {
            return new ContractAssertion()
            {
                FromPostcondition = ValidatePostcondition(method, returnValue),
                FromInvariant = ValidateInvariant(method, receiver),
                FromStaticInvariant = ValidateStaticInvariant(method)
            };
        }

        private bool ValidatePostcondition(MethodInfo method, object returnValue)
        {
            var postcondition = method.GetCustomAttribute(typeof(Postcondition)) as Postcondition;
            if (postcondition == null)
            {
                return false;
            }

            var expressionEvaluator = new ExpressionEvaluator
            {
                Variables = new Dictionary<string, object>()
                {
                    {"output", returnValue }
                }
            };

            try
            {
                expressionEvaluator.Evaluate<bool>(postcondition.Expression);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private bool ValidateInvariant(MethodInfo method, object receiver)
        {
            if (!(method.DeclaringType.GetCustomAttribute(typeof(Invariant)) is Invariant invariant))
            {
                return false;
            }

            var variables = new Dictionary<string, object>() { { "receiver", receiver } };
            var computedExpression = invariant.Expression;
            var orderedFields = invariant.Fields.OrderByDescending(_ => _.Length).ToList();
            foreach (var field in orderedFields)
            {
                var classField = method.DeclaringType.GetField(field);
                if (classField == null)
                {
                    return false;
                }

                if (classField.IsStatic)
                {
                    variables.Add(field, method.DeclaringType.GetField(field).GetValue(null));
                }
                else
                {
                    computedExpression = computedExpression.Replace(field, "receiver." + field);
                }
            }

            var expressionEvaluator = new ExpressionEvaluator() { Variables = variables };

            try
            {
                expressionEvaluator.Evaluate<bool>(computedExpression);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private bool ValidateStaticInvariant(MethodInfo method)
        {
            var staticInvariant = method.DeclaringType.GetCustomAttribute(typeof(StaticInvariant)) as StaticInvariant;
            if (staticInvariant == null)
            {
                return false;
            }

            var variables = new Dictionary<string, object>();
            var orderedFields = staticInvariant.Fields.OrderByDescending(_ => _.Length).ToList();
            foreach (var field in orderedFields)
            {
                var classField = method.DeclaringType.GetField(field);
                if (classField == null || classField.IsStatic == false)
                {
                    return false;
                }

                variables.Add(field, method.DeclaringType.GetField(field).GetValue(null));
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
                return false;
            }

            return true;
        }
    }
}
