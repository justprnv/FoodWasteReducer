# 🍽️ Food Waste Reducer API

<div align="center">

![Food Waste Reducer](https://img.shields.io/badge/Food%20Waste-Reducer-green?style=for-the-badge&logo=recycle)
![.NET](https://img.shields.io/badge/.NET-9.0-blue?style=for-the-badge&logo=dotnet)
![OpenAI](https://img.shields.io/badge/OpenAI-GPT--4o--mini-orange?style=for-the-badge&logo=openai)

**An intelligent API that analyzes food images and generates recipes to reduce food waste**

[🚀 Quick Start](#-quick-start) • [📋 Prerequisites](#-prerequisites) • [🔧 Installation](#-installation) • [📖 API Documentation](#-api-documentation) • [🧪 Testing](#-testing)

</div>

---

## 📖 Overview

The **Food Waste Reducer API** is a powerful ASP.NET Core minimal API that leverages OpenAI's vision capabilities to:

- 🔍 **Analyze food images** and identify ingredients with confidence scores
- 👨‍🍳 **Generate smart recipes** using detected ingredients to minimize food waste
- 📊 **Provide nutritional information** for each generated recipe
- 🌐 **Serve a test interface** for easy manual testing

### ✨ Key Features

- 🖼️ **Image Analysis**: Upload food images to detect ingredients automatically
- 🍳 **Recipe Generation**: Create practical recipes using available ingredients
- 📈 **Nutrition Tracking**: Get detailed nutritional information per serving
- 🧪 **Built-in Testing**: Interactive web interface for API testing
- 🔒 **Secure**: Environment-based API key management
- 🌍 **CORS Enabled**: Ready for frontend integration

---

## 📋 Prerequisites

Before you begin, ensure you have the following installed:

### Required Software
- **.NET SDK 9.0+** - [Download here](https://dotnet.microsoft.com/download)
- **OpenAI API Key** with access to `gpt-4o-mini` model
- **Git** (for cloning the repository)

### System Requirements
- **Operating System**: Windows, macOS, or Linux
- **Memory**: Minimum 4GB RAM
- **Storage**: At least 1GB free space
- **Network**: Internet connection for OpenAI API calls

---

## 🔧 Installation

### Step 1: Clone the Repository
```bash
git clone <repository-url>
cd FoodWasteReducer.Api/FoodWasteReducer.Api
```

### Step 2: Restore Dependencies
```bash
dotnet restore
```

### Step 3: Configure Environment Variables

#### 🍎 macOS/Linux (zsh/bash)
```bash
export OPENAI_API_KEY="sk-your-openai-api-key-here"
```

#### 🪟 Windows (PowerShell)
```powershell
$Env:OPENAI_API_KEY = "sk-your-openai-api-key-here"
```

#### 🪟 Windows (Command Prompt)
```cmd
set OPENAI_API_KEY=sk-your-openai-api-key-here
```

#### 🔧 Alternative: Using appsettings.json
You can also add your API key to `appsettings.json`:
```json
{
  "OPENAI_API_KEY": "sk-your-openai-api-key-here",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Step 4: Build the Project
```bash
dotnet build
```

### Step 5: Run the Application
```bash
dotnet run --urls http://localhost:5136
```

🎉 **Success!** Your API is now running at `http://localhost:5136`

---

## 🧪 Testing

### Option 1: Built-in Test Interface
1. Open your browser and navigate to: `http://localhost:5136/test`
2. Upload an image using the file input
3. Click "Analyze" to detect ingredients
4. Click "Generate Recipe" to create a recipe

### Option 2: Using cURL

#### Test Image Analysis
```bash
curl -X POST "http://localhost:5136/api/analyze-image" \
  -H "Content-Type: multipart/form-data" \
  -F "image=@/path/to/your/image.jpg"
```

#### Test Recipe Generation
```bash
curl -X POST "http://localhost:5136/api/generate-recipe" \
  -H "Content-Type: application/json" \
  -d '{
    "items": ["tomato", "onion", "garlic"],
    "servings": 2
  }'
```

### Option 3: Using Postman
1. Import the API endpoints from `FoodWasteReducer.Api.http`
2. Set up environment variables for the base URL
3. Test both endpoints with sample data

---

## 📖 API Documentation

### Base URL
```
http://localhost:5136
```

### Endpoints

#### 🔍 POST `/api/analyze-image`
Analyzes an uploaded image to detect food ingredients.

**Request:**
- **Content-Type**: `multipart/form-data`
- **Body**: Form data with field name `image` containing the image file

**Response:**
```json
{
  "items": [
    {
      "name": "tomato",
      "confidence": 0.95
    },
    {
      "name": "onion",
      "confidence": 0.87
    }
  ]
}
```

**Example cURL:**
```bash
curl -X POST "http://localhost:5136/api/analyze-image" \
  -H "Content-Type: multipart/form-data" \
  -F "image=@food_image.jpg"
```

#### 👨‍🍳 POST `/api/generate-recipe`
Generates a recipe using the provided ingredients.

**Request:**
- **Content-Type**: `application/json`
- **Body:**
```json
{
  "items": ["tomato", "onion", "garlic"],
  "servings": 2
}
```

**Response:**
```json
{
  "title": "Fresh Tomato and Onion Salad",
  "ingredients": [
    "2 large tomatoes, diced",
    "1 medium onion, thinly sliced",
    "2 cloves garlic, minced",
    "2 tbsp olive oil",
    "Salt and pepper to taste"
  ],
  "steps": [
    "Dice the tomatoes into bite-sized pieces",
    "Thinly slice the onion",
    "Mince the garlic",
    "Combine all ingredients in a bowl",
    "Drizzle with olive oil and season with salt and pepper",
    "Toss gently and serve immediately"
  ],
  "servings": 2,
  "nutrition_per_serving": {
    "calories": 85.5,
    "protein_g": 2.1,
    "carbs_g": 12.3,
    "fat_g": 3.8
  }
}
```

**Example cURL:**
```bash
curl -X POST "http://localhost:5136/api/generate-recipe" \
  -H "Content-Type: application/json" \
  -d '{
    "items": ["tomato", "onion", "garlic"],
    "servings": 2
  }'
```

#### 🧪 GET `/test`
Serves a built-in test interface for manual API testing.

**Response:** HTML page with interactive form for testing both endpoints.

---

## ⚙️ Configuration

### Environment Variables
| Variable | Description | Required | Default |
|----------|-------------|----------|---------|
| `OPENAI_API_KEY` | Your OpenAI API key | ✅ Yes | - |
| `ASPNETCORE_ENVIRONMENT` | Environment (Development/Production) | ❌ No | Development |

### CORS Configuration
The API is configured with permissive CORS settings for development:
- **Origins**: All origins allowed
- **Headers**: All headers allowed  
- **Methods**: All methods allowed

⚠️ **Production Note**: Update CORS settings for production deployment.

---

## 🚀 Deployment

### Development
```bash
dotnet run --urls http://localhost:5136
```

### Production
```bash
dotnet publish -c Release -o ./publish
cd publish
dotnet FoodWasteReducer.Api.dll --urls http://0.0.0.0:80
```

### Docker (Optional)
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["FoodWasteReducer.Api.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FoodWasteReducer.Api.dll"]
```

---

## 🛠️ Troubleshooting

### Common Issues

#### ❌ "OPENAI_API_KEY not configured"
**Solution**: Ensure your OpenAI API key is properly set as an environment variable or in `appsettings.json`.

#### ❌ "OpenAI request failed"
**Possible Causes**:
- Invalid API key
- Insufficient API credits
- Network connectivity issues
- Rate limiting

**Solutions**:
- Verify your API key is correct
- Check your OpenAI account balance
- Ensure stable internet connection
- Wait and retry if rate limited

#### ❌ "No image uploaded"
**Solution**: Ensure you're sending the image with the correct field name `image` in your multipart form data.

#### ❌ Build errors
**Solution**: Ensure you have .NET SDK 9.0+ installed and run `dotnet restore` before building.

### Getting Help
1. Check the application logs for detailed error messages
2. Verify all prerequisites are installed
3. Ensure environment variables are properly set
4. Test with the built-in test interface first

---

## 📝 License

This project is part of a capstone assignment. Please refer to your institution's guidelines for usage and distribution.

---

## 🤝 Contributing

This is a capstone project. For questions or issues, please contact the development team.

---

<div align="center">

**Made with ❤️ for reducing food waste**

[⬆️ Back to Top](#-food-waste-reducer-api)

</div>
