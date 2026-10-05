using AI_ticket_analyzer.Models;
using AI_ticket_analyzer.Models.DTO;
using AI_ticket_analyzer.Service;

public class MockTicketAiService : ITicketService
{
    public Task<AnalyzeTicketResponse> AnalyzeTicketAsync(AnalyzeTicketRequest request)
    {
        var text = $"{request.Title} {request.Description}".ToLower();

        var result = new AnalyzeTicketResponse
        {
            Summary = GenerateSummary(request.Title),
            Category = DetectCategory(text),
            Priority = DetectPriority(text)
        };

        return Task.FromResult(result);
    }

    private static string GenerateSummary(string title)
        => title.Length > 80 ? title[..80] : title;

    private static string DetectCategory(string text)
    {
        if (text.Contains("login") || text.Contains("password"))
            return "Login";

        if (text.Contains("payment") || text.Contains("invoice"))
            return "Billing";

        if (text.Contains("error") || text.Contains("crash"))
            return "Bug";

        if (text.Contains("feature") || text.Contains("request"))
            return "Feature Request";

        return "Other";
    }

    private static string DetectPriority(string text)
    {
        if (text.Contains("urgent") || text.Contains("cannot"))
            return "High";

        if (text.Contains("slow") || text.Contains("sometimes"))
            return "Medium";

        return "Low";
    }
}
