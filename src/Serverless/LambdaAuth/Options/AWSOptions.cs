namespace TechLanchesLambda.Options
{
    public class AWSOptions
    {
        public string Region { get; set; } = string.Empty;
        public string UserPoolId { get; set; } = string.Empty;
        public string UserPoolClientId { get; set; } = string.Empty;
        public string UserTechLanches { get; set; } = string.Empty;
        public string EmailDefault { get; set; } = string.Empty;
        public string PasswordDefault { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}