import React, { useState } from 'react';

const BACKEND_URL = 'http://localhost:5000';

function App() {
  const [prUrl, setPrUrl] = useState('');
  const [codeContent, setCodeContent] = useState('');
  const [fileName, setFileName] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [reviewResult, setReviewResult] = useState(null);

  const handleFileChange = (e) => {
    const file = e.target.files[0];
    if (file) {
      setFileName(file.name);
      const reader = new FileReader();
      reader.onload = (event) => {
        setCodeContent(event.target.result);
        setPrUrl(''); // Reset PR URL since we are doing file upload
      };
      reader.readAsText(file);
    }
  };

  const handleReview = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    setReviewResult(null);

    const payload = prUrl.trim()
      ? { prUrl: prUrl.trim() }
      : { codeContent, fileName: fileName || 'UploadedFile.cs' };

    try {
      const response = await fetch(`${BACKEND_URL}/api/review`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload),
      });

      if (!response.ok) {
        const errData = await response.json();
        throw new Error(errData?.error || errData?.detail || `Error ${response.status}: ${response.statusText}`);
      }

      const data = await response.json();
      setReviewResult(data);
    } catch (err) {
      console.error(err);
      setError(err.message || 'An unexpected error occurred connecting to the backend.');
    } finally {
      setLoading(false);
    }
  };

  const downloadReport = () => {
    if (!reviewResult || !reviewResult.reviews) return;

    let markdown = `# AI Code Review Report\n\n`;
    markdown += `Generated on: ${new Date().toLocaleString()}\n`;
    if (prUrl) {
      markdown += `Source: Pull Request [${prUrl}](${prUrl})\n\n`;
    } else {
      markdown += `Source File: ${fileName || 'Uploaded File'}\n\n`;
    }

    markdown += `## Summary of Review Comments\n\n`;

    reviewResult.reviews.forEach((review, index) => {
      markdown += `### File: \`${review.filePath}\` (Suggestion #${index + 1})\n`;
      if (review.lineStart) {
        markdown += `**Lines**: ${review.lineStart} - ${review.lineEnd || review.lineStart}\n`;
      }
      markdown += `**Recommendation**: ${review.comment}\n\n`;
      markdown += `#### Original Code:\n\`\`\`\n${review.originalCode}\n\`\`\`\n\n`;
      markdown += `#### Suggested Code:\n\`\`\`\n${review.suggestedCode}\n\`\`\`\n\n`;
      markdown += `---\n\n`;
    });

    const blob = new Blob([markdown], { type: 'text/markdown;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.setAttribute('download', `code_review_report_${Date.now()}.md`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  return (
    <div className="app-container">
      <header className="app-header">
        <h1>🔍 Stateless AI Code Reviewer</h1>
        <p className="subtitle">Instant feedback and side-by-side code suggestions. No database required.</p>
      </header>

      <main className="app-content">
        <section className="config-section card">
          <h3>🔧 Start Code Review</h3>
          <form onSubmit={handleReview}>
            <div className="input-group">
              <label htmlFor="prUrl">GitHub Public Pull Request URL</label>
              <input
                id="prUrl"
                type="url"
                value={prUrl}
                onChange={(e) => {
                  setPrUrl(e.target.value);
                  setCodeContent('');
                  setFileName('');
                }}
                placeholder="e.g. https://github.com/owner/repo/pull/42"
              />
            </div>

            <div className="divider"><span>OR</span></div>

            <div className="input-group">
              <label htmlFor="fileUpload">Upload Code File</label>
              <input
                id="fileUpload"
                type="file"
                onChange={handleFileChange}
              />
              {fileName && <p className="file-indicator">Loaded: <code>{fileName}</code></p>}
            </div>

            <button type="submit" className="btn btn-primary" disabled={loading || (!prUrl && !codeContent)}>
              {loading ? 'Analyzing Code...' : 'Start Code Review'}
            </button>
          </form>
        </section>

        {error && (
          <section className="error-section card">
            <p>⚠️ {error}</p>
          </section>
        )}

        {loading && (
          <div className="loading-container">
            <div className="spinner"></div>
            <p>Gemini is reviewing your code changes. Please wait...</p>
          </div>
        )}

        {/* Live Preview Panel before submit (only when file is uploaded) */}
        {!reviewResult && !loading && codeContent && (
          <section className="live-preview-section card">
            <h3>📄 File Preview: <code>{fileName}</code></h3>
            <pre className="code-pre">
              <code>{codeContent}</code>
            </pre>
          </section>
        )}

        {reviewResult && (
          <div className="dashboard-split-layout">
            {/* Left Side: Code Source Viewer */}
            <section className="code-viewer-panel card">
              <h3>📄 Source Code / Diff Under Review</h3>
              <pre className="code-pre">
                <code>{reviewResult.rawCodeOrDiff || codeContent}</code>
              </pre>
            </section>

            {/* Right Side: Suggestions & Reports */}
            <section className="results-section">
              <div className="results-header card">
                <h2>Review Results ({reviewResult.reviews?.length || 0} issues found)</h2>
                {reviewResult.reviews?.length > 0 && (
                  <button onClick={downloadReport} className="btn btn-secondary">
                    📥 Download Report (.md)
                  </button>
                )}
              </div>

              {(!reviewResult.reviews || reviewResult.reviews.length === 0) ? (
                <div className="card empty-card">
                  <p>🎉 Gemini found no issues! The code looks solid.</p>
                </div>
              ) : (
                reviewResult.reviews.map((review, idx) => (
                  <div key={idx} className="review-card card">
                    <div className="review-card-header">
                      <span className="file-badge">{review.filePath}</span>
                      {review.lineStart && (
                        <span className="line-badge">Lines {review.lineStart}-{review.lineEnd || review.lineStart}</span>
                      )}
                    </div>
                    
                    <div className="review-comment">
                      <p>💡 {review.comment}</p>
                    </div>

                    <div className="diff-container">
                      <div className="diff-pane original">
                        <div className="diff-pane-title">Original Code</div>
                        <pre><code>{review.originalCode || '(empty/new file)'}</code></pre>
                      </div>
                      <div className="diff-pane suggested">
                        <div className="diff-pane-title">Suggested Improvement</div>
                        <pre><code>{review.suggestedCode}</code></pre>
                      </div>
                    </div>
                  </div>
                ))
              )}
            </section>
          </div>
        )}
      </main>
    </div>
  );
}

export default App;
