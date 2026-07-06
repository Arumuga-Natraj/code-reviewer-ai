using CodeReviewer.Backend.Models;

namespace CodeReviewer.Backend.Services;

public interface IReviewService
{
    Task<ReviewResponse> ReviewPullRequestAsync(string prUrl);
    Task<ReviewResponse> ReviewCodeContentAsync(string fileName, string codeContent);
}
