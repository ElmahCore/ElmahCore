This project is licensed under the terms of the Apache license 2.0.

# Using ElmahCore
ELMAH for Net.Standard and Net.Core (3.1, 5, 6)

![alt text](https://github.com/ElmahCore/ElmahCore/raw/master/images/elmah-new-ui.png)

Add nuget package **elmahcore**

## Simple usage
 Startup.cs
```csharp
1)	services.AddElmah() in ConfigureServices 
2)	app.UseElmah(); in Configure
```
`app.UseElmah()` must be after initializing other exception handling middleware, such as (UseExceptionHandler, UseDeveloperExceptionPage, etc.)

Default elmah path `~/elmah`.

## Change URL path
```csharp
services.AddElmah(options => options.Path = "you_path_here")
```
## Restrict access to the Elmah url
```csharp
services.AddElmah(options =>
{
        options.OnPermissionCheck = context => context.User.Identity.IsAuthenticated;
});
```
**Note:** `app.UseElmah();` needs to be after 
```
app.UseAuthentication();
app.UseAuthorization();
app.UseElmah();
```
or the user will be redirected to the sign in screen even if he is authenticated.
## Change Error Log type
You can create your own error log, which will store errors anywhere.
```csharp
    class MyErrorLog: ErrorLog
    //implement ErrorLog
```
 This ErrorLogs available in board:
 - MemoryErrorLog – store errors in memory (by default)
 - XmlFileErrorLog – store errors in XML files
 - SqlErrorLog - store errors in MS SQL (add reference to [ElmahCore.Sql](https://www.nuget.org/packages/ElmahCore.Sql))
 - MysqlErrorLog - store errors in MySQL (add reference to [ElmahCore.MySql](https://www.nuget.org/packages/ElmahCore.MySql))
 - PgsqlErrorLog - store errors in PostgreSQL (add reference to [ElmahCore.Postgresql](https://www.nuget.org/packages/ElmahCore.Postgresql))
```csharp
services.AddElmah<XmlFileErrorLog>(options =>
{
    options.LogPath = "~/log"; // OR options.LogPath = "с:\errors";
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
## Rise exception
```csharp
public IActionResult Test()
{
    HttpContext.RiseError(new InvalidOperationException("Test"));
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
You can create your own notifiers by implement IErrorNotifier interface and add notifier to Elmah options:
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

