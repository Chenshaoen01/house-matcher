using HouseMatcher.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SignalRChat.Hubs;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var configValue = builder.Configuration;

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

builder.Services.AddSignalR();
builder.Services.AddSingleton<ImgurService>();

builder.Services.AddDbContext<HouseMatcherContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

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

app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<ChatHub>("/chatHub");

app.Use(async (context, next) =>
{
    // �ˬd�ШD�O�_�� Controller Action
    var endpoint = context.GetEndpoint();
    if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
    {
        // �p�G Action �[�� AllowAnonymous �ݩʡA�h��������U�@�Ӥ����n��
        await next();
        return;
    }

    // �ˬd�ϥΪ̬O�_�w�q�L��������
    if (!context.User.Identity.IsAuthenticated)
    {
        // �ϥΪ̥��q�L�������ҡA�N�L�̾ɦV�n�J����
        context.Response.Redirect("/Home/Redirect");
        return;
    }

    // �ϥΪ̤w�q�L�������ҡA�~�����ШD
    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
