# Elmah.AspNetCore

ELMAH (Error Logging Middleware and Handlers) for ASP.NET Core. (dotnet 6, 7, 8)

Features include:

- Logging of unhandled exceptions
- Hooks to include handled exceptions and other contextual information
- Captures of logs

This is a fork of [ElmahCore](https://github.com/ElmahCore/ElmahCore). Credit goes to the owners and contributors of that library. This fork attempts to catch up the library with ongoing changes in the dotnet releases and follow established conventions and practices for integegration.

![alt text](https://github.com/ElmahCore/ElmahCore/raw/master/images/elmah-new-ui.png)

Add Nuget package **Elmah.AspNetCore**

## Simple usage

*First*, install the _Elmah.AspNetCore_ [Nuget package](https://www.nuget.org/packages/Elmah.AspNetCore) into your app.

```shell
dotnet add package Elmah.AspNetCore
```

Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

// This call wires up services needed by Elmah
builder.Services.AddElmah();

var app = builder.Build();

app.UseElmah();

app.MapElmah();
```

`app.UseElmah()` registers the Elmah middleware which will capture any uncaught errors in subsequent middleware. It is recommended that this is included before most other middleware. For best results, call after the built-in `UseExceptionHandler`.

`app.MapElmah()` registers the routes used to serve content for the Elmah UI. By default these will be under `/elmah`, but the method includes an overload which allows overriding the root path. 

## Restrict access to the Elmah url

The `MapElmah()` registers the Elmah endpoints as regular endpoints in the application. As such, it will pick up the default authorization & authentication policies used by the application. Metadata can be applied to the returned endpoint builder to customize this.

```csharp
// allow all users to access UI
app.MapElmah().AllowAnonymous();

// require authenticated user
app.MapElmah().RequireAuthorization();
```

> See dotnet documentation for [Authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/introduction?view=aspnetcore-8.0) for additional details.

## Change Error Log type
You can create your own error log, which will store errors anywhere.

```csharp
    class MyErrorLog : ErrorLog
    //implement ErrorLog
```

This ErrorLogs available in board:
- MemoryErrorLog – store errors in memory (by default)
- XmlFileErrorLog – store errors in XML files
- SqlErrorLog - store errors in MS SQL (add reference to [ElmahCore.Sql](https://www.nuget.org/packages/ElmahCore.Sql))
- MysqlErrorLog - store errors in MySQL (add reference to [ElmahCore.MySql](https://www.nuget.org/packages/ElmahCore.MySql))
- PgsqlErrorLog - store errors in PostgreSQL (add reference to [ElmahCore.Postgresql](https://www.nuget.org/packages/ElmahCore.Postgresql))

```csharp
services.AddElmah(elmah =>
{
  elmah.PersistToFile("~/log"; /* OR "с:\errors" */);
});
```

```csharp
services.AddElmah<SqlErrorLog>(options =>
{
    options.ConnectionString = "connection_string";
    options.SqlServerDatabaseSchemaName = "Errors"; //Defaults to dbo if not set
    options.SqlServerDatabaseTableName = "ElmahError"; //Defaults to ELMAH_Error if not set
});
```
## Raise exception
```csharp
public IActionResult Test()
{
    HttpContext.RaiseError(new InvalidOperationException("Test"));
    ...
}
```
## Microsoft.Extensions.Logging support
Since version 2.0 ElmahCore support Microsoft.Extensions.Logging
![alt text](https://github.com/ElmahCore/ElmahCore/raw/master/images/elmah-log.png)

## Source Preview
Since version 2.0.1 ElmahCore support source preview.
Just add paths to source files.
```csharp
services.AddElmah(options =>
{
   options.SourcePaths = new []
   {
      @"D:\tmp\ElmahCore.DemoCore3",
      @"D:\tmp\ElmahCore.Mvc",
      @"D:\tmp\ElmahCore"
   };
});
```

## Log the request body
Since version 2.0.5 ElmahCore can log the request body.

## Logging SQL request body
Since version 2.0.6 ElmahCore can log the SQL request body.
![alt text](https://github.com/ElmahCore/ElmahCore/raw/master/images/elmah-4.png)

## Logging method parameters
Since version 2.0.6 ElmahCore can log method parameters.
![alt text](https://github.com/ElmahCore/ElmahCore/raw/master/images/elmah-5.png)
```csharp
using ElmahCore;
...

public void TestMethod(string p1, int p2)
{
    // Logging method parameters
    this.LogParams((nameof(p1), p1), (nameof(p2), p2));
    ...
}

```

## Using UseElmahExceptionPage
You can replace UseDeveloperExceptionPage to UseElmahExceptionPage
```csharp
if (env.IsDevelopment())
{
   //app.UseDeveloperExceptionPage();
   app.UseElmahExceptionPage();
}
```

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
```csharp
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

JavaScript filters not yet impemented :(

Add notifiers to errorFilter node if you do not want to send notifications
Filtered errors will be logged, but will not be sent.

## Search And Filters

Since version 2.2.0 tou can use full-text search and multiple filter.

Full-text search work on analyzed text fields.

![alt text](https://github.com/ElmahCore/ElmahCore/raw/master/images/elmah-filters-1.png)

Filters are available through either the **Add filter** button.

![alt text](https://github.com/ElmahCore/ElmahCore/raw/master/images/elmah-filters-2.png)

Or you can use **filter icon** to the right of the error field.

![alt text](https://github.com/ElmahCore/ElmahCore/raw/master/images/elmah-filters-3.png)

Currently supports only Memory and XmlFile error logs.