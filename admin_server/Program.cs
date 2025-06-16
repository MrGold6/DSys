using cental_server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSignalR();

builder.Services.AddSingleton(provider =>
{
    var client = new AdminHub("http://172.20.10.2:7100");
    try
    {
        client.StartAsync().Wait();
        Console.WriteLine("✅ SignalR підключено");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Помилка SignalR: " + ex.Message);
    }
    return client;
});

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", config =>
    {
        config.LoginPath = "/Account/Login";
        config.LogoutPath = "/Account/Logout";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization(); 

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Network}/{action=Index}/{id?}");

//app.UseEndpoints(endpoints => { _ = endpoints.MapHub<ConnectionHub>("/notificationHub"); });



app.Run();
