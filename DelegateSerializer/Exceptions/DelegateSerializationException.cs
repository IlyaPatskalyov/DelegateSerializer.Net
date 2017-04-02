using System;

namespace DelegateSerializer.Exceptions
{
    public class DelegateSerializationException : Exception
    {
        public DelegateSerializationException(string message) : base(message)
        {
        }
    }
}