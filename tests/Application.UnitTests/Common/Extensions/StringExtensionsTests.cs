using CleanArchitecture.Northwind.Application.Common.Extensions;
using NUnit.Framework;
using Shouldly;

namespace CleanArchitecture.Northwind.Application.UnitTests.Common.Extensions;

public class StringExtensionsTests
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void TrimToNullShouldReturnNullForMissingOrWhitespaceValue(string? value)
    {
        value.TrimToNull().ShouldBeNull();
    }

    [Test]
    public void TrimToNullShouldTrimNonWhitespaceValue()
    {
        "  Northwind  ".TrimToNull().ShouldBe("Northwind");
    }
}
