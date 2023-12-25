using ElmahCore;
using ElmahCore.DemoCore6;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// Register and configure the services used by Elmah.
builder.Host.UseElmah((_, elmah) =>
{
    elmah.Configure(options =>
    {
        options.Notifiers.Add(new MyNotifier());
        options.Filters.Add(new CmsErrorLogFilter());
    });

    elmah.PersistToFile("~/log");
    elmah.UseElmahExceptionPage();
    elmah.SetLogLevel(LogLevel.Information);
});

builder.Services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
{
    builder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
}));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseCors("MyPolicy");

app.UseHttpsRedirection();
app.UseStaticFiles();

// Adds elmah middleware - the placement of this call is important.
// All middleware after this has context captured and available to
// elmah.
app.UseElmahMiddleware();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

// Map the endpoints exposed by Elmah. You can use the returned builder
// to add metadata to configure authn, authz, etc.
app.MapElmah();

app.Run();
