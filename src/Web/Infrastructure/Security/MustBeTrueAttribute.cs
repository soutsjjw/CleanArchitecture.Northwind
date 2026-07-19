using System.ComponentModel.DataAnnotations;

namespace CleanArchitecture.Northwind.Web.Infrastructure.Security;

public sealed class MustBeTrueAttribute : ValidationAttribute
{
    public override bool IsValid(object? value) => value is bool b && b;
}
