using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Web.Configuration;
using Xunit;

namespace Integration.Tests;

public sealed class DeploymentConfigurationValidatorTests
{
    [Fact]
    public void Production_configuration_accepts_explicit_host_and_persistent_paths()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Database"] = "Server=sql;Database=ConstructionManagement;User ID=app;Password=secret",
            ["AllowedHosts"] = "construction.example.com",
            ["FileStorage:RootPath"] = "/app/data/uploads",
            ["Security:DataProtection:KeysPath"] = "/app/data/keys"
        });

        var exception = Record.Exception(() =>
            DeploymentConfigurationValidator.Validate(
                configuration,
                CreateEnvironment(Environments.Production)));

        Assert.Null(exception);
    }

    [Fact]
    public void Production_configuration_rejects_localhost_only_hosts()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Database"] = "Server=sql;Database=ConstructionManagement;User ID=app;Password=secret",
            ["AllowedHosts"] = "localhost;127.0.0.1",
            ["FileStorage:RootPath"] = "/app/data/uploads",
            ["Security:DataProtection:KeysPath"] = "/app/data/keys"
        });

        var exception = Assert.Throws<InvalidOperationException>(() =>
            DeploymentConfigurationValidator.Validate(
                configuration,
                CreateEnvironment(Environments.Production)));

        Assert.Contains(
            "externally reachable production host",
            exception.Message,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Production_configuration_rejects_wildcard_hosts()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Database"] = "Server=sql;Database=ConstructionManagement;User ID=app;Password=secret",
            ["AllowedHosts"] = "*",
            ["FileStorage:RootPath"] = "/app/data/uploads",
            ["Security:DataProtection:KeysPath"] = "/app/data/keys"
        });

        var exception = Assert.Throws<InvalidOperationException>(() =>
            DeploymentConfigurationValidator.Validate(
                configuration,
                CreateEnvironment(Environments.Production)));

        Assert.Contains(
            "cannot contain '*'",
            exception.Message,
            StringComparison.Ordinal);
    }

    private static IConfiguration BuildConfiguration(
        IDictionary<string, string?> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    private static IHostEnvironment CreateEnvironment(
        string environmentName)
    {
        return new TestHostEnvironment
        {
            EnvironmentName = environmentName
        };
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = string.Empty;

        public string ApplicationName { get; set; } = "Integration.Tests";

        public string ContentRootPath { get; set; } =
            Directory.GetCurrentDirectory();

        public IFileProvider ContentRootFileProvider { get; set; } =
            new NullFileProvider();
    }
}
