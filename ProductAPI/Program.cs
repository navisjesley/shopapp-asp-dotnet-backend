using ProductAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;

// let's test the build and push image to acr github pipeline 2
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Product Service API", Version = "v1" });
});

// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("ReactLocal", policy =>
//     {
//         policy
//             .WithOrigins("http://localhost:3000")
//             .AllowAnyHeader()
//             .AllowAnyMethod();
//     });
// });

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ProductApiCaller", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole(builder.Configuration["Authorization:ProductApiAccessRole"] ?? "ProductApi.Access");
    });
});

builder.Services.AddScoped<IProductCatalogRepository, ProductCatalogRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();
// app.UseCors("ReactLocal");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();