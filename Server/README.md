# Spotify Stats API

ASP.NET Core Web API backend for the Spotify Stats application.

## Setup

### Prerequisites
- .NET 9.0 SDK or later
- Spotify Developer Account

### Spotify App Configuration

1. Go to [Spotify Developer Dashboard](https://developer.spotify.com/dashboard)
2. Create a new app
3. Note your **Client ID** and **Client Secret**
4. Add `http://localhost:5000/api/auth/callback` to your app's Redirect URIs

### Configuration

Update the Spotify credentials in `appsettings.Development.json`:

```json
"Spotify": {
  "ClientId": "your_actual_client_id",
  "ClientSecret": "your_actual_client_secret",
  "RedirectUri": "http://localhost:5000/api/auth/callback"
}
```

## Running the API

```bash
cd server/SpotifyStats.API
dotnet run
```

The API will be available at `https://localhost:5001` or `http://localhost:5000`

## API Endpoints

### Authentication

- **GET** `/api/auth/login` - Initiates Spotify OAuth flow
  - Returns: `{ authUrl: string }` - URL to redirect user for Spotify login

- **GET** `/api/auth/callback?code={code}` - Callback endpoint for Spotify OAuth
  - Query Params: `code` (authorization code from Spotify)
  - Returns: Authentication result

- **POST** `/api/auth/logout` - Logs out the current user
  - Returns: `{ message: string }`

## CORS

The API is configured to accept requests from:
- `http://localhost:4200` (Angular development server)

## Development

The API uses:
- Controller-based routing
- OpenAPI/Swagger for API documentation (available in development mode)
- CORS enabled for local Angular client
