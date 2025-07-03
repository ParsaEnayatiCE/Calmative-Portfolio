using Microsoft.Extensions.Options;

namespace Calmative.Admin.Web.Services
{
    public class AdminCredentialsOptions
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public interface IAdminAuthService
    {
        bool ValidateCredentials(string username, string password);
    }

    public class AdminAuthService : IAdminAuthService
    {
        private readonly AdminCredentialsOptions _credentials;

        public AdminAuthService(IOptions<AdminCredentialsOptions> options)
        {
            _credentials = options.Value;
        }

        public bool ValidateCredentials(string username, string password)
        {
            return username == _credentials.Username && password == _credentials.Password;
        }
    }
} 