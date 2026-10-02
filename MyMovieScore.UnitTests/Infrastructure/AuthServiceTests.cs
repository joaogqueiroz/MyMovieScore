using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using MyMovieScore.Infrastructure.Auth;

namespace MyMovieScore.UnitTests.Infrastructure
{
    public class AuthServiceTests
    {
        private static AuthService CreateService() => new AuthService(Options.Create(new JwtOptions
        {
            Key = "unit-test-signing-key-with-more-than-256-bits",
            Issuer = "MyMovieScore",
            Audience = "ClientMyMovieScore",
            ExpirationHours = 2
        }));

        [Fact]
        public void HashPassword_DoesNotStoreThePlainPassword()
        {
            var hash = CreateService().HashPassword("Senha@123");

            Assert.NotEqual("Senha@123", hash);
            Assert.DoesNotContain("Senha@123", hash);
        }

        [Fact]
        public void HashPassword_UsesASaltSoTheSamePasswordHashesDifferently()
        {
            var service = CreateService();

            Assert.NotEqual(service.HashPassword("Senha@123"), service.HashPassword("Senha@123"));
        }

        [Fact]
        public void VerifyPassword_AcceptsOnlyTheRightPassword()
        {
            var service = CreateService();
            var hash = service.HashPassword("Senha@123");

            Assert.True(service.VerifyPassword(hash, "Senha@123"));
            Assert.False(service.VerifyPassword(hash, "Senha@124"));
        }

        [Fact]
        public void VerifyPassword_RejectsALegacySha256Hash()
        {
            // SHA256 of "Senha@123" as the API stored it before PasswordHasher
            const string legacyHash = "a2ca37fe6fdc490b8f7ce841e1701a169d2b1697c6b5b5c63f94abb8f9b6d6dd";

            Assert.False(CreateService().VerifyPassword(legacyHash, "Senha@123"));
        }

        [Fact]
        public void GenerateJwtToken_HasIssuerAudienceUserAndExpiration()
        {
            var before = DateTime.UtcNow;

            var token = new JwtSecurityTokenHandler().ReadJwtToken(CreateService().GenerateJwtToken("user@test.com"));

            Assert.Equal("MyMovieScore", token.Issuer);
            Assert.Contains("ClientMyMovieScore", token.Audiences);
            Assert.Equal("user@test.com", token.Claims.Single(c => c.Type == "userName").Value);
            Assert.InRange(token.ValidTo, before.AddHours(2).AddMinutes(-1), before.AddHours(2).AddMinutes(1));
        }
    }
}
