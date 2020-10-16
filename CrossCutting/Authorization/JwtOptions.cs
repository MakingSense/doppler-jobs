using System.Diagnostics.CodeAnalysis;

namespace CrossCutting.Authorization
{
    [ExcludeFromCodeCoverage]
    public class JwtOptions
    {
        public string RsaParametersFilePath { get; set; }
        public long TokenLifeTime { get; set; }
    }
}
