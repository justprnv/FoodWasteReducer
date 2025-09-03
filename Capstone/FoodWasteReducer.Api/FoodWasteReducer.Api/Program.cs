var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});
builder.Services.AddHttpClient();

var app = builder.Build();

// Development tooling
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();

// Configuration
var openAiApiKey = builder.Configuration["OPENAI_API_KEY"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
if (string.IsNullOrWhiteSpace(openAiApiKey))
{
    app.Logger.LogWarning("OPENAI_API_KEY is not configured. Endpoints will return 500 until it is set.");
}

const string openAiUrl = "https://api.openai.com/v1/chat/completions";
const string visionModel = "gpt-4o-mini";

// Simple test page for manual testing
app.MapGet("/test", () =>
{
    var html = """
    <!doctype html>
    <html>
    <head><meta charset="utf-8"/><title>Food Waste Reducer Test</title></head>
    <body>
      <h1>Food Waste Reducer - Test</h1>
      <input type="file" id="file" accept="image/*" />
      <button id="analyze">Analyze</button>
      <pre id="items"></pre>
      <button id="recipe">Generate Recipe</button>
      <pre id="recipeOut"></pre>
      <script>
        let lastItems = [];
        document.getElementById('analyze').onclick = async () => {
          const f = document.getElementById('file').files[0];
          const fd = new FormData();
          fd.append('image', f);
          const r = await fetch('/api/analyze-image', { method: 'POST', body: fd });
          const j = await r.json();
          lastItems = (j.items||[]).map(x=>x.name);
          document.getElementById('items').textContent = JSON.stringify(j, null, 2);
        };
        document.getElementById('recipe').onclick = async () => {
          const r = await fetch('/api/generate-recipe', { method: 'POST', headers: {'Content-Type':'application/json'}, body: JSON.stringify({ items: lastItems, servings: 2 }) });
          const j = await r.json();
          document.getElementById('recipeOut').textContent = JSON.stringify(j, null, 2);
        };
      </script>
     </body>
    </html>
    """;
    return Results.Text(html, "text/html");
});

// Analyze Image endpoint
app.MapPost("/api/analyze-image", async (IHttpClientFactory httpClientFactory, HttpRequest request, ILoggerFactory loggerFactory) =>
{
    var logger = loggerFactory.CreateLogger("analyze-image");
    if (string.IsNullOrWhiteSpace(openAiApiKey))
    {
        return Results.Problem("OPENAI_API_KEY not configured", statusCode: 500);
    }

    if (!request.HasFormContentType)
    {
        return Results.BadRequest(new { error = "Expected multipart/form-data with field 'image'" });
    }
    var form = await request.ReadFormAsync();
    var file = form.Files["image"] ?? form.Files.FirstOrDefault();
    if (file == null || file.Length == 0)
    {
        return Results.BadRequest(new { error = "No image uploaded" });
    }

    await using var ms = new MemoryStream();
    await file.CopyToAsync(ms);
    var bytes = ms.ToArray();
    var base64 = Convert.ToBase64String(bytes);
    var mime = string.IsNullOrWhiteSpace(file.ContentType) ? "image/jpeg" : file.ContentType;
    var dataUrl = $"data:{mime};base64,{base64}";

    var systemPrompt = "You are an expert food vision assistant. Identify distinct edible food items and common ingredients visible in the image. Return concise names. Estimate confidence 0-1.";
    var userInstruction = "Identify items. Return strictly the JSON schema: { items: [ { name: string, confidence: number } ] }";

    var body = new
    {
        model = visionModel,
        messages = new object[]
        {
            new { role = "system", content = systemPrompt },
            new { role = "user", content = new object[]
                {
                    new { type = "text", text = userInstruction },
                    new { type = "image_url", image_url = new { url = dataUrl } }
                }
            }
        },
        response_format = new
        {
            type = "json_schema",
            json_schema = new
            {
                name = "detected_items",
                schema = new
                {
                    type = "object",
                    additionalProperties = false,
                    required = new[] { "items" },
                    properties = new
                    {
                        items = new
                        {
                            type = "array",
                            items = new
                            {
                                type = "object",
                                additionalProperties = false,
                                required = new[] { "name", "confidence" },
                                properties = new
                                {
                                    name = new { type = "string" },
                                    confidence = new { type = "number", minimum = 0, maximum = 1 }
                                }
                            }
                        }
                    }
                }
            }
        }
    };

    var http = httpClientFactory.CreateClient();
    var req = new HttpRequestMessage(HttpMethod.Post, openAiUrl);
    req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", openAiApiKey);
    req.Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(body), System.Text.Encoding.UTF8, "application/json");

    try
    {
        var resp = await http.SendAsync(req);
        var respText = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode)
        {
            logger.LogError("OpenAI error {Status}: {Body}", resp.StatusCode, respText);
            return Results.Problem("OpenAI request failed", statusCode: 502);
        }

        using var doc = System.Text.Json.JsonDocument.Parse(respText);
        var content = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        if (string.IsNullOrWhiteSpace(content))
        {
            return Results.Ok(new AnalyzeImageResponse(new List<DetectedItem>()));
        }
        var parsed = System.Text.Json.JsonSerializer.Deserialize<AnalyzeImageResponse>(content, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return Results.Ok(parsed ?? new AnalyzeImageResponse(new List<DetectedItem>()));
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Analyze image failed");
        return Results.Problem("Analyze image failed", statusCode: 500);
    }
});

