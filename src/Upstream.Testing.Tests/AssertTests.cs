using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Sdk;

namespace Upstream.Testing.Tests
{
    public class AssertTests
    {
        [Fact]
        public void AllPropertiesMapped_success()
        {
            var expected = new { Foo = "Bar" };
            var actual = new { Foo = "Bar" };

           var output = Assert.AllPropertiesMapped(expected, actual);

            Assert.True(output);
        }

        [Fact]
        public void AllPropertiesMapped_failure_different_value()
        {
            var expected = new { Foo = "Bar" };
            var actual = new { Foo = "Wumbo" };

            Assert.Throws<TestingException>(() => Assert.AllPropertiesMapped(expected, actual));
        }

        [Fact]
        public void AllPropertiesMapped_failure_missing_property()
        {
            var expected = new { Foo = "Bar", Bar = "Foo" };
            var actual = new { Foo = "Bar" };

            Assert.Throws<TestingException>(() => Assert.AllPropertiesMapped(expected, actual));
        }

        [Fact]
        public void AllPropertiesMapped_failure_multiple_errors()
        {
            var expected = new { Foo = "Bar", Bar = "Foo" };
            var actual = new { Foo = "Wumbo" };

            var ex = Assert.Throws<AggregateException>(() => Assert.AllPropertiesMapped(expected, actual));

            Assert.Equal(2, ex.InnerExceptions.Count);
            Assert.IsType<TestingException>(ex.InnerExceptions[0]);
            Assert.IsType<TestingException>(ex.InnerExceptions[1]);
        }

        [Fact]
        public void AllPropertiesMapped_success_ignored_property()
        {
            var expected = new { Foo = "Bar", Bar = "Foo" };
            var actual = new { Foo = "Bar" };

            var output = Assert.AllPropertiesMapped(expected, actual, excludedProperties: "Bar");

            Assert.True(output);
        }

        [Fact]
        public void AllPropertiesMapped_success_translated_property()
        {
            var expected = new { Foo = "Bar" };
            var actual = new { Wumbo = "Bar" };

            var output = Assert.AllPropertiesMapped(expected, actual, translatedProperties: new Dictionary<string, string> { {"Foo", "Wumbo"} });

            Assert.True(output);
        }

        [Fact]
        public void NoPropertiesNullOrDefault_success()
        {
            var obj = new { Foo = "Bar" };

            var output = Assert.NoPropertiesNullOrDefault(obj);

            Assert.True(output);
        }

        [Fact]
        public void NoPropertiesNullOrDefault_null()
        {
            var obj = new { Foo = (string)null };

            Assert.Throws<NotNullException>(() => Assert.NoPropertiesNullOrDefault(obj));
        }

        [Fact]
        public void NoPropertiesNullOrDefault_default()
        {
            var obj = new { Foo = 0 };

            Assert.Throws<NotEqualException>(() => Assert.NoPropertiesNullOrDefault(obj));
        }

        [Fact]
        public void NoPropertiesNullOrDefault_multiple_errors()
        {
            var obj = new { Foo = (string)null, Bar = 0 };

            var ex = Assert.Throws<AggregateException>(() => Assert.NoPropertiesNullOrDefault(obj));

            Assert.Equal(2, ex.InnerExceptions.Count);
            Assert.IsType<NotNullException>(ex.InnerExceptions[0]);
            Assert.IsType<NotEqualException>(ex.InnerExceptions[1]);
        }
    }
}
