namespace ClinicAPI.Helpers
{
    public class JwtOptions
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Key { get; set; }
        public int ExpirationInYears { get; set; }
        public int RefreshTokenExpirationInDays { get; set; }
    }
}
