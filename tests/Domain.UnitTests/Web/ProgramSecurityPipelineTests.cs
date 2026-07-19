using System.Text.RegularExpressions;
using NUnit.Framework;
using Shouldly;

namespace CleanArchitecture.Northwind.Domain.UnitTests.Web;

public class ProgramSecurityPipelineTests
{
    [Test]
    public void ProgramDoesNotLogCloudflareSecretKey()
    {
        var program = ReadProgramSource();

        Regex.IsMatch(program, @"Console\.WriteLine\([^;]*SecretKey", RegexOptions.Singleline).ShouldBeFalse();
    }

    [Test]
    public void ProgramRegistersHttpsRedirectionOnlyOnce()
    {
        var program = ReadProgramSource();

        Regex.Matches(program, @"\bapp\.UseHttpsRedirection\(").Count.ShouldBe(1);
    }

    [Test]
    public void ProgramOrdersProxyAndSecurityMiddlewareBeforeStaticFiles()
    {
        var program = ReadProgramSource();

        program.IndexOf("app.UseForwardedHeaders();", StringComparison.Ordinal).ShouldBeLessThan(
            program.IndexOf("app.UseHsts();", StringComparison.Ordinal));
        program.IndexOf("app.UseHsts();", StringComparison.Ordinal).ShouldBeLessThan(
            program.IndexOf("app.UseHttpsRedirection();", StringComparison.Ordinal));
        program.IndexOf("app.UseHttpsRedirection();", StringComparison.Ordinal).ShouldBeLessThan(
            program.IndexOf("app.UseSecurityHeaders(app.Environment);", StringComparison.Ordinal));
        program.IndexOf("app.UseSecurityHeaders(app.Environment);", StringComparison.Ordinal).ShouldBeLessThan(
            program.IndexOf("app.UseStaticFiles(", StringComparison.Ordinal));
    }

    private static string ReadProgramSource()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);

        while (directory is not null)
        {
            var programPath = Path.Combine(directory.FullName, "src", "Web", "Program.cs");
            if (File.Exists(programPath))
            {
                return File.ReadAllText(programPath);
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not locate src/Web/Program.cs from test directory.");
    }
}
