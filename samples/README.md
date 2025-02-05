# Azure EasyAuth Sample Apps

There are two Blazor web apps &ndash; one is deployed to Azure App Service, and the other is deployed to Azure Container Apps.

## EasyAuth with Entra ID

To use EasyAuth with Entra ID, you should uncomment two places on each app.

1. Open `Program.cs` and uncomment these two lines:

    ```csharp
    builder.Services.AddAuthentication(EasyAuthAuthenticationScheme.Name)
                    .AddAzureEasyAuthHandler<EntraIDEasyAuthAuthenticationHandler>();
    ```

1. Open `Components/Layout/MainLayout.razor` and uncomment the line:

    ```razor
    <a href="/.auth/login/aad?post_login_redirect_uri=/">Login</a>
    ```

## EasyAuth with GitHub

To use EasyAuth with GitHub, you should uncomment two places on each app.

1. Open `Program.cs` and uncomment these two lines:

    ```csharp
    builder.Services.AddAuthentication(EasyAuthAuthenticationScheme.Name)
                    .AddAzureEasyAuthHandler<GitHubEasyAuthAuthenticationHandler>();
    ```

1. Open `Components/Layout/MainLayout.razor` and uncomment the line:

    ```razor
    <a href="/.auth/login/github?post_login_redirect_uri=/">Login</a>
    ```
