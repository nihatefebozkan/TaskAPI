using Moq;
using System.Security.Cryptography;
using TaskAPI.Application.Interfaces;
using TaskAPI.Infrastructure.Security;
using Xunit;
using KeyProvider = TaskAPI.Infrastructure.Security.ConfigurationProvider;

namespace TaskAPI.API.Tests
{
    public class ConfigurationProviderTests
    {
        [Fact]
        public void ValidBase64Key_IsDecodedCorrectly()
        {
            var expected = Enumerable.Range(1, 32).Select(value => (byte)value).ToArray();

            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["key"] = Convert.ToBase64String(expected)
            }).Build();

            var actual = new KeyProvider(configuration).GetKey("key");

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void MissingOrEmptyKey_IsRejected(string? value)
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["key"] = value
            }).Build();

            var exception = Assert.Throws<InvalidOperationException>(() => new KeyProvider(configuration).GetKey("key"));

            Assert.Equal("pepper key is missing or empty.", exception.Message);
        }

        [Fact]
        public void InvalidBase64Key_IsRejected()
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["key"] = "not-base64!"
            }).Build();

            Assert.Throws<FormatException>(() => new KeyProvider(configuration).GetKey("key"));
        }

        [Fact]
        public void RequestedConfigurationName_IsUsed()
        {
            var expected = new byte[] { 1, 2, 3, 4 };

            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["another-key"] = Convert.ToBase64String(expected)
            }).Build();

            Assert.Equal(expected, new KeyProvider(configuration).GetKey("another-key"));
        }
    }

    public class PasswordHasherTests
    {
        private static PasswordHasher CreateHasher(byte keyValue = 1)
        {
            var provider = new Mock<IPepperKeyProvider>();
            provider.Setup(current => current.GetKey("key")).Returns(Enumerable.Repeat(keyValue, 32).ToArray());

            return new PasswordHasher(provider.Object);
        }

        [Theory]
        [InlineData("Test-password-123!")]
        [InlineData("Türkçe-Şifre-123!")]
        [InlineData("🔐-Test-password")]
        public void HashAndVerify_CorrectPasswordSucceeds(string password)
        {
            var hasher = CreateHasher();
            var hash = hasher.Hash(password);

            Assert.True(hasher.Verify(password, hash));
            Assert.DoesNotContain(password, hash);
        }

        [Fact]
        public void WrongPassword_IsRejected()
        {
            var hasher = CreateHasher();
            var hash = hasher.Hash("Correct-password");

            Assert.False(hasher.Verify("Wrong-password", hash));
        }

        [Fact]
        public void DifferentPepperKey_IsRejected()
        {
            var hash = CreateHasher(1).Hash("Test-password");

            Assert.False(CreateHasher(2).Verify("Test-password", hash));
        }

        [Fact]
        public void SamePassword_ProducesDifferentSaltsAndHashes()
        {
            var hasher = CreateHasher();

            var first = hasher.Hash("Test-password");
            var second = hasher.Hash("Test-password");

            Assert.NotEqual(first, second);
            Assert.NotEqual(first.Split('.')[2], second.Split('.')[2]);
            Assert.True(hasher.Verify("Test-password", first));
            Assert.True(hasher.Verify("Test-password", second));
        }

        [Fact]
        public void StoredFormat_ContainsExpectedVersionIterationsSaltAndHash()
        {
            var hash = CreateHasher().Hash("Test-password");
            var parts = hash.Split('.');

            Assert.Equal(4, parts.Length);
            Assert.Equal("v1", parts[0]);
            Assert.Equal(600_000, int.Parse(parts[1]));
            Assert.Equal(16, Convert.FromBase64String(parts[2]).Length);
            Assert.Equal(32, Convert.FromBase64String(parts[3]).Length);
        }

        [Theory]
        [InlineData("")]
        [InlineData("invalid")]
        [InlineData("v1.600000.only-three-parts")]
        [InlineData("v2.600000.salt.hash")]
        public void UnsupportedStoredFormat_ReturnsFalse(string storedHash)
        {
            Assert.False(CreateHasher().Verify("Test-password", storedHash));
        }

        [Fact]
        public void ModifiedHash_IsRejected()
        {
            var hasher = CreateHasher();
            var parts = hasher.Hash("Test-password").Split('.');
            var hashBytes = Convert.FromBase64String(parts[3]);

            hashBytes[0] ^= 1;
            parts[3] = Convert.ToBase64String(hashBytes);

            Assert.False(hasher.Verify("Test-password", string.Join(".", parts)));
        }

        [Fact]
        public void ModifiedSalt_IsRejected()
        {
            var hasher = CreateHasher();
            var parts = hasher.Hash("Test-password").Split('.');
            var saltBytes = Convert.FromBase64String(parts[2]);

            saltBytes[0] ^= 1;
            parts[2] = Convert.ToBase64String(saltBytes);

            Assert.False(hasher.Verify("Test-password", string.Join(".", parts)));
        }

        [Theory]
        [InlineData("v1.invalid.AQ==.AQ==")]
        [InlineData("v1.600000.not-base64!.AQ==")]
        [InlineData("v1.600000.AQ==.not-base64!")]
        public void MalformedStoredValues_ThrowCurrentFormatException(string storedHash)
        {
            Assert.Throws<FormatException>(() => CreateHasher().Verify("Test-password", storedHash));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void NonPositiveIterations_AreRejected(int iterations)
        {
            var salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
            var hash = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

            Assert.Throws<ArgumentOutOfRangeException>(() => CreateHasher().Verify("Test-password", $"v1.{iterations}.{salt}.{hash}"));
        }

        [Fact]
        public void MissingPepperError_IsNotHidden()
        {
            var provider = new Mock<IPepperKeyProvider>();
            provider.Setup(current => current.GetKey("key")).Throws(new InvalidOperationException("Missing key."));

            var hasher = new PasswordHasher(provider.Object);

            var exception = Assert.Throws<InvalidOperationException>(() => hasher.Hash("Test-password"));

            Assert.Equal("Missing key.", exception.Message);
        }
    }
}