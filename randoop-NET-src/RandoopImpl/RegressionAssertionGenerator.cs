using System;
using System.Linq;
using System.Text;

namespace Randoop
{
    public class RegressionAssertionGenerator
    {
        public string GenerateRegressionAssertion(object retVal, string newValueName, int timesReturnValRetrieved) //xiao.qu@us.abb.com adds
        {
            StringBuilder code = new StringBuilder();
            if (retVal != null) //CASE2
            {
                code.Append("\r\n\t\t\t//Regression assertion (captures the current behavior of the code)\r\n");

                if (retVal.GetType() == typeof(System.String))
                {
                    string temp = retVal.ToString().Replace("\\", "\\\\").Replace("\n", "\\n").Replace("\r", "\\r");
                    temp = temp.Replace("\"", "\\\"");
                    code.Append("\t\t\tAssert.AreEqual(" + "\"" + temp + "\", " + newValueName +
                        ",\"Regression Failure? [" + timesReturnValRetrieved.ToString() + "]\");\r\n");

                    return code.ToString();
                }
                else //not string
                {
                    var primitiveTypes = new Type[] { typeof(bool), typeof(byte), typeof(short), typeof(int), typeof(long), typeof(float), typeof(double), typeof(char) };

                    if (primitiveTypes.Contains(retVal.GetType()))
                    {
                        code.Append(ComputeAssertion(retVal, newValueName, timesReturnValRetrieved));
                    }
                    else
                    {
                        code.Append("\t\t\tAssert.IsNotNull(" + newValueName + ");\r\n");
                    }

                    return code.ToString();
                }

            }
            else //return value is null or execution throws exceptions
            {
                return string.Empty;
            }
        }

        private string ComputeAssertion(object retVal, string newValueName, int timesReturnValRetrived)
        {
            return "\t\t\tAssert.AreEqual(" + retVal.ToString().ToLower() + ", "
                            + newValueName + ",\"Regression Failure? [" + timesReturnValRetrived.ToString() + "]\");\r\n";
        }
    }
}
