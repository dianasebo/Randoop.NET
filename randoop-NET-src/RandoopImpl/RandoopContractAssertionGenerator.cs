using Randoop.RandoopContracts;
using RandoopContracts;
using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Randoop
{
    public class RandoopContractAssertionGenerator
    {
        private const string PostconditionComment = "\r\n\t\t\t//Contract assertion generated from postcondition\r\n";
        private const string InvalidPostconditionComment = "\r\n\t\t\t//Specified postcondition was invalid so no assertion was generated\r\n";
        private const string InvariantComment = "\r\n\t\t\t//Contract assertion generated from invariant\r\n";
        private const string InvalidInvariantComment = "\r\n\t\t\t//Specified invariant was invalid so no assertion was generated\r\n";
        private const string StaticInvariantComment = "\r\n\t\t\t//Contract assertion generated from static invariant\r\n";
        private const string InvalidStaticInvariantComment = "\r\n\t\t\t//Specified static invariant was invalid so no assertion was generated\r\n";

        public string Compute(MethodInfo method, string methodCallResult, string methodCallReceiver, ContractState contractStates)
        {
            var postcondition = method.GetCustomAttribute(typeof(Postcondition)) as Postcondition;
            var invariant = method.DeclaringType.GetCustomAttribute(typeof(Invariant)) as Invariant;
            var staticInvariant = method.DeclaringType.GetCustomAttribute(typeof(StaticInvariant)) as StaticInvariant;

            StringBuilder code = new StringBuilder();

            AppendPostcondition(contractStates.PostconditionState, methodCallResult, postcondition, code);
            AppendInvariant(contractStates.InvariantState, methodCallReceiver, method.DeclaringType, invariant, code);
            AppendStaticInvariant(method.DeclaringType, contractStates.StaticInvariantState, staticInvariant, code);

            return code.ToString();
        }

        private void AppendPostcondition(ContractStateEnum postconditionState, string methodCallResult, Postcondition postcondition, StringBuilder code)
        {
            if (postconditionState == ContractStateEnum.Ok)
            {
                code.Append(PostconditionComment);
                var expression = ComputePostconditionExpression(postcondition.Expression, methodCallResult);
                code.Append(Assertion(expression));
            }

            if (postconditionState == ContractStateEnum.Invalid)
            {
                code.Append(InvalidPostconditionComment);
            }
        }

        private void AppendInvariant(ContractStateEnum invariantState, string methodCallReceiver, Type declaringType, Invariant invariant, StringBuilder code)
        {
            if (invariantState == ContractStateEnum.Ok)
            {
                code.Append(InvariantComment);
                var expression = ComputeInvariantExpression(invariant.Expression, invariant.Fields, methodCallReceiver, declaringType);
                code.Append(Assertion(expression));
            }

            if (invariantState == ContractStateEnum.Invalid)
            {
                code.Append(InvalidInvariantComment);
            }
        }

        private void AppendStaticInvariant(Type declaringType, ContractStateEnum staticInvariantState, StaticInvariant staticInvariant, StringBuilder code)
        {
            if (staticInvariantState == ContractStateEnum.Ok)
            {
                code.Append(StaticInvariantComment);
                var expression = ComputeStaticInvariantExpression(staticInvariant.Expression, staticInvariant.Fields, declaringType);
                code.Append(Assertion(expression));
            }

            if (staticInvariantState == ContractStateEnum.Invalid)
            {
                code.Append(InvalidStaticInvariantComment);
            }
        }

        private string Assertion(string expression)
        {
            return "\t\t\tAssert.IsTrue(" + expression + ");";
        }

        private string ComputePostconditionExpression(string expression, string methodCallResult)
        {
            return expression.Replace("output", methodCallResult);
        }

        private string ComputeInvariantExpression(string expression, string[] fields, string methodCallReceiver, Type declaringType)
        {
            var orderedFields = fields.OrderByDescending(_ => _.Length).ToList();
            foreach (var fieldName in orderedFields)
            {
                if (declaringType.GetField(fieldName) != null)
                {
                    expression = expression.Replace(fieldName, methodCallReceiver + "." + fieldName);
                }
                else if (declaringType.GetMethod("Get" + char.ToUpper(fieldName[0]) + fieldName.Substring(1)) != null)
                {
                    expression = expression.Replace(fieldName, methodCallReceiver + ".Get" + char.ToUpper(fieldName[0]) + fieldName.Substring(1) + "()");
                }
                else
                {
                    throw new InvalidRandoopContractException("dafuq son");
                }
            }

            return expression;
        }

        private string ComputeStaticInvariantExpression(string expression, string[] fields, Type declaringType)
        {
            var orderedFields = fields.OrderByDescending(_ => _.Length).ToList();
            foreach (var fieldName in fields)
            {
                if (declaringType.GetField(fieldName) != null)
                {
                    expression = expression.Replace(fieldName, declaringType + "." + fieldName);
                }
                else if (declaringType.GetMethod("Get" + char.ToUpper(fieldName[0]) + fieldName.Substring(1)) != null)
                {
                    expression = expression.Replace(fieldName, declaringType + ".Get" + char.ToUpper(fieldName[0]) + fieldName.Substring(1) + "()");
                }
                else
                {
                    throw new InvalidRandoopContractException("dafuq son");
                }
            }

            return expression;
        }



        public string Compute(ConstructorInfo constructor, string newValueName, ContractState contractStates)
        {
            var postcondition = constructor.GetCustomAttribute(typeof(Postcondition)) as Postcondition;
            var invariant = constructor.DeclaringType.GetCustomAttribute(typeof(Invariant)) as Invariant;
            var staticInvariant = constructor.DeclaringType.GetCustomAttribute(typeof(StaticInvariant)) as StaticInvariant;

            StringBuilder code = new StringBuilder();
            AppendPostcondition(contractStates.PostconditionState, newValueName, postcondition, code);
            AppendInvariant(contractStates.InvariantState, newValueName, constructor.DeclaringType, invariant, code);
            AppendStaticInvariant(constructor.DeclaringType, contractStates.StaticInvariantState, staticInvariant, code);

            return code.ToString();
        }
    }
}
