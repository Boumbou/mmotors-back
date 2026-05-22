using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using mmotors_back.Data;
using mmotors_back.Models;
using mmotors_back.Mappers;
using mmotors_back.Features.Accounts.Interfaces;
using mmotors_back.Features.Accounts.Services;
using mmotors_back.Features.Vehicles.Interfaces;
using mmotors_back.Features.Vehicles.Repositories;
using mmotors_back.Features.Applications.Interfaces;
using mmotors_back.Features.Applications.Repositories;
using mmotors_back.Features.Applications.Services;
using mmotors_back.Features.Services.Interfaces;
using mmotors_back.Features.Services.Repositories;
using mmotors_back.Features.Shared.Interfaces;
using mmotors_back.Features.Shared.Services;
using mmotors_back.Features.Documents.Interfaces;
using mmotors_back.Features.Documents.Repositories;
using mmotors_back.Features.DocumentTemplates.Interfaces;
using mmotors_back.Features.DocumentTemplates.Repositories;
using Amazon.S3;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;


//create builder instance
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
#region Services
    //allow CORS for development
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins("http://localhost:5173") // Adjust the origin as needed
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    builder.Services.AddControllers();
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    // builder.Services.AddOpenApi();


    //add health checks
    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!, name: "PostgreSQL");

    // add identity
    builder.Services.AddIdentity<User, IdentityRole>( options =>
    {
        // Configure identity options here if needed
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
    })
    .AddEntityFrameworkStores<AppDbContext>();

    //add db context
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
    );

    //add token authentication
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
        options.DefaultChallengeScheme =
        options.DefaultForbidScheme =
        options.DefaultScheme =
        options.DefaultSignInScheme =
        options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidAudience = builder.Configuration["JWT:Audience"],
                ValidIssuer = builder.Configuration["JWT:Issuer"],
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"]!)
                ),
            };
        }
    );

    /*
        * Add application services
        * one for role Customer only
        * one for role Staff and Admin only
        * one for role Admin only
    */
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("RequireAuthenticatedUser", policy => policy.RequireAuthenticatedUser());
        options.AddPolicy("RequireCustomerRole", policy => policy.RequireRole("Customer"));
        options.AddPolicy("RequireStaffOrAdminRole", policy => policy.RequireRole("Staff", "Admin"));
        options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    });


    //add swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Version = "v1",
            Title = "MMotors API",
            Description = "An ASP.NET Core Web API for managing MMotors.",
        });
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT Authorization header using the Bearer scheme."
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                Array.Empty<string>()
            }
        });

    });


    //add mappers
    builder.Services.AddScoped<UserMapper>();

    //add token service
    builder.Services.AddScoped<ITokenService, TokenService>();

    //add repositories
    builder.Services.AddScoped<IVehiclesRepository, VehiclesRepository>();
    builder.Services.AddScoped<IApplicationsRepository, ApplicationRepository>();
    builder.Services.AddScoped<IServicesRepository, ServicesRepository>();
    builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
    builder.Services.AddScoped<IDocumentTemplateRepository, DocumentTemplateRepository>();

    //add services
    builder.Services.AddScoped<CheckAuthorization>();

    //add PaginationService
    builder.Services.AddScoped<IPaginationService, PaginationService>();

    //add DataSeeder
    builder.Services.AddScoped<DataSeeder>();

    //add storage service
    var storageType = builder.Configuration["Storage:Type"];
    if (storageType == "S3")
    {
        builder.Services.AddAWSService<IAmazonS3>();
        builder.Services.AddScoped<IStorageService, S3FileStorageService>();
    }
    else
    {
        builder.Services.AddScoped<IStorageService, LocalStorageService>(provider =>
        {
            var storagePath = builder.Configuration["Storage:Local:StoragePath"]??"wwwroot/uploads";
            return new LocalStorageService(storagePath);
        });
    }
#endregion

#region Build App and Configure Middleware
    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // app.UseHttpsRedirection();
    app.UseRouting();
    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseStaticFiles();

    app.MapControllers();

    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(entry => new
                {
                    name = entry.Key,
                    status = entry.Value.Status.ToString(),
                    exception = entry.Value.Exception?.Message,
                    duration = entry.Value.Duration.ToString()
                })
            };
            await context.Response.WriteAsJsonAsync(result);
        }
    });


    //run migrations if not run yet
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate();
    }


    //seed admin user
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        var dataSeeder = new DataSeeder(userManager, roleManager);
        await dataSeeder.SeedData(new User
        {
            Name = "Admin",
            LastName = "User",
            UserName = "admin@example.com",
            Email = "admin@example.com"
        }, "adminPassword1");

        await dataSeeder.SeedVehicles(services.GetRequiredService<AppDbContext>());
        await dataSeeder.SeedDocumentTemplates(services.GetRequiredService<AppDbContext>());
    }
#endregion

app.Run();
