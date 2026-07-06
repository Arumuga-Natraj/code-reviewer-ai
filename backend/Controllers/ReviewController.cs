using Microsoft.AspNetCore.Mvc;
using CodeReviewer.Backend.Models;
using CodeReviewer.Backend.Services;

namespace CodeReviewer.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpPost]
    public async Task<IActionResult> PostReview([FromBody] ReviewGenericRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PrUrl) && string.IsNullOrWhiteSpace(request.CodeContent))
        {
            return BadRequest(new { error = "Either a GitHub PR URL or direct Code Content must be provided." });
        }

        try
        {
            if (!string.IsNullOrWhiteSpace(request.PrUrl))
            {
                var response = await _reviewService.ReviewPullRequestAsync(request.PrUrl);
                return Ok(response);
            }
            else
            {
                var response = await _reviewService.ReviewCodeContentAsync(
                    request.FileName ?? "UploadedFile.txt",
                    request.CodeContent!
                );
                return Ok(response);
            }
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"An error occurred during code review: {ex.Message}" });
        }
    }

    // Helper request wrapper class to match the original single endpoint payload structure
    public class ReviewGenericRequest
    {
        public string? PrUrl { get; set; }
        public string? CodeContent { get; set; }
        public string? FileName { get; set; }
    }
}
