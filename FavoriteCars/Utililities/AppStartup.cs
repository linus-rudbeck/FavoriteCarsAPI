using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace FavoriteCars.Utililities
{    
    public class AppStartup
    {
        private const string DB_CONTEXT_NAME = "DatabaseConnection";

        private WebApplication app;

        public AppStartup(string[] args) {
            Build(args);
        }

        private void Build(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            SetupSwagger(builder);

            string connectionString = builder.Configuration.GetConnectionString(DB_CONTEXT_NAME);
            builder.Services.AddDbContext<DatabaseContext>( o => o.UseSqlite(connectionString) );

            builder.Services.AddControllers();

            SetupJwtAuthentication(builder);

            builder.Services.AddSignalR();

            app = builder.Build();
        }

        private void SetupSwagger(WebApplicationBuilder builder)
        {
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(setup =>
            {
                // Include 'SecurityScheme' to use JWT Authentication
                var jwtSecurityScheme = new OpenApiSecurityScheme
                {
                    BearerFormat = "JWT",
                    Name = "JWT Authentication",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",

                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };

                setup.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

                setup.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { jwtSecurityScheme, Array.Empty<string>() }
                });

            });
        }

        private void SetupJwtAuthentication(WebApplicationBuilder builder)
        {
            // Retrieve the "JwtSettings" section from the application's configuration (e.g., appsettings.json).
            var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

            if (jwtSettings == null)
            {
                throw new Exception("JWT settings missing in appsettings.json");
            }

            // Register the retrieved "JwtSettings" section as a singleton service, 
            // ensuring only one instance of it is created and reused throughout the application.
            builder.Services.AddSingleton(jwtSettings);

            // Convert the "Key" value from the "JwtSettings" section into a byte array.
            // ASCII encoding is used to convert the string to bytes.
            var key = Encoding.ASCII.GetBytes(jwtSettings.Key);

            // Configure authentication services for the application.
            builder.Services.AddAuthentication(o =>
            {
                // Set the default scheme to authenticate users using JWT (JSON Web Tokens).
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

                // Set the default scheme that challenges unauthorized users using JWT.
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                // Set the parameters that are used to validate incoming JWTs.
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true, // Validate the signing key of the JWT to ensure it's coming from a trusted issuer.
                    IssuerSigningKey = new SymmetricSecurityKey(key), // Specify the key that's used to sign the JWT.
                    ValidateIssuer = true, // Validate the issuer of the JWT.
                    ValidateAudience = true, // Validate the audience of the JWT.
                    ValidateLifetime = true, // Validate the lifetime of the JWT to ensure it hasn't expired.

                    // Specify the valid issuer for the JWT. 
                    ValidIssuer = jwtSettings.Issuer, // Any JWT not from this issuer should be rejected.

                    // Specify the valid audience for the JWT. 
                    ValidAudience = jwtSettings.Audience, // Any JWT not intended for this audience should be rejected.        
                };
            });
        }

        public void RunApp()
        {
            app.UseCors(o =>
            {
                o.WithOrigins("http://localhost:3000")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.MapHub<RealTimeHub>("/realtimehub");

            app.Run();
        }
    }
}
