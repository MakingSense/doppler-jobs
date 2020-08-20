using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Doppler.Currency.Job.Authorization
{
    [ExcludeFromCodeCoverage]
    public static class TokenServiceCollectionExtensions
    {
        public static IServiceCollection AddJwtToken([NotNull] this IServiceCollection services)
        {
            // TODO: consider to get the path from somewhere else
            var basePath = Directory.GetCurrentDirectory();
            var rsaParametersFilePath = services.BuildServiceProvider()
                .GetService<IOptions<JwtOptions>>()
                .Value
                .RsaParametersFilePath;

            var absolutePath = Path.Combine(basePath, rsaParametersFilePath);
            RsaSecurityKey key;
            using (var textReader = File.OpenText(absolutePath))
            {
                var publicAndPrivate = new RSACryptoServiceProvider();
                publicAndPrivate.FromXmlString(textReader.ReadToEnd());

                key = new RsaSecurityKey(publicAndPrivate.ExportParameters(true));
            }

            return services.AddSingleton(new SigningCredentials(key, SecurityAlgorithms.RsaSha256));
        }
    }
}
