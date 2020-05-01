//*********************************************************
//  NOTE: 
// xiao.qu@us.abb.com addes it for handling mapped_methods.txt
// format error
//
//  Date: 2012/07/03 
//*********************************************************


using System;

namespace Common
{
    [Serializable]
    public class InvalidFileFormatException: Exception
    {
        public InvalidFileFormatException(string message)
            : base("Method Mapping File Format is invalid: " + message)
        {
        }

    }
}
