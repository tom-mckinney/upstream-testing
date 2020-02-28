using Moq;
using System;
using System.Collections.Generic;
using System.Reflection;
using Upstream.Testing.Extensions;

namespace Upstream.Testing
{
    public abstract class TestBase<T> : IDisposable
        where T : class
    {
        protected virtual bool VerifyAll => true;

        protected virtual MockRepository MockRepository { get; } = new MockRepository(MockBehavior.Strict);

        protected abstract T CreateTestClass();

        protected virtual void VerifyMocks()
        {
            var exceptions = new List<Exception>();

            Type instanceType = this.GetType();

            var allFields = instanceType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            foreach (var field in allFields)
            {
                if (field?.FieldType?.BaseType == typeof(Mock))
                {
                    string verifyMethodName = VerifyAll ? nameof(Mock.VerifyAll) : nameof(Mock.Verify);
                    MethodInfo verifyMethod = field.FieldType.GetMethod(verifyMethodName, new Type[] { });

                    try
                    {
                        verifyMethod?.Invoke(field.GetValue(this), new object[] { });
                    }
                    catch (Exception e)
                    {
                        exceptions.Add(e);
                    }
                }
            }

            try
            {
                if (VerifyAll)
                    MockRepository?.VerifyAll();
                else
                    MockRepository?.Verify();
            }
            catch (Exception e)
            {
                exceptions.Add(e);
            }

            exceptions.ThrowIfAny();
        }

        public void Dispose()
        {
            VerifyMocks();
        }
    }
}
