using RandoopContracts;
using System;
using System.Reflection;
using System.Text;

namespace Randoop
{
    public class ContractAssertionGenerator
    {
        private readonly MethodInfo method;
        private const string PostconditionComment = "\r\n\t\t\t// Contract assertion generated from postcondition\r\n";
        private const string InvariantComment = "\r\n\t\t\t// Contract assertion generated from invariant\r\n";
        private const string StaticInvariantComment = "\r\n\t\t\t// Contract assertion generated from static invariant\r\n";

        public ContractAssertionGenerator(MethodInfo method)
        {
            this.method = method;
        }

        public string Compute(string methodCallResult, string methodCallReceiver)
        {
            var postcondition = method.GetCustomAttribute(typeof(Postcondition)) as Postcondition;
            var invariant = method.DeclaringType.GetCustomAttribute(typeof(Invariant)) as Invariant;
            var staticInvariant = method.DeclaringType.GetCustomAttribute(typeof(StaticInvariant)) as StaticInvariant;

            StringBuilder code = new StringBuilder();

            if (MethodIsNonStaticAndNonVoid())
            {
                AppendPostcondition(methodCallResult, postcondition, code);
                AppendInvariant(methodCallReceiver, invariant, code);
                AppendStaticInvariant(staticInvariant, code);
            }
            else if (MethodIsNonStaticAndVoid())
            {
                AppendInvariant(methodCallReceiver, invariant, code);
                AppendStaticInvariant(staticInvariant, code);
            }
            else if (MethodIsStaticAndNonVoid())
            {
                AppendPostcondition(methodCallResult, postcondition, code);
                AppendStaticInvariant(staticInvariant, code);
            }
            else if (MethodIsStaticAndVoid())
            {
                AppendStaticInvariant(staticInvariant, code);
            }

            return code.ToString();
        }

        private void AppendPostcondition(string methodCallResult, Postcondition postcondition, StringBuilder code)
        {
            if (postcondition != null)
            {
                code.Append(PostconditionComment);
                var expression = ComputePostconditionExpression(postcondition.Expression, methodCallResult);
                code.Append(Assertion(expression));
            }
        }

        private void AppendInvariant(string methodCallReceiver, Invariant invariant, StringBuilder code)
        {
            if (invariant != null)
            {
                code.Append(InvariantComment);
                var expression = ComputeInvariantExpression(invariant.Expression, invariant.Fields, method.DeclaringType, methodCallReceiver);
                code.Append(Assertion(expression));
            }
        }

        private void AppendStaticInvariant(StaticInvariant staticInvariant, StringBuilder code)
        {
            if (staticInvariant != null)
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
            foreach (var fieldName in fields)
            {
                if (declaringType.GetField(fieldName) != null)
                {
                    expression = expression.Replace(fieldName, methodCallReceiver + "." + fieldName);
                }
                else
                {
                    return string.Empty;
                }

            }

            return expression;
        }

        private string ComputeStaticInvariantExpression(string expression, string[] fields, Type declaringType)
        {
            foreach (var fieldName in fields)
            {
                if (declaringType.GetField(fieldName) != null)
                {
                    expression = expression.Replace(fieldName, declaringType + "." + fieldName);
                }
                else
                {
                    return string.Empty;
                }

            }

            return expression;
        }

        private bool MethodIsNonStaticAndNonVoid()
        {
            return method.IsStatic == false && method.ReturnType.Equals(typeof(void)) == false;
        }

        private bool MethodIsNonStaticAndVoid()
        {
            return method.IsStatic == false && method.ReturnType.Equals(typeof(void));
        }

        private bool MethodIsStaticAndNonVoid()
        {
            return method.IsStatic && method.ReturnType.Equals(typeof(void)) == false;
        }

        private bool MethodIsStaticAndVoid()
        {
            return method.IsStatic && method.ReturnType.Equals(typeof(void));
        }
    }
}
