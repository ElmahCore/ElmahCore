using Elmah;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();

        // Default implementation - will keep an in-memory error log
        builder.Host.UseElmah();

        var app = builder.Build();

        app.UseExceptionHandler("/Error");
        app.UseElmahMiddleware();

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        
        app.UseRouting();

        app.MapRazorPages();
        app.MapElmah("/elmah.axd");

        app.Run();
    }
}