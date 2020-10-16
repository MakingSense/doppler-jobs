namespace CrossCutting.Authorization
{
    public interface IJwtTokenGenerator
    {
        string CreateJwtToken();
    }
}
