# Using ElmahCore
ELMAH for ASP.NET Core

Add nuget package **elmahcore**

## Simple usage
 Startup.cs
```sh
1)	services.AddElmah() in ConfigureServices 
2)	app.UseElmah(); in Configure
```

## Change URL path
```sh
services.AddElmah(options => option.Path = "you_path_here")
```
## Change Error Log type
You can create your own error log, which will store errors anywhere.
```sh
    class MyErrorLog: ErrorLog
    //implement ErrorLog
```
 This ErrorLogs available in board:
 - MemoryErrorLog – store errors in memory (by default)
 - XmlFileErrorLog – store errors in XML files
 - SqlErrorLog - store errors in MS SQL (add reference to [ElmahCore.Sql](https://www.nuget.org/packages/ElmahCore.Sql))
```sh
services.AddElmah<XmlFileErrorLog>(options =>
{
    options.LogPath = "~/log"; // OR options.LogPath = "с:\errors";
});
```
```sh
services.AddElmah<SqlErrorLog>(options =>
{
    options.ConnectionString = "connection_string"; // DB structure see here: https://bitbucket.org/project-elmah/main/downloads/ELMAH-1.2-db-SQLServer.sql
});
```
## Rise exception
```sh
public IActionResult Test()
{
    HttpContext.RiseError(new InvalidOperationException("Test"));
    ...
}
```
## Using Notifiers
You can create your own notifiers by implement IErrorNotifier interface and add notifier to Elmah options:
```sh
services.AddElmah<XmlFileErrorLog>(options =>
{
    options.Path = @"errors";
    options.LogPath = "~/logs";
    options.Notifiers.Add(new ErrorMailNotifier("Email",emailOptions));
});
```
Each notifier must have unique name.
## Using Filers
You can use Elmah XML filter configuration in separate file, create and add custom filters:
```sh
services.AddElmah<XmlFileErrorLog>(options =>
{
    options.FiltersConfig = "elmah.xml";
    options.Filters.Add(new MyFilter());
})
```
Custom filter must implement IErrorFilter.
XML filter config example:
```sh
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

Add notifiers to errorFilte node if you do not want to send notifications
Filtered errors will be logged, but will not be sent.

