using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Upstream.Testing.Extensions
{
    public static class ExceptionExtensions
    {
        public static void ThrowIfAny(this IEnumerable<Exception> exceptions)
        {
            var count = exceptions?.Count();

            if (count > 0)
            {
                if (count == 1)
                {
                    throw exceptions.First();
                }
                else
                {
                    throw new AggregateException(exceptions);
                }
            }
        }
    }
}
