using AwesomeGithubStats.Core.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Tests.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace Tests.IntegrationTests
{
    public class GithubServiceTests : IClassFixture<Warmup>
    {
        private readonly IGithubService _githubService;

        public GithubServiceTests(Warmup services, ITestOutputHelper output)
        {
            _githubService = services.Services.GetRequiredService<IGithubService>();
        }

        [Theory]
        [InlineData("brunohbrito")]
        [InlineData("sindresorhus")]
        [InlineData("kamranahmedse")]
        public async Task Should_Get_User_Information(string username)
        {
            var stats = await _githubService.GetUserStats(username);

            stats.Should().NotBeNull();
            stats.Commits.Should().BeGreaterThan(0);
        }

    }
}