# 🔍 Stateless AI Code Reviewer

An automated, stateless code reviewer platform built with **.NET 8 (ASP.NET Core Web API)** and **React (Vite)**. It analyzes code files and public GitHub Pull Requests on-demand, generating side-by-side code suggestions and downloadable markdown reports. 

No database or complex server setups are required!

---

## 🚀 Features

*   **GitHub Integration**: Fetch and review pull request code differences using public GitHub APIs (no OAuth/token registration required for public repositories).
*   **Direct File Upload**: Upload and preview code files directly from your system to run instant reviews.
*   **Side-by-Side Comparison**: Displays the original code and the AI's suggested improvements next to each other.
*   **Structured Feedback**: Identifies bugs, performance issues, security flaws, and best practices.
*   **Downloadable Reports**: Download the complete code review feedback as a structured Markdown (`.md`) file.
*   **Flexible LLM Support**: Supports both **Google Gemini API (free tier)** and **local Ollama** (e.g., `codellama`, `deepseek-coder`).

---

## 🛠️ Project Structure

```
code-reviewer-ai/
├── backend/                  # ASP.NET Core MVC/Service Web API
│   ├── Controllers/          # REST Controller endpoints
│   ├── Services/             # GitHub fetchers, Prompt Builders, & LLM integrations
│   ├── Models/               # Data Transfer Objects (DTOs)
│   ├── Utils/                # Path and file helpers
│   ├── Program.cs            # App configuration and Dependency Injection setup
│   └── appsettings.json      # Key configurations (Gemini/Ollama setups)
│
├── frontend/                 # React Single Page App (Vite)
│   ├── src/
│   │   ├── App.jsx           # Form states, API client, and split-screen layout
│   │   ├── App.css           # Premium dark-theme styling
│   │   └── main.jsx          # Entry point
│   ├── index.html
│   └── package.json
│
├── project_documentation.md  # Detailed project study guide and flowcharts
└── README.md                 # Project introduction and user guide
```

---

## ⚙️ Setup & Installation

### 1. Prerequisites
*   [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
*   [Node.js](https://nodejs.org/) (v18 or higher)
*   *(Optional)* [Ollama](https://ollama.com/) (For offline local model execution)

---

### 2. Backend Setup (`/backend`)

1. Open your terminal in the `backend` folder:
   ```bash
   cd backend
   ```

2. Configure your LLM Provider in `appsettings.json`:
   * **To use Google Gemini (Cloud)**:
     1. Get a free API key from [Google AI Studio](https://aistudio.google.com/).
     2. Add your key to your environment variables as `GEMINI_API_KEY`, or paste it inside `appsettings.json`:
        ```json
        "LLM": { "Provider": "gemini" },
        "Gemini": {
          "ApiKey": "YOUR_GEMINI_API_KEY",
          "Model": "gemini-2.5-flash"
        }
        ```
   * **To use Ollama (Local)**:
     1. Set the provider to `"ollama"`.
     2. Run `ollama run codellama` (or any model of your choice) locally.
        ```json
        "LLM": { "Provider": "ollama" },
        "Ollama": {
          "Endpoint": "http://localhost:11434/api/generate",
          "Model": "codellama"
        }
        ```

3. Run the backend Web API server:
   ```bash
   dotnet run
   ```
   *The backend will default to `http://localhost:5000`.*

---

### 3. Frontend Setup (`/frontend`)

1. Open a new terminal in the `frontend` folder:
   ```bash
   cd frontend
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Launch the Vite development server:
   ```bash
   npm run dev
   ```
   *The frontend will run at `http://localhost:5173`.*

---

## 📈 Study Guide & Architecture Flow

For a comprehensive review of the project architecture, flowcharts, data flows, and code designs, please refer to our internal [Project Study Guide / Documentation](project_documentation.md).
