# api-test-host
An ASP.NET Core Host that does some specific stuff to test APIs.

## Build Status
[![Build Status](https://masch0212.visualstudio.com/MaSch/_apis/build/status/MaSch0212.api-test-host?branchName=main)](https://masch0212.visualstudio.com/MaSch/_build/latest?definitionId=6&branchName=main)

## Getting Started
Make sure you have .NET 5.0 runtime installed.

Download the [latest release](https://github.com/MaSch0212/api-test-host/releases/latest) and unzip the files to a directory of your choice.

To start the server just start the `ApiTestHost.exe` on Windows or run `dotnet ApiTestHost.dll` on Linux or macOS.

For more information about the endpoints you can open `http://localhost:8426` in your browser to see the Swagger UI (API documentation).

### JSON Server
The json server will serve json files and can be accessed using the "/json/{url}" endpoint. The following HTTP methods are available for this endpoint:
- **Get**: Get the content of the specific json document<br>
- **Delete**: Delete the specific json document
- **Post**/**Put**/**Post**: Add or overwrite the specific json document

All json documents are saved as "*.json" files in the "json" subfolder right next to the `ApiTestHost.dll` file. You can add/remove/edit files there or use the provided HTTP endpoints.

**Example**: `http://localhost:8426/json/.well-known/my-app`

### Delay Server
The delay server can be used to simulate very slow connections and can be accessed using the "/delay/{delay}/{url}" endpoint. All HTTP methods are available and they will just use this information to call the wanted url. All headers and the body of the request are also used for the call of the wanted url.

**Example**: `http://localhost:8426/delay/5000/http://my-server/my-app/api/test?test=true` will call the endpoint `http://my-server/my-app/api/test?test=true` after waiting 5 seconds.

## Server Configuration

### Hosting
The hosting can be configured using the `appsettings.json`.

1. **Port**: change the endpoint under `Kestrel/Endpoints/Http/Url`
2. **HTTPS**: add a `Https` configuration to `Kestrel/Endpoints` and add a certificate like this:
    ``` JSON
    {
      "Kestrel": {
        "Endpoints": {
          "Http": {
            "Url": "http://localhost:5000"
          },
          "Https": {
            "Url": "https://localhost:5001",
            "Certificate": {
              "Path": "<path to .pfx file>",
              "Password": "<certificate password>"
            }
          }
        }
      }
    }
    ```

More information about Kestrel configuration can be found here: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-5.0