using Moq;
using System;
using System.Reflection;

namespace Upstream.Testing
{
    public abstract class TestBase<T> : IDisposable
        where T : class
    {
        protected abstract T CreateTestClass();

        protected virtual bool VerifyAll => true;

        protected virtual void VerifyMocks()
        {
            Type instanceType = this.GetType();

            var allFields = instanceType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            foreach (var field in allFields)
            {
                if (field?.FieldType?.BaseType == typeof(Mock))
                {
                    string verifyMethodName = VerifyAll ? nameof(Mock.VerifyAll) : nameof(Mock.Verify);
                    MethodInfo verifyMethod = field.FieldType.GetMethod(verifyMethodName, new Type[] { });

                    verifyMethod?.Invoke(field.GetValue(this), new object[] { });
                }
            }
        }

        public void Dispose()
        {
            VerifyMocks();
        }
    }
}
