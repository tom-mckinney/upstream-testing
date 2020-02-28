using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit.Sdk;

namespace Xunit
{
    public partial class Assert
    {
        /// <summary>
        /// Asserts that all properties of <paramref name="actual"/> have been mapped from <paramref name="expected"/>.
        /// </summary>
        /// <typeparam name="TOrigin">origin object type</typeparam>
        /// <typeparam name="TMap">mapped object type</typeparam>
        /// <param name="expected">origin object</param>
        /// <param name="actual">mapped object</param>
        /// <returns></returns>
        public static bool AllPropertiesMapped<TOrigin, TMap>(TOrigin expected, TMap actual)
        {
            return AllPropertiesMapped(expected, actual, new List<string>(), new Dictionary<string, string>());
        }

        /// <summary>
        /// Asserts that all properties of <paramref name="actual"/> have been mapped from <paramref name="expected"/>.
        /// All <paramref name="excludedProperties"/> will be excluded.
        /// </summary>
        /// <typeparam name="TOrigin">origin object type</typeparam>
        /// <typeparam name="TMap">mapped object type</typeparam>
        /// <param name="expected">origin object</param>
        /// <param name="actual">mapped object</param>
        /// <param name="excludedProperties">excluded properties</param>
        /// <returns></returns>
        public static bool AllPropertiesMapped<TOrigin, TMap>(TOrigin expected, TMap actual, params string[] excludedProperties)
        {
            return AllPropertiesMapped(expected, actual, excludedProperties, new Dictionary<string, string>());
        }

        /// <summary>
        /// Asserts that all properties of <paramref name="actual"/> have been mapped from <paramref name="expected"/>.
        /// All <paramref name="excludedProperties"/> will be excluded.
        /// </summary>
        /// <typeparam name="TOrigin">origin object type</typeparam>
        /// <typeparam name="TMap">mapped object type</typeparam>
        /// <param name="expected">origin object</param>
        /// <param name="actual">mapped object</param>
        /// <param name="excludedProperties">excluded properties</param>
        /// <returns></returns>
        public static bool AllPropertiesMapped<TOrigin, TMap>(TOrigin expected, TMap actual, IEnumerable<string> excludedProperties)
        {
            return AllPropertiesMapped(expected, actual, excludedProperties, new Dictionary<string, string>());
        }

        public static bool AllPropertiesMapped<TOrigin, TMap>(TOrigin expected, TMap actual, IDictionary<string, string> translatedProperties)
        {
            return AllPropertiesMapped(expected, actual, new string[] { }, translatedProperties);
        }

        /// <summary>
        /// Asserts that all properties of <paramref name="actual"/> have been mapped from <paramref name="expected"/>.
        /// All <paramref name="excludedProperties"/> will be excluded and any properties with a matching key in <paramref name="translatedProperties"/> will be translated to respective property in class <typeparamref name="TExpected"/>
        /// </summary>
        /// <typeparam name="TExpected">origin object type</typeparam>
        /// <typeparam name="TMap">mapped object type</typeparam>
        /// <param name="expected">origin object</param>
        /// <param name="actual">mapped object</param>
        /// <param name="excludedProperties">excluded properties</param>
        /// <param name="translatedProperties">translated properties</param>
        /// <returns>True or throws Xunit exception</returns>
        public static bool AllPropertiesMapped<TExpected, TMap>(TExpected expected, TMap actual, IEnumerable<string> excludedProperties, IDictionary<string, string> translatedProperties)
        {
            if (expected == null)
                throw new ArgumentNullException(nameof(expected));
            if (actual == null)
                throw new ArgumentNullException(nameof(actual));
            if (excludedProperties == null)
                throw new ArgumentNullException(nameof(excludedProperties));
            if (translatedProperties == null)
                throw new ArgumentNullException(nameof(translatedProperties));

            Type expectedType = typeof(TExpected);
            Type mappedType = typeof(TMap);
            PropertyInfo[] expectedProperties = expectedType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            PropertyInfo[] mappedProperties = mappedType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);


            List<Exception> exceptions = new List<Exception>();

            foreach (var expectedProp in expectedProperties)
            {
                if (excludedProperties.Contains(expectedProp.Name))
                    continue;

                if (!translatedProperties.TryGetValue(expectedProp.Name, out string propertyName))
                {
                    propertyName = expectedProp.Name;
                }

                PropertyInfo mappedProperty = null;
                try
                {
                    mappedProperty = Assert.Single(mappedProperties, p => p.Name == propertyName);
                }
                catch (Exception e)
                {
                    string message = FormatMappingErrorMessage($"Could not find matching property for {propertyName}.", typeof(TExpected), typeof(TMap));
                    exceptions.Add(new TestingException(message, e));
                    continue;
                }

                try
                {
                    Assert.Equal(expectedProp.GetValue(expected), mappedProperty.GetValue(actual));
                }
                catch (Exception e)
                {
                    string message = FormatMappingErrorMessage($"Value did not match for {propertyName}.", typeof(TExpected), typeof(TMap));
                    exceptions.Add(new TestingException(message, e));
                }
            }

            ThrowIfAny(exceptions);

            return true;
        }

        /// <summary>
        /// Asserts that all properties have a value that is neither null nor default
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="excludedProperties"></param>
        /// <returns></returns>
        public static bool NoPropertiesNullOrDefault<T>(T obj, params string[] excludedProperties)
        {
            Type objType = typeof(T);
            PropertyInfo[] objProperties = objType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

            List<Exception> exceptions = new List<Exception>();

            foreach (var prop in objProperties)
            {
                if (excludedProperties.Contains(prop.Name))
                    continue;

                try
                {
                    var value = prop.GetValue(obj);

                    Assert.NotNull(value);

                    object defaultValue = prop.PropertyType.IsValueType ? Activator.CreateInstance(prop.PropertyType) : null;
                    Assert.NotEqual(defaultValue, value);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }

            ThrowIfAny(exceptions);

            return true;
        }

        private static void ThrowIfAny(IEnumerable<Exception> exceptions)
        {
            if (exceptions.Any())
            {
                if (exceptions.Count() == 1)
                {
                    throw exceptions.ElementAt(0);
                }
                else
                {
                    throw new AggregateException(exceptions);
                }
            }
        }

        private static string FormatMappingErrorMessage(string message, Type originType, Type mapType)
        {
            return $"{message}\nFailed to map from {originType?.Name} to {mapType?.Name}.";
        }
    }
}
