using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Sdk;

namespace FluentAssertions
{
    public static class PropertyMatchAssertions
    {
        public static AndConstraint<ObjectAssertions> MatchAllProperties(
            this ObjectAssertions assertions,
            object other,
            string because = "",
            params object[] becauseArgs)
        {
            return MatchAllProperties(assertions, other, new List<string>(), new Dictionary<string, string>(), because, becauseArgs);
        }

        public static AndConstraint<ObjectAssertions> MatchAllProperties(
            this ObjectAssertions assertions,
            object other,
            IEnumerable<string> excludedProperties,
            string because = "",
            params object[] becauseArgs)
        {
            return MatchAllProperties(assertions, other, excludedProperties, new Dictionary<string, string>(), because, becauseArgs);
        }

        public static AndConstraint<ObjectAssertions> MatchAllProperties(
            this ObjectAssertions assertions,
            object other,
            IDictionary<string, string> translatedProperties,
            string because = "",
            params object[] becauseArgs)
        {
            return MatchAllProperties(assertions, other, new string[] { }, translatedProperties, because, becauseArgs);
        }

        public static AndConstraint<ObjectAssertions> MatchAllProperties(
            this ObjectAssertions assertions,
            object other,
            IEnumerable<string> excludedProperties,
            IDictionary<string, string> translatedProperties,
            string because = "",
            params object[] becauseArgs)
        {
            var failureMessages = new List<string>();

            var subjectType = assertions.Subject.GetType();

            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .ForCondition(other != null)
                .FailWith("You can't assert that all properties match if the other object is null")
                .Then
                .Given(() => subjectType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
                .ForCondition((subjectProperties) =>
                {
                    var otherType = other.GetType();
                    var otherProperties = otherType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

                    foreach (var subjectProp in subjectProperties)
                    {
                        if (excludedProperties.Contains(subjectProp.Name))
                            continue;

                        if (!translatedProperties.TryGetValue(subjectProp.Name, out string propertyName))
                        {
                            propertyName = subjectProp.Name;
                        }

                        var otherProp = otherProperties.SingleOrDefault(p => p.Name == propertyName);
                        if (otherProp == null)
                        {
                            string message = $"Could not find matching property for {propertyName}.";
                            failureMessages.Add(message);
                            continue;
                        }

                        var subjectValue = subjectProp.GetValue(assertions.Subject);
                        var otherValue = otherProp.GetValue(other);

                        if (!subjectValue.Equals(otherProp.GetValue(other)))
                        {
                            string message = $"Value did not match for {propertyName}.\nExpected: {subjectValue}\nRecieved: {otherValue}";
                            failureMessages.Add(message);
                        }
                    }

                    return !failureMessages.Any();
                })
                .FailWith(
                    "Expected all properties to match, but received {0} unmatched {1}.\nSummary: {2}",
                    failureMessages.Count,
                    PropertyPluralization(failureMessages.Count),
                    failureMessages);

            return new AndConstraint<ObjectAssertions>(assertions);
        }

        private static string PropertyPluralization(int count) => count == 1 ? "property" : "properties";

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
