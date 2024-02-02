using AspNet.Security.OAuth.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using SkiServiceMongoDB; 
using SkiServiceMongoDB.Authentication;
using SkiServiceMongoDB.Data;
using static SkiServiceMongoDB.Authentication.BasicAuthenticationHandler;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public static Dictionary<string, string> ValidUsers = new Dictionary<string, string>
    {
        {"Admin", "1234"}
    };

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<MongoDbContext>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("MongoDBConnection");
            var databaseName = "SkiServiceDB";

            return new MongoDbContext(connectionString, databaseName);
        });

        services.AddAuthentication("Bearer")
        .AddJwtBearer("Bearer", options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "Anthony", // Geben Sie den Aussteller (Issuer) an
                ValidAudience = "your_api_identifier", // Geben Sie die Audience (Resource) Ihrer API an
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("1234")) // Geben Sie den Secret Key an
            };
        });
        services.AddControllers();
        services.AddScoped<TokenValidationService>();


        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter Bearer token",
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
            // Benutzer für Swagger
            c.AddSecurityDefinition("basic", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "basic",
                Description = "Input your username and password to access this API"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "basic",
            },
            new string[] { }
        }
    });
        });
    }


    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SkiServiceAPI v1"));
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }


}
