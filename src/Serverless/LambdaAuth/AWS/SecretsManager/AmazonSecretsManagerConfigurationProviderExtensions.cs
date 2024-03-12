using Microsoft.Extensions.Configuration;

namespace TechLanchesLambda.AWS.SecretsManager;

public static class AmazonSecretsManagerConfigurationProviderExtensions
{
    public static IConfigurationBuilder AddAmazonSecretsManager(this IConfigurationBuilder configurationBuilder,
                    string region,
                    string secretName)
    {
        var configurationSource =
                new AmazonSecretsManagerConfigurationSource(region, secretName);

        configurationBuilder.Add(configurationSource);
        return configurationBuilder;
    }
}
