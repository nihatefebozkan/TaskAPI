using Microsoft.Extensions.Configuration;
using TaskAPI.Entities.Interfaces;

namespace TaskAPI.Infrastructure.Security
{
    public class ConfigurationPepperKeyProvider(IConfiguration configuration) : IPepperKeyProvider
    {
        public byte[] GetKey()
        {
            var base64Key = configuration["Security:Pepper"];

            if (string.IsNullOrWhiteSpace(base64Key))
            {
                throw new InvalidOperationException("Security:Pepper ayarı bulunamadı.");
            }

            return Convert.FromBase64String(base64Key);
        }
    }
}