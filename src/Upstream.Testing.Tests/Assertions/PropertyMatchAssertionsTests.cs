using FluentAssertions;
using FluentAssertions.Execution;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Sdk;

namespace Upstream.Testing.Tests.Assertions
{
    public class PropertyMatchAssertionsTests
    {
        [Fact]
        public void MatchesAllProperties_success()
        {
            object obj = new { Foo = "Bar" };
            object other = new { Foo = "Bar" };

            Action assertion = () => obj.Should().MatchAllProperties(other);

            assertion.Should()
                .NotThrow();
        }

        [Fact]
        public void MatchesAllProperties_throws_if_a_property_does_not_match()
        {
            object obj = new { Foo = "Bar" };
            object other = new { Foo = "Foo?" };

            Action assertion = () => obj.Should().MatchAllProperties(other);

            assertion.Should()
                .Throw<XunitException>()
                .WithMessage("*Expected all properties to match, but received 1 unmatched \"property\".*");
        }

        [Fact]
        public void MatchesAllProperties_throws_if_other_is_null()
        {
            object obj = new { Foo = "Bar" };
            object other = null;

            Action assertion = () => obj.Should().MatchAllProperties(other);

            assertion.Should()
                .Throw<XunitException>()
                .WithMessage("*null*");
        }
    }
}
