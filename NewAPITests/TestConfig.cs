using System;
using System.IO;
using Microsoft.Extensions.Configuration;

public static class TestConfig
{
    public static IConfigurationRoot CreateConfig()
    {
        var basePath = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "NewAPI")
        );

        if (!Directory.Exists(basePath))
            throw new DirectoryNotFoundException($"Config base path not found: {basePath}");

        return new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();
    }
}
