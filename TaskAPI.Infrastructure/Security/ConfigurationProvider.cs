using Microsoft.Extensions.Configuration;
using TaskAPI.Application.Interfaces;

namespace TaskAPI.Infrastructure.Security
{
    public class ConfigurationProvider(IConfiguration configuration) : IPepperKeyProvider
    {
        public byte[] GetKey(string key)
        {
            var base64Key = configuration[key];

            if (string.IsNullOrWhiteSpace(base64Key))
            {
                throw new InvalidOperationException("pepper key is missing or empty.");
            }

            return Convert.FromBase64String(base64Key);
        }
    }
}