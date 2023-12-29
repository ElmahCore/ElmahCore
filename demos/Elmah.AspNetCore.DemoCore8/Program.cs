using Elmah;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();

        builder.Host.UseElmah(elmah => elmah.UseElmahExceptionPage());

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