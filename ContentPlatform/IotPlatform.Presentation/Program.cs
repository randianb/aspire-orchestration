using IotPlatform.Presentation;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddOptions();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// --- 使用配置值设置 HttpClient 的 BaseAddress ---

builder.Services.AddHttpClient("IotPlatform.Api", client =>
{
    client.BaseAddress =  new Uri(builder.Configuration["Api:ContentPlatformBaseUrl"]); // 使用从配置中获取并验证过的 Uri
});
await builder.Build().RunAsync();
