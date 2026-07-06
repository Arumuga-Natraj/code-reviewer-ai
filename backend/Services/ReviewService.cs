using CodeReviewer.Backend.Models;

namespace CodeReviewer.Backend.Services;

public class ReviewService : IReviewService
{
    private readonly GitHubService _gitHubService;
    private readonly OllamaService _ollamaService;
    private readonly PromptService _promptService;

    public ReviewService(GitHubService gitHubService, OllamaService ollamaService, PromptService promptService)
    {
        _gitHubService = gitHubService;
        _ollamaService = ollamaService;
        _promptService = promptService;
    }

    public async Task<ReviewResponse> ReviewPullRequestAsync(string prUrl)
    {
        var diff = await _gitHubService.FetchPullRequestDiffAsync(prUrl);
        var prompt = _promptService.BuildCodeReviewPrompt(prUrl, diff);
        var response = await _ollamaService.GenerateReviewAsync(prompt);
        response.RawCodeOrDiff = diff;
        return response;
    }

    public async Task<ReviewResponse> ReviewCodeContentAsync(string fileName, string codeContent)
    {
        var prompt = _promptService.BuildCodeReviewPrompt(fileName, codeContent);
        var response = await _ollamaService.GenerateReviewAsync(prompt);
        response.RawCodeOrDiff = codeContent;
        return response;
    }
}
