using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyMovieScore.Api.Filters;
using MyMovieScore.Application.Commands.CreateMovie;
using MyMovieScore.Application.Validators;
using MyMovieScore.Core.Repositories;
using MyMovieScore.Core.Services;
using MyMovieScore.Infrastructure.Auth;
using MyMovieScore.Infrastructure.ExternalServices;
using MyMovieScore.Infrastructure.Persistence;
using MyMovieScore.Infrastructure.Persistence.Repositories;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("MyMovieScoreCs");
builder.Services.AddDbContext<MyMovieScoreDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddControllers(options => options.Filters.Add(typeof(ValidationFilter)))
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<CreateUserCommandValidator>());

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<ExternalServiceOptions>(builder.Configuration.GetSection(ExternalServiceOptions.SectionName));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddHttpClient<IIMDbExternalService, IMDbExternalService>((serviceProvider, client) =>
{
  var options = serviceProvider.GetRequiredService<IOptions<ExternalServiceOptions>>().Value;
  client.BaseAddress = new Uri(options.BaseUrl);
});
builder.Services.AddMediatR(typeof(CreateMovieCommand));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new OpenApiInfo { Title = "MyMovieScore.API", Version = "v1" });

  c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
  {
    Name = "Authorization",
    Type = SecuritySchemeType.ApiKey,
    Scheme = "Bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "JWT Authorization header usando o esquema Bearer."
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
                             new string[] {}
                     }
                 });
});
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
builder.Services
  .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(options =>
  {
    options.TokenValidationParameters = new TokenValidationParameters
    {
      ValidateIssuer = true,
      ValidateAudience = true,
      ValidateLifetime = true,
      ValidateIssuerSigningKey = true,

      ValidIssuer = jwtOptions.Issuer,
      ValidAudience = jwtOptions.Audience,
      IssuerSigningKey = new SymmetricSecurityKey
                  (Encoding.UTF8.GetBytes(jwtOptions.Key))
    };
  });
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
  app.UseSwagger();
  app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
  using var scope = app.Services.CreateScope();
  var services = scope.ServiceProvider;

  var context = services.GetRequiredService<MyMovieScoreDbContext>();
  if (context.Database.GetPendingMigrations().Any())
  {
    context.Database.Migrate();
  }
}

app.Run();

// Lets WebApplicationFactory<Program> in MyMovieScore.ApiTests start the API
public partial class Program { }
