using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(opts =>
    {
        opts.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        opts.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        opts.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddOpenIdConnect(opts =>
    {
        opts.ResponseType = builder.Configuration["Authentication:Cognito:ResponseType"];
        opts.MetadataAddress = builder.Configuration["Authentication:Cognito:MetadataAddress"];
        opts.ClientId = builder.Configuration["Authentication:Cognito:ClientId"];
        opts.Events = new OpenIdConnectEvents
        {
            OnRedirectToIdentityProviderForSignOut = context =>
            {
                context.ProtocolMessage.Scope = "openid";
                context.ProtocolMessage.ResponseType = "code";

                var cognitoDomain = builder.Configuration["Authentication:Cognito:CognitoDomain"];
                var clientId = builder.Configuration["Authentication:Cognito:ClientId"];
                var logoutUrl =
                    $"{context.Request.Scheme}://{context.Request.Host}{builder.Configuration["Authentication:Cognito:AppSignOutUrl"]}";

                context.ProtocolMessage.IssuerAddress =
                    $"{cognitoDomain}/logout?client_id={clientId}&logout_uri={logoutUrl}&redirect_uri={logoutUrl}";

                // delete cookies
                context.Properties.Items.Remove(CookieAuthenticationDefaults.AuthenticationScheme);

                // close openid session
                context.Properties.Items.Remove(OpenIdConnectDefaults.AuthenticationScheme);

                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "products",
        pattern: "products/{action}/{id?}",
        defaults: new { controller = "Products", action = "Index" });

    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();
