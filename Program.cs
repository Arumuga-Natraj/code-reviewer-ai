using CodeReviewer.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure CORS to allow local React development server
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add MVC Controllers
builder.Services.AddControllers();

// Add API Explorer & Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register HTTP Client
builder.Services.AddHttpClient();

// Register Custom Services (DI)
builder.Services.AddSingleton<PromptService>();
builder.Services.AddScoped<GitHubService>();
builder.Services.AddScoped<OllamaService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReactApp");

app.UseAuthorization();

// Map Controllers
app.MapControllers();

// Health-check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.Run();