// Generate Recipe endpoint
app.MapPost("/api/generate-recipe", async (IHttpClientFactory httpClientFactory, GenerateRecipeRequest input, ILoggerFactory loggerFactory) =>
{
    var logger = loggerFactory.CreateLogger("generate-recipe");
    if (string.IsNullOrWhiteSpace(openAiApiKey))
    {
        return Results.Problem("OPENAI_API_KEY not configured", statusCode: 500);
    }
    if (input.items == null || input.items.Count == 0)
    {
        return Results.BadRequest(new { error = "Provide items[]" });
    }
    var servings = input.servings ?? 2;

    var systemPrompt = "You are a chef and nutritionist. Create practical recipes minimizing food waste, using only provided items plus pantry staples (oil, salt, pepper, water).";
    var userInstruction = $"Items: {string.Join(", ", input.items)}. Create one recipe with title, ingredient list with quantities, clear steps, servings={servings}, and nutrition per serving (calories, protein_g, carbs_g, fat_g). Output strictly JSON matching schema.";

    var body = new
    {
        model = visionModel,
        messages = new object[]
        {
            new { role = "system", content = systemPrompt },
            new { role = "user", content = userInstruction }
        },
        response_format = new { type = "json_object" }
    };

    var http = httpClientFactory.CreateClient();
    var req = new HttpRequestMessage(HttpMethod.Post, openAiUrl);
    req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", openAiApiKey);
    req.Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(body), System.Text.Encoding.UTF8, "application/json");

    try
    {
        var resp = await http.SendAsync(req);
        var respText = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode)
        {
            logger.LogError("OpenAI error {Status}: {Body}", resp.StatusCode, respText);
            return Results.Problem("OpenAI request failed", statusCode: 502);
        }
        using var doc = System.Text.Json.JsonDocument.Parse(respText);
        var content = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        if (string.IsNullOrWhiteSpace(content))
        {
            return Results.Problem("Empty response", statusCode: 502);
        }
        var recipe = System.Text.Json.JsonSerializer.Deserialize<RecipeResult>(content, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return Results.Ok(recipe);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Generate recipe failed");
        return Results.Problem("Generate recipe failed", statusCode: 500);
    }
});

app.Run();

// DTOs (types must come after all top-level statements)
record DetectedItem(string name, double confidence);
record AnalyzeImageResponse(List<DetectedItem> items);
record GenerateRecipeRequest(List<string> items, int? servings);
record RecipeNutrition(double calories, double protein_g, double carbs_g, double fat_g);
record RecipeResult(string title, List<string> ingredients, List<string> steps, int servings, RecipeNutrition nutrition_per_serving);
