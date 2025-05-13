using Carter;
using ContentPlatform.Api;
using ContentPlatform.Api.Database;
using ContentPlatform.Api.Extensions;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.NewtonsoftJson;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o => o.CustomSchemaIds(id => id.FullName!.Replace('+', '-')));
builder.Services.AddCors();

builder.Services.AddDbContext<ApplicationDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("contentplatform-db")));

var assembly = typeof(Program).Assembly;

builder.Services.AddMediatR(config => config.RegisterServicesFromAssembly(assembly));

builder.Services.AddCarter();

builder.Services.AddValidatorsFromAssembly(assembly);

builder.Services.AddMassTransit(busConfigurator =>
{
    busConfigurator.SetKebabCaseEndpointNameFormatter();
    
    busConfigurator.UsingRabbitMq((context, configurator) =>
    {
        configurator.Host(builder.Configuration.GetConnectionString("contentplatform-mq"));

        configurator.ConfigureEndpoints(context);
    });
});

builder.AddRedisDistributedCache("contentplatform-cache");
builder.Services.AddFusionCache().AsHybridCache().
    WithRegisteredDistributedCache() .WithSerializer(new FusionCacheNewtonsoftJsonSerializer());
var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

    app.ApplyMigrations();
}

app.MapCarter();

app.UseHttpsRedirection();

app.Run();
