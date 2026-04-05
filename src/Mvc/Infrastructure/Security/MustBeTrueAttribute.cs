using System.ComponentModel.DataAnnotations;

namespace Mvc.Infrastructure.Security;

public sealed class MustBeTrueAttribute : ValidationAttribute
{
    public override bool IsValid(object? value) => value is bool b && b;
}
