[![License](https://img.shields.io/github/license/jrsearles/ElmahCore)](LICENSE)
[![Build](https://github.com/jrsearles/ElmahCore/actions/workflows/build.yml/badge.svg)](https://github.com/jrsearles/ElmahCore/actions/workflows/build.yml)
[![Nuget](https://img.shields.io/nuget/v/Elmah.AspNetCore)](https://www.nuget.org/packages/Elmah.AspNetCore)

# Elmah.AspNetCore

ELMAH (Error Logging Middleware and Handlers) for ASP.NET Core. (Dotnet 6+)

Features include:

- Logging of unhandled exceptions
- Friendly UI to view captured errors and contextual information
- Hooks to include handled exceptions and other contextual information
- [Various ways to persist error logs](#error-persistence)
- [Supports securing UI via built-in ASP.Net Core functionality](#restrict-access-to-the-elmah-ui)
- Integrates with `Microsoft.Extensions.Logging` to capture logs made during a request

> This is a fork of [ElmahCore](https://github.com/ElmahCore/ElmahCore) which is itself a fork of the original [Elmah](https://elmah.github.io/) library. Credit goes to the owners and contributors of those libraries. This fork attempts to align with modern 

![alt text](https://github.com/ElmahCore/ElmahCore/raw/master/images/elmah-new-ui.png)

## Basic usage

**First**, install the _Elmah.AspNetCore_ [Nuget package](https://www.nuget.org/packages/Elmah.AspNetCore) into your app.

```shell
dotnet add package Elmah.AspNetCore
```

**Next**, in your application's _Program.cs_ file, configure Elmah:

```csharp
using Elmah;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseElmah(); // <- Add this to configure Elmah

var app = builder.Build();

app.UseErrorHandler();
app.UseElmahMiddleware(); // <- Add this to register middleware

app.MapElmah(); // <- Add this to register Elmah endpoints
```

`builder.Host.UseElmah()` registers and configures the Elmah services. An overload which accepts an action is available to modify the configuration.

`app.UseElmahMiddleware()` registers the middleware used by Elmah to start capturing errors and contextual information. Only middleware registered after the Elmah middleware will be included in the error capturing. It is recommended that this is included before most other middleware. For best results, call after the built-in `UseExceptionHandler()`.

`app.MapElmah()` registers the routes used to serve content for the Elmah UI. By default these will be under `/elmah`, but the method includes an overload which allows overriding the root path.

## Elmah Options

| Option                | Type     | Default                                 | Description                                                           |
| --------------------- | -------- | --------------------------------------- | --------------------------------------------------------------------- |
| ApplicationName       | string   | ApplicationName from [`IHostEnvironment`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.ihostenvironment?view=dotnet-plat-ext-8.0) | Application name captured in error log                                |
| LogRequestBody        | bool     | `true`                                  | Logs the body of the request                                          |
| LogRequestCookies     | bool     | `true`                                  | Logs the cookie values for the request                                |
| LogRequestForm        | bool     | `true`                                  | Logs the form values for the request                                  |
| LogSqlQueries         | bool     | `true`                                  | Logs SQL queries using "SqlClientDiagnosticListener"                  |
| LogSqlQueryParameters | bool     | `true`                                  | Logs parameter values for the SQL queries captured by `LogSqlQueries` |
| ShowElmahErrorPage    | bool     | `false`                                 | Displays the Elmah UI when an error is captured                       |
| SourcePaths           | string[] | empty                                   | Paths to source code to enrich stack traces                           |

> :information_source: Elmah options work well with environment specific `appsettings` files. A `Configure` method exists on the builder to enable binding configuration to Elmah options.

```json
{
    "Elmah": {
        "ShowElmahErrorPage": true
    }
}
```

```csharp
builder.Host.UseElmah((builderContext, elmah) =>
{
    elmah.Configure(builderContext.Configuration.GetSection("Elmah"));
});
```

## Restrict access to the Elmah UI

The `MapElmah()` method registers the Elmah endpoints as regular endpoints in the application. As such, it can accept authorization policies just like any other endpoints in the application. Metadata can be applied to the returned endpoint collection.

```csharp
// allow all users to access UI
app.MapElmah().AllowAnonymous();

// or require authenticated user
app.MapElmah().RequireAuthorization();
```

> See dotnet documentation for [Authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/introduction?view=aspnetcore-8.0) for additional details.

## Error Persistence

The following persistence options are built into the core package:

- MemoryErrorLog – store errors in memory (default)
- XmlFileErrorLog – store errors in XML files

```csharp
builder.Host.UseElmah((builderContext, elmah) =>
{
  elmah.PersistToFile("~/log"; /* OR "с:\errors" */);
});
```

- SqlErrorLog - store errors in MS SQL (add reference to [ElmahCore.Sql](https://www.nuget.org/packages/ElmahCore.Sql))
- MysqlErrorLog - store errors in MySQL (add reference to [ElmahCore.MySql](https://www.nuget.org/packages/ElmahCore.MySql))
- PgsqlErrorLog - store errors in PostgreSQL (add reference to [ElmahCore.Postgresql](https://www.nuget.org/packages/ElmahCore.Postgresql))

```csharp
builder.Host.UseElmah((builderContext, elmah) =>
{
  elmah.PersistToSql(options =>
  {
    options.ConnectionString = "connection_string";
    options.SqlServerDatabaseSchemaName = "Errors"; //Defaults to dbo if not set
    options.SqlServerDatabaseTableName = "ElmahError"; //Defaults to ELMAH_Error if not set
  });
});
```

You can create implement your own error log persistence by implementing the abstract class `Elmah.ErrorLog` and registered it using the builder method `elmah.PersistTo<YourErrorLog>()`.

## Using UseElmahExceptionPage

Use `UseElmahExceptionPage` (Or the `ShowElmahErrorPage` in Elmah options) to automatically display the Elmah UI diagnostics page when an error is captured.

```csharp
builder.Host.UseElmah((builderContext, elmah) =>
{
    if (builderContext.HostingEnvironment.IsDevelopment())
    {
        elmah.UseElmahExceptionPage();
    }
});
```

> :warning: The Elmah diagnostics page can leak sensitive environmental details. Consider limiting the page to development environments or [placing security on the Elmah endpoints](#restrict-access-to-the-elmah-ui).

## Using Notifiers

You can create your own notifiers by implement IErrorNotifier or IErrorNotifierWithId interface and add notifier to Elmah options:

```csharp
services.AddElmah<XmlFileErrorLog>(options =>
{
    options.Path = @"errors";
    options.LogPath = "~/logs";
    options.Notifiers.Add(new ErrorMailNotifier("Email",emailOptions));
});
```

Each notifier must have unique name.

## Using Filters

You can use Elmah XML filter configuration in separate file, create and add custom filters:

```csharp
services.AddElmah<XmlFileErrorLog>(options =>
{
    options.FiltersConfig = "elmah.xml";
    options.Filters.Add(new MyFilter());
})
```

Custom filter must implement IErrorFilter.
XML filter config example:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<elmah>
	<errorFilter>
		<notifiers>
			<notifier name="Email"/>
		</notifiers>
		<test>
			<and>
				<greater binding="HttpStatusCode" value="399" type="Int32" />
				<lesser  binding="HttpStatusCode" value="500" type="Int32" />
			</and> 
		</test>
	</errorFilter>
</elmah>
```

see more [here](https://elmah.github.io/a/error-filtering/examples/)

JavaScript filters not yet implemented :(

Add notifiers to errorFilter node if you do not want to send notifications
Filtered errors will be logged, but will not be sent.

## Extensions

### Raise an Exception

```csharp
using Elmah.AspNetCore;

public async Task<IActionResult> Test()
{
    await HttpContext.RaiseErrorAsync(new InvalidOperationException("Test"));
}
```

### Logging method parameters

```csharp
using Elmah.AspNetCore;

public void TestMethod(string p1, int p2)
{
    // Logging method parameters
    HttpContext.LogParamsToElmah(p1, p2);
}
```

## Contributing

The Elmah application contains a small [Vue](https://vuejs.org/) frontend which is bundled and embedded into the application when packaged. The source for the frontend is in the `ui` folder. The bundled content is not included in source control. Run `npm install` and then `npm run build` to generate the bundled content locally, which will place the bundled application into the `wwwroot` folder of the `Elmah.AspNetCore` project. Building the Elmah project will then embed the bundled content.

Running `build.ps1` from the root of the repository will run all of these steps and generate the packages locally.
