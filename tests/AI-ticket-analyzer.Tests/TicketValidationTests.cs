using AI_ticket_analyzer.Exceptions;
using AI_ticket_analyzer.Models.DTO;
using AI_ticket_analyzer.Service;
using Xunit;

namespace AI_ticket_analyzer.Tests
{
    public class TicketValidationTests
    {
        [Fact]
        public void ValidateAiResponse_WithValidData_DoesNotThrow()
        {
            var response = new AnalyzeTicketResponse
            {
                Summary = "User cannot login with valid credentials",
                Category = "Login",
                Priority = "High"
            };

            var exception = Record.Exception(() => TicketService.ValidateAiResponse(response));
            Assert.Null(exception);
        }

        [Fact]
        public void ValidateAiResponse_WithNullResponse_ThrowsInvalidAiResponseException()
        {
            Assert.Throws<InvalidAiResponseException>(() => TicketService.ValidateAiResponse(null!));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ValidateAiResponse_WithNullOrWhitespaceSummary_ThrowsInvalidAiResponseException(string? summary)
        {
            var response = new AnalyzeTicketResponse
            {
                Summary = summary!,
                Category = "Login",
                Priority = "High"
            };

            Assert.Throws<InvalidAiResponseException>(() => TicketService.ValidateAiResponse(response));
        }

        [Fact]
        public void ValidateAiResponse_WithSummaryExceeding500Characters_ThrowsInvalidAiResponseException()
        {
            var response = new AnalyzeTicketResponse
            {
                Summary = new string('a', 501),
                Category = "Billing",
                Priority = "Low"
            };

            Assert.Throws<InvalidAiResponseException>(() => TicketService.ValidateAiResponse(response));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("UnknownCategory")]
        [InlineData("Security")]
        public void ValidateAiResponse_WithInvalidCategory_ThrowsInvalidAiResponseException(string? category)
        {
            var response = new AnalyzeTicketResponse
            {
                Summary = "Valid summary",
                Category = category!,
                Priority = "Medium"
            };

            Assert.Throws<InvalidAiResponseException>(() => TicketService.ValidateAiResponse(response));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("Critical")]
        [InlineData("Urgent")]
        [InlineData("P0")]
        public void ValidateAiResponse_WithInvalidPriority_ThrowsInvalidAiResponseException(string? priority)
        {
            var response = new AnalyzeTicketResponse
            {
                Summary = "Valid summary",
                Category = "Bug",
                Priority = priority!
            };

            Assert.Throws<InvalidAiResponseException>(() => TicketService.ValidateAiResponse(response));
        }

        [Theory]
        [InlineData("Billing")]
        [InlineData("Login")]
        [InlineData("Bug")]
        [InlineData("Feature Request")]
        [InlineData("Support")]
        [InlineData("Other")]
        public void ValidateAiResponse_WithAllAllowedCategories_PassesValidation(string category)
        {
            var response = new AnalyzeTicketResponse
            {
                Summary = "Valid summary",
                Category = category,
                Priority = "Low"
            };

            var exception = Record.Exception(() => TicketService.ValidateAiResponse(response));
            Assert.Null(exception);
        }

        [Theory]
        [InlineData("Low")]
        [InlineData("Medium")]
        [InlineData("High")]
        public void ValidateAiResponse_WithAllAllowedPriorities_PassesValidation(string priority)
        {
            var response = new AnalyzeTicketResponse
            {
                Summary = "Valid summary",
                Category = "Bug",
                Priority = priority
            };

            var exception = Record.Exception(() => TicketService.ValidateAiResponse(response));
            Assert.Null(exception);
        }
    }
}
