using AI_ticket_analyzer.Models.DTO;
using AI_ticket_analyzer.Service;
using Xunit;

namespace AI_ticket_analyzer.Tests
{
    public class MockAiClientTests
    {
        private readonly MockAiClient _client = new();

        [Theory]
        [InlineData("Cannot login to account", "Forgot password", "Login")]
        [InlineData("Credit card failed", "Please refund my invoice payment", "Billing")]
        [InlineData("Application crash on submit", "NullReferenceException error", "Bug")]
        [InlineData("New button needed", "Please add dark mode feature request", "Feature Request")]
        [InlineData("General question", "How does the system work?", "Other")]
        public async Task AnalyzeTicketAsync_DetectsCorrectCategory(string title, string description, string expectedCategory)
        {
            var request = new AnalyzeTicketRequest { Title = title, Description = description };

            var result = await _client.AnalyzeTicketAsync(request);

            Assert.Equal(expectedCategory, result.Category);
        }

        [Theory]
        [InlineData("Urgent server down", "Users cannot access system", "High")]
        [InlineData("Page load is slow", "Sometimes fails to refresh", "Medium")]
        [InlineData("Minor color change", "Change header font", "Low")]
        public async Task AnalyzeTicketAsync_DetectsCorrectPriority(string title, string description, string expectedPriority)
        {
            var request = new AnalyzeTicketRequest { Title = title, Description = description };

            var result = await _client.AnalyzeTicketAsync(request);

            Assert.Equal(expectedPriority, result.Priority);
        }

        [Fact]
        public async Task AnalyzeTicketAsync_TruncatesSummary_WhenTitleExceeds80Chars()
        {
            var longTitle = new string('x', 120);
            var request = new AnalyzeTicketRequest { Title = longTitle, Description = "Test description" };

            var result = await _client.AnalyzeTicketAsync(request);

            Assert.Equal(80, result.Summary.Length);
            Assert.Equal(longTitle[..80], result.Summary);
        }

        [Fact]
        public async Task AnalyzeTicketAsync_ThrowsOperationCanceledException_WhenCancellationRequested()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();
            var request = new AnalyzeTicketRequest { Title = "Test", Description = "Test" };

            await Assert.ThrowsAsync<OperationCanceledException>(() => _client.AnalyzeTicketAsync(request, cts.Token));
        }
    }
}
