namespace CodeReviewer.Backend.Services;

public class PromptService
{
    public string BuildCodeReviewPrompt(string sourceName, string codeToReview)
    {
        return $"You are an expert senior software engineer reviewing a pull request or code file for {sourceName}.\n" +
               "Analyze the following code/diff for bugs, performance issues, security vulnerabilities, readability, and best practices.\n" +
               "For each issue you find, you MUST provide structural feedback in JSON format.\n" +
               "You must provide the exact segment of original code, a suggested replacement, and an explanation.\n\n" +
               "Your output response must strictly conform to the following JSON structure:\n" +
               "{\n" +
               "  \"reviews\": [\n" +
               "    {\n" +
               "      \"filePath\": \"Name of the file\",\n" +
               "      \"originalCode\": \"The exact lines of the original bad code snippet\",\n" +
               "      \"suggestedCode\": \"Your improved replacement code block\",\n" +
               "      \"comment\": \"Explanation of why this change is suggested and what it fixes\",\n" +
               "      \"lineStart\": 10, // approximate start line number\n" +
               "      \"lineEnd\": 12   // approximate end line number\n" +
               "    }\n" +
               "  ]\n" +
               "}\n\n" +
               "Here is the diff or code content to review:\n\n" +
               codeToReview;
    }
}
