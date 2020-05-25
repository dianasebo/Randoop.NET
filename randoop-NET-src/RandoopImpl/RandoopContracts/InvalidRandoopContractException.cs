using System;

namespace Randoop.RandoopContracts
{
    public class InvalidRandoopContractException : Exception
    {
        public InvalidRandoopContractException() { }

        public InvalidRandoopContractException(string message) : base(message) { }

        public InvalidRandoopContractException(string message, Exception innerException) : base(message, innerException) { }
    }
}
