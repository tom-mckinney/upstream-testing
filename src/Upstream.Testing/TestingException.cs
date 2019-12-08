using System;

namespace Xunit.Sdk
{
    public class TestingException : XunitException
    {
        public TestingException()
        {
        }

        public TestingException(string message)
            : base(message)
        {
        }

        public TestingException(string message, Exception e)
            : base(message, e)
        {
        }
    }
}
