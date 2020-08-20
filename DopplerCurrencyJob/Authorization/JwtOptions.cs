using System.Diagnostics.CodeAnalysis;

namespace Doppler.Currency.Job.Authorization
{
    [ExcludeFromCodeCoverage]
    public class JwtOptions
    {
        public string RsaParametersFilePath { get; set; }
        public long TokenLifeTime { get; set; }
    }
}
