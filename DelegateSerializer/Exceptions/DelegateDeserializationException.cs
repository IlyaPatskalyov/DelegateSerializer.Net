using System;

namespace DelegateSerializer.Exceptions
{
    public class DelegateDeserializationException : Exception
    {
        public DelegateDeserializationException(string message) : base(message)
        {
        }
    }
}