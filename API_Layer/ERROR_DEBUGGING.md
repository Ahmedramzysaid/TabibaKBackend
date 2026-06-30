# Error handling and debugging (Tabibak API)

This project is **ASP.NET Core**. When hosted on IIS, the web.config includes `<customErrors mode="Off"/>` so you can see the **real error** when something fails (e.g. database connection).

## "Server Error in '/' Application – custom error settings prevent details from being viewed remotely"

That message usually means one of:

1. **App failed to start** (e.g. 502.5 on IIS) – IIS shows its own error page. Fix: see *IIS / startup errors* below.
2. **Database error when using the app** – e.g. wrong connection string, SQL Server not running, or login failed. The web.config in this project has **`<customErrors mode="Off"/>`** so after redeploying you should see the actual exception message (e.g. "Cannot open database", "Login failed") instead of the generic one.
3. **You are on classic ASP.NET** – This API is ASP.NET Core; if you see this on a different app, use `<customErrors mode="Off"/>` in web.config only for local debugging.

## How errors work in this API

| Environment   | Unhandled exception behavior |
|---------------|------------------------------|
| **Development** | Full exception details in JSON response and Developer Exception Page in browser. |
| **Production**  | Generic message in JSON (`"An unexpected error occurred..."`); details only in logs. |

- **GlobalErrorHandling** (in code) returns JSON and shows details only when `IsDevelopment()`.
- **UseDeveloperExceptionPage()** runs only in Development and shows the detailed yellow error page in the browser.

## Seeing detailed errors when debugging

1. **Run with Development environment**
   - Set `ASPNETCORE_ENVIRONMENT=Development` (launchSettings.json, or env var when running `dotnet run`).
2. **Local run**
   - From API_Layer: `dotnet run` – then any unhandled exception returns detailed JSON and (in browser) the developer exception page.
3. **IIS**
   - In the site’s web.config `<aspNetCore>` add (temporarily):
     - `<environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Development" />`
   - Restart the app pool. Switch back to `Production` when done debugging.

## IIS / startup errors (502.5 or generic “Server Error”)

1. **web.config**
   - `stdoutLogEnabled="true"` is set so startup errors are written to the `logs` folder (e.g. `.\logs\stdout_*.log`). Check that folder after a failed start.
2. **Connection string**
   - Ensure appsettings (or appsettings.Production.json) has a valid connection string and the app can reach the DB.
3. **.NET runtime**
   - On the server, install the same .NET version as the project (e.g. .NET 9) and ensure the app pool uses “No Managed Code” and the correct identity.

## Production-safe settings

- **Do not** set `ASPNETCORE_ENVIRONMENT=Development` on production.
- **Do not** enable Developer Exception Page in Production (it is already gated by `IsDevelopment()`).
- In production, **stdoutLogEnabled** can be set to `false` if you rely only on your normal logging (e.g. Serilog).
- **httpErrors existingResponse="PassThrough"** in web.config is fine in production so your API’s JSON error response is returned instead of an IIS HTML error page.
