using System.Text.RegularExpressions;

namespace CodeReviewer.Backend.Services;

public class GitHubService
{
    private readonly HttpClient _httpClient;

    public GitHubService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> FetchPullRequestDiffAsync(string prUrl)
    {
        var match = Regex.Match(prUrl, @"github\.com/(?<owner>[^/]+)/(?<repo>[^/]+)/pull/(?<num>\d+)");
        if (!match.Success)
        {
            throw new ArgumentException("Invalid GitHub Pull Request URL. Format must be: https://github.com/owner/repo/pull/number");
        }

        var owner = match.Groups["owner"].Value;
        var repo = match.Groups["repo"].Value;
        var prNum = match.Groups["num"].Value;

        var githubUrl = $"https://api.github.com/repos/{owner}/{repo}/pulls/{prNum}";
        
        using var request = new HttpRequestMessage(HttpMethod.Get, githubUrl);
        request.Headers.Add("User-Agent", "CodeReviewer-Backend");
        request.Headers.Add("Accept", "application/vnd.github.v3.diff");

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to fetch PR diff from GitHub. Status: {response.StatusCode}. Details: {error}");
        }

        return await response.Content.ReadAsStringAsync();
    }
}
