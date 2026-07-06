# Project Study Guide: AI Code Reviewer Platform

This document provides a comprehensive explanation of the architecture, data flow, components, and code pathways of the Stateless AI Code Reviewer platform.

---

## 1. System Architecture & Component Interaction

The platform is designed to be **stateless**, meaning it does not persist reviews in a database. It processes files and pull requests (PRs) on-demand, reviews them via an LLM, and returns the suggestions directly to the browser for rendering and download.

### High-Level Architecture Flowchart

```mermaid
graph TD
    User([User]) -->|1. Paste PR URL / Upload File| React[React Frontend]
    React -->|2. HTTP POST JSON| Controller[ReviewController]
    
    subgraph .NET Web API Backend
        Controller -->|3. Route Request| ReviewService[ReviewService]
        ReviewService -->|4. If PR, Fetch Diff| GitHubService[GitHubService]
        GitHubService -->|5. HTTP GET Diff| GitHubAPI[(GitHub Public API)]
        GitHubAPI -->|6. Return Raw Diff| GitHubService
        
        ReviewService -->|7. Build Prompt Template| PromptService[PromptService]
        ReviewService -->|8. Generate Report JSON| OllamaService[OllamaService]
    end
    
    subgraph LLM Layer (Free Options)
        OllamaService -->|9a. HTTP POST JSON Schema| GeminiAPI[Google Gemini 1.5/2.5 Flash]
        OllamaService -->|9b. HTTP POST Local host| OllamaAPI[Local Ollama Server]
    end
    
    GeminiAPI -->|10. Structured JSON Result| OllamaService
    OllamaAPI -->|10. Structured JSON Result| OllamaService
    
    OllamaService -->|11. ReviewResponse Object| ReviewService
    ReviewService -->|12. Append original diff/source code| Controller
    Controller -->|13. Return JSON payload| React
    React -->|14. Render Side-by-Side comparison & Download MD| User
```

---

## 2. Directory Structure & Code Responsibilities

The backend is built using ASP.NET Core with a clean MVC (Model-View-Controller) / Service-oriented design:

```
AI-CodeReview-Backend
│
├── Controllers
│   └── ReviewController.cs       # Entry point for HTTP requests. Handles routing.
│
├── Services
│   ├── IReviewService.cs        # Interface contract defining review actions.
│   ├── ReviewService.cs         # Core coordinator class orchestrating services.
│   ├── GitHubService.cs         # Client helper fetching public PR differences.
│   ├── OllamaService.cs         # LLM integration layer for Ollama (local) & Gemini (cloud).
│   └── PromptService.cs         # Prompt engineer module generating instructions.
│
├── Models
│   ├── ReviewRequest.cs         # DTO for uploaded file review inputs.
│   ├── PullRequestReviewRequest.cs  # DTO for PR reviews.
│   ├── ReviewResponse.cs        # Structured API return containing list of issues & source.
│   └── ReviewIssue.cs           # Detailed model for an individual code suggestion.
│
├── Utils
│   └── FileHelper.cs            # Input cleaning and text processing helpers.
│
├── appsettings.json             # Configuration file (LLM Provider, API keys, models).
└── Program.cs                   # Bootstrapper configuring CORS, dependency injection, and server.
```

---

## 3. Step-by-Step Code Walkthrough

### Step A: The Request
The user interacts with the React frontend. If they input a PR link like `https://github.com/owner/repo/pull/42` and submit, the React app fires a request to `/api/review`:
```json
{
  "prUrl": "https://github.com/owner/repo/pull/42"
}
```

### Step B: The Controller
`ReviewController.cs` receives this payload.
* It checks if `prUrl` is present.
* It routes the execution to `ReviewService.ReviewPullRequestAsync(prUrl)`.

### Step C: The Fetching Phase
Inside `GitHubService.cs`, the URL string is parsed using regular expressions to extract:
* `owner`
* `repo`
* `pull_number`
It sends an HTTP GET request to GitHub's REST API adding the header:
`Accept: application/vnd.github.v3.diff`
This header instructs GitHub to return the pull request's changed code lines as a standard raw Git Diff instead of a JSON metadata structure.

### Step D: Prompt Construction
`PromptService.cs` combines the raw source code (or PR diff) with strict structural formatting instructions. To ensure the LLM returns standard JSON that the backend can parse without errors, the prompt specifies:
* The response must be a JSON object containing an array called `"reviews"`.
* Each item in the array must contain `"filePath"`, `"originalCode"`, `"suggestedCode"`, `"comment"`, `"lineStart"`, and `"lineEnd"`.

### Step E: The LLM Query
`OllamaService.cs` checks `appsettings.json` to decide which free model to use:
* **Gemini (Cloud)**:
  * Sends an HTTP POST to Google's API Endpoint.
  * Uses the **JSON Schema** feature (`responseMimeType: "application/json"`) to force Gemini to output valid JSON matching our class structure.
* **Ollama (Local)**:
  * Sends a request to `http://localhost:11434/api/generate`.
  * Instructs Ollama to return output using JSON formatting.

### Step F: The Response Assembly
`ReviewService.cs` takes the parsed review list from the LLM, attaches the raw source code or diff (`RawCodeOrDiff`), and returns it to the React frontend.
The frontend displays the original code next to the suggested code in the side-by-side diff comparison panel, while showing the full raw code on the left screen.

---

## 4. Key Study Concepts

### 1. Dependency Injection (DI)
All classes are registered in [Program.cs](file:///d:/codereviewer/backend/Program.cs) (e.g., `builder.Services.AddScoped<GitHubService>();`). This allows ASP.NET Core to instantiate services automatically and pass them to controllers and classes, keeping components decoupled and easy to test.

### 2. CORS (Cross-Origin Resource Sharing)
Since the React frontend runs on `http://localhost:5173` and the backend runs on `http://localhost:5000`, the browser would block communications due to origin mismatch. In [Program.cs](file:///d:/codereviewer/backend/Program.cs), the policy `"AllowReactApp"` explicitly authorizes the frontend's origin.

### 3. Stateless Design Benefits
Because there is no database, the app uses very little memory, can run on minimal cloud resources, and does not require complex setup or data migration scripts.
