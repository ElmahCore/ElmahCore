using ElmahCore;
using ElmahCore.DemoCore6;
using ElmahCore.Mvc;
using ElmahCore.Mvc.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddElmah(elmah =>
{
    elmah.Configure(options =>
    {
        options.Notifiers.Add(new MyNotifier());
        options.Filters.Add(new CmsErrorLogFilter());
    });

    elmah.PersistToFile("~/log");
});

builder.Services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
{
    builder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
}));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseElmahExceptionPage();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseCors("MyPolicy");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseElmah();

app.MapRazorPages();
app.MapElmah();

app.Run();
