namespace Doppler.Sap.Job.Service.Settings
{
    public class HttpClientPoliciesSettings
    {
        public string ClientName { get; set; }
        public Policies Policies { get; set; }
    }

    public class Policies
    {
        public int RetryAttemps { get; set; }
    }
}
