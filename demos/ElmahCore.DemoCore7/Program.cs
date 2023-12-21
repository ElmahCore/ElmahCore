using ElmahCore;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();
        builder.Services.AddElmah(elmah =>
        {
            elmah.UseElmahExceptionPage();
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }
        app.UseStaticFiles();

        app.UseElmah();
        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();
        app.MapElmah();

        app.Run();
    }
}