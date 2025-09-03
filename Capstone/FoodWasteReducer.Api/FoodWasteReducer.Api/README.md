Food Waste Reducer - Backend (ASP.NET Core)

Overview
This minimal API provides two endpoints:
- POST /api/analyze-image: Accepts an image and returns detected ingredient items.
- POST /api/generate-recipe: Accepts detected items and returns a recipe with nutrition info.

Quick start
1) Prereqs
- .NET SDK 9.0+
- An OpenAI API key with access to a vision-capable model (gpt-4o-mini)

2) Configure environment
Export your API key before running:

macOS/Linux (zsh/bash):
export OPENAI_API_KEY="sk-..."

Windows (PowerShell):
$Env:OPENAI_API_KEY = "sk-..."

3) Run
dotnet run --urls http://localhost:5136

4) Test
Open http://localhost:5136/test in your browser.

Endpoints
1. POST /api/analyze-image
   - Request: multipart/form-data with field name image
   - Response: { "items": [ { "name": string, "confidence": number } ] }

2. POST /api/generate-recipe
   - Request: application/json like { "items": ["tomato","egg"], "servings": 2 }
   - Response: Recipe JSON with title, ingredients, steps, servings, nutrition_per_serving

Notes
- CORS is enabled for all origins by default for development convenience.
- Ensure OPENAI_API_KEY is set before calling endpoints.

