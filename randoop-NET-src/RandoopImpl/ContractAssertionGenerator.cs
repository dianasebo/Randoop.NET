using Randoop.RandoopContracts;
using RandoopContracts;
using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Randoop
{
    public class ContractAssertionGenerator
    {
        private const string PostconditionComment = "\r\n\t\t\t//Contract assertion generated from postcondition\r\n";
        private const string InvariantComment = "\r\n\t\t\t//Contract assertion generated from invariant\r\n";
        private const string StaticInvariantComment = "\r\n\t\t\t//Contract assertion generated from static invariant\r\n";

        public string Compute(MethodInfo method, string methodCallResult, string methodCallReceiver, ContractAssertion canGenerateContractAssertion)
        {
            var postcondition = method.GetCustomAttribute(typeof(Postcondition)) as Postcondition;
            var invariant = method.DeclaringType.GetCustomAttribute(typeof(Invariant)) as Invariant;
            var staticInvariant = method.DeclaringType.GetCustomAttribute(typeof(StaticInvariant)) as StaticInvariant;

            StringBuilder code = new StringBuilder();

            if (MethodIsNonStaticAndNonVoid(method))
            {
                AppendPostcondition(canGenerateContractAssertion.FromPostcondition, methodCallResult, postcondition, code);
                AppendInvariant(method, canGenerateContractAssertion.FromInvariant, methodCallReceiver, invariant, code);
                AppendStaticInvariant(method, canGenerateContractAssertion.FromStaticInvariant, staticInvariant, code);
            }
            else if (MethodIsNonStaticAndVoid(method))
            {
                AppendInvariant(method, canGenerateContractAssertion.FromInvariant, methodCallReceiver, invariant, code);
                AppendStaticInvariant(method, canGenerateContractAssertion.FromStaticInvariant, staticInvariant, code);
            }
            else if (MethodIsStaticAndNonVoid(method))
            {
                AppendPostcondition(canGenerateContractAssertion.FromPostcondition, methodCallResult, postcondition, code);
                AppendStaticInvariant(method, canGenerateContractAssertion.FromStaticInvariant, staticInvariant, code);
            }
            else if (MethodIsStaticAndVoid(method))
            {
                AppendStaticInvariant(method, canGenerateContractAssertion.FromStaticInvariant, staticInvariant, code);
            }

            return code.ToString();
        }

        private void AppendPostcondition(bool canGenerate, string methodCallResult, Postcondition postcondition, StringBuilder code)
        {
            if (canGenerate)
            {
                code.Append(PostconditionComment);
                var expression = ComputePostconditionExpression(postcondition.Expression, methodCallResult);
                code.Append(Assertion(expression));
            }
        }

        private void AppendInvariant(MethodBase method, bool canGenerate, string methodCallReceiver, Invariant invariant, StringBuilder code)
        {
            if (canGenerate)
            {
                code.Append(InvariantComment);
                var expression = ComputeInvariantExpression(invariant.Expression, invariant.Fields, method.DeclaringType, methodCallReceiver);
                code.Append(Assertion(expression));
            }
        }

        private void AppendStaticInvariant(MethodBase method, bool canGenerate, StaticInvariant staticInvariant, StringBuilder code)
        {
            if (canGenerate)
            {
                code.Append(StaticInvariantComment);
                var expression = ComputeStaticInvariantExpression(staticInvariant.Expression, staticInvariant.Fields, method.DeclaringType);
                code.Append(Assertion(expression));
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

        private string ComputeInvariantExpression(string expression, string[] fields, Type declaringType, string methodCallReceiver)
        {
            var orderedFields = fields.OrderByDescending(_ => _.Length).ToList();
            foreach (var fieldName in orderedFields)
            {
                expression = expression.Replace(fieldName, methodCallReceiver + "." + fieldName);
            }

            return expression;
        }

        private string ComputeStaticInvariantExpression(string expression, string[] fields, Type declaringType)
        {
            var orderedFields = fields.OrderByDescending(_ => _.Length).ToList();
            foreach (var fieldName in fields)
            {
                expression = expression.Replace(fieldName, declaringType + "." + fieldName);
            }

            return expression;
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

        internal string Compute(ConstructorInfo constructor, string newValueName, ContractAssertion canGenerateContractAssertion)
        {
            var postcondition = constructor.GetCustomAttribute(typeof(Postcondition)) as Postcondition;
            var invariant = constructor.DeclaringType.GetCustomAttribute(typeof(Invariant)) as Invariant;
            var staticInvariant = constructor.DeclaringType.GetCustomAttribute(typeof(StaticInvariant)) as StaticInvariant;

            StringBuilder code = new StringBuilder();
            AppendPostcondition(canGenerateContractAssertion.FromPostcondition, newValueName, postcondition, code);
            AppendInvariant(constructor, canGenerateContractAssertion.FromInvariant, newValueName, invariant, code);
            AppendStaticInvariant(constructor, canGenerateContractAssertion.FromStaticInvariant, staticInvariant, code);

            return code.ToString();
        }
    }
}
