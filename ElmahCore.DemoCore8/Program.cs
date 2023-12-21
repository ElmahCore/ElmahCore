using ElmahCore;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();

        // Default implementation - will keep an in-memory error log
        builder.Services.AddElmah();

        var app = builder.Build();

        app.UseExceptionHandler("/Error");
        app.UseElmah();

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        
        app.UseRouting();

        app.MapRazorPages();
        app.MapElmah("/elmah.axd");

        app.Run();
    }
}