using AspNetCore_Template.exception;

namespace AspNetCore_Template.helper;

public static class Helper
{
    public static string GetEnvVar(string envName)
    {
        return Environment.GetEnvironmentVariable(envName) ?? throw new EnvironmentVariableMissingException(envName);
    }

    public static string GetDatabaseConnectionString()
    {
        return
            $"Host={GetEnvVar("DB_HOST")};Username={GetEnvVar("DB_USERNAME")};Password={GetEnvVar("DB_PASSWORD")};Database={GetEnvVar("DB_NAME")}";
    }

    public static string GetAppLogoUrl()
    {
        return Path.Combine("APP_DOMAIN", "images", "logo.png");
    }
}