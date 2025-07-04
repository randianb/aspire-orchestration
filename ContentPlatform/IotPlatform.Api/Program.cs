using System.Globalization;
using Carter;
using IotPlatform.Api;
using IotPlatform.Api.Busi.Channel.EventHandler;
using IotPlatform.Api.Busi.Driver.EventHandler;
using IotPlatform.Api.Busi.Logic;
using IotPlatform.Api.Busi.Logic.Common;
using IotPlatform.Api.Busi.Logic.EdgeDriver;
using IotPlatform.Api.Busi.Logic.Enums;
using IotPlatform.Api.Busi.Sender.Api;
using IotPlatform.Api.Busi.Sender.EventHandler;
using IotPlatform.Api.Busi.Tag.EventHandler;
using IotPlatform.Api.Busi.Tag.Hubs;
using IotPlatform.Api.Database;
using IotPlatform.Api.Entities;
using IotPlatform.Api.Extensions;
using IotPlatform.Api.Repository;
using Contracts;
using FluentValidation;
using Mapster;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using Tipa.Commons;
using Tipa.EFCoreExtend;
using Tipa.ReflectionHelper;
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
TypeAdapterConfig<CreateSender.Command, SenderEntity>.NewConfig()
    .Ignore(dest => dest.Options);
TypeAdapterConfig<ChannelTagDTO, ChannelTagEntity>.NewConfig()
    .Ignore(dest => dest.Value);
builder.Services.AddMediatR(config => config.RegisterServicesFromAssembly(assembly));

builder.Services.AddCarter();
var assemblies = ReflectionHelper.GetAllReferencedAssemblies();

builder.Services.RunModuleInitializers(assemblies);

builder.Services.ConfigureRepository();
builder.Services.AddHttpClient();
// builder.Services.AddHttpClient("Dk", client =>
// {
//     string dk = builder.Configuration.GetSection("Dk:Url").Value;
//     client.BaseAddress = new Uri(dk);
// });

builder.AddRedisDistributedCache("contentplatform-cache");
var entryOptions = new FusionCacheEntryOptions().SetDuration(TimeSpan.FromMinutes(10));
builder.Services.AddFusionCache()
    .WithDefaultEntryOptions(entryOptions)
    .WithPostSetup((sp, c) => { c.DefaultEntryOptions.Duration = TimeSpan.FromMinutes(5); })
    .AsHybridCache().WithRegisteredDistributedCache().WithSerializer(new FusionCacheNewtonsoftJsonSerializer());



builder.Services.AddSignalR();
builder.Services.AddScoped<ValueParserService>();
builder.Services.AddScoped<TagService>();
builder.Services.AddScoped<OpcUaEdgeDriver>();
builder.Services.AddScoped<OpcDaEdgeDriver>();
builder.Services.AddSingleton<ObservableDictionary<string, IEdgeDriver>>();
builder.Services.AddSingleton<IEdgeDriverFactory, EdgeDriverFactory>();

builder.Services.AddTransient<Constants.EdgeDriverResolver>(serviceProvider => type =>
{
    switch (type)
    {
        case DriverTypeEnum.OpcUa:
            return serviceProvider.GetRequiredService<OpcUaEdgeDriver>();
        case DriverTypeEnum.OpcDa:
            return serviceProvider.GetRequiredService<OpcDaEdgeDriver>();
        default:
            throw new ArgumentException("Unknown service name");
    }
});

builder.Services.AddValidatorsFromAssembly(assembly);

builder.Services.AddMassTransit(busConfigurator =>
{
    busConfigurator.SetKebabCaseEndpointNameFormatter();

    busConfigurator.AddConsumer<DriverCreatedConsumer>();
    busConfigurator.AddConsumer<DriverUpdatedConsumer>();
    busConfigurator.AddConsumer<DriverDeletedConsumer>();


    busConfigurator.AddConsumer<TagCreatedConsumer>();
    busConfigurator.AddConsumer<TagUpdatedConsumer>();
    busConfigurator.AddConsumer<TagDeletedConsumer>();
    busConfigurator.AddConsumer<TagChangeConsumer>();


    busConfigurator.AddConsumer<SenderCreatedConsumer>();
    busConfigurator.AddConsumer<SenderUpdatedConsumer>();
    busConfigurator.AddConsumer<SenderDeletedConsumer>();
    busConfigurator.AddConsumer<DkSenderConsumer>();
    busConfigurator.AddConsumer<Influxdb2SenderConsumer>();


    busConfigurator.AddConsumer<ChannelCreatedConsumer>();
    busConfigurator.AddConsumer<ChannelUpdatedConsumer>();
    busConfigurator.AddConsumer<ChannelDeletedConsumer>();
    busConfigurator.AddConsumer<ChannelTagChangedConsumer>();

    busConfigurator.UsingRabbitMq((context, configurator) =>
    {
        configurator.Host(builder.Configuration.GetConnectionString("contentplatform-mq"));

        configurator.ConfigureEndpoints(context);
    });
});

builder.Services.AddHostedService<DriverRunBackgroundService>();
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
app.MapHub<TagNotificationHub>("api/tagHub");
app.Run();