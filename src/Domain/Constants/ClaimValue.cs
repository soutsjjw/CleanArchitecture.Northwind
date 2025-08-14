namespace CleanArchitecture.Northwind.Domain.Constants;

public static class ClaimValue
{
    public static string Perm(string module, string op) => $"{module}:{op}";
    public static string Scope(string module, DataScope scope) => $"{module}:{scope}";
}
