using Telegram.Bot;
using TelegramBot;
using TelegramBot.Services;

var builder = WebApplication.CreateBuilder(args);

// Setup Bot configuration
var helperBotConfigurationSection = builder.Configuration.GetSection(BotConfiguration.HelperBotSection);
var classRegistrationBotConfigurationSection = builder.Configuration.GetSection(BotConfiguration.ClassRegistrationBotSection);

builder.Services.Configure<BotConfiguration>(nameof(BotConfiguration.HelperBotSection),helperBotConfigurationSection);
builder.Services.Configure<BotConfiguration>(nameof(BotConfiguration.ClassRegistrationBotSection), classRegistrationBotConfigurationSection);

var helperBotConfiguration = helperBotConfigurationSection.Get<BotConfiguration>();
var classRegistrationBotConfiguration = classRegistrationBotConfigurationSection.Get<BotConfiguration>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Register named HttpClient to get benefits of IHttpClientFactory
// and consume it with ITelegramBotClient typed client.
// More read:
//  https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests#typed-clients
//  https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
builder.Services.AddHttpClient("telegram_bot_client")
    .AddTypedClient<IReadOnlyDictionary<string,ITelegramBotClient>>(httpClient =>
    {
        TelegramBotClientOptions helperBotOptions = new(helperBotConfiguration!.BotToken);
        TelegramBotClientOptions classRegBotOptions = new(classRegistrationBotConfiguration!.BotToken);
        var clients = new Dictionary<string, ITelegramBotClient>
        {
            [nameof(BotConfiguration.HelperBotSection)] = new TelegramBotClient(helperBotOptions, httpClient),
            [nameof(BotConfiguration.ClassRegistrationBotSection)] = new TelegramBotClient(classRegBotOptions, httpClient)
        };
        return clients.AsReadOnly();
    })
    /*.AddTypedClient<ITelegramBotClient>(httpClient =>
    {
        TelegramBotClientOptions options = new(classRegistrationBotConfiguration!.BotToken);
        return new TelegramBotClient(options, httpClient);
    })*/;

// Dummy business-logic service
builder.Services.AddScoped<UpdateHandlers>();

// There are several strategies for completing asynchronous tasks during startup.
// Some of them could be found in this article https://andrewlock.net/running-async-tasks-on-app-startup-in-asp-net-core-part-1/
// We are going to use IHostedService to add and later remove Webhook
builder.Services.AddHostedService<ConfigureWebhook>();

// The Telegram.Bot library heavily depends on Newtonsoft.Json library to deserialize
// incoming webhook updates and send serialized responses back.
// Read more about adding Newtonsoft.Json to ASP.NET Core pipeline:
// ReSharper disable once CommentTypo
//   https://docs.microsoft.com/en-us/aspnet/core/web-api/advanced/formatting?view=aspnetcore-6.0#add-newtonsoftjson-based-json-format-support
builder.Services
    .AddControllers()
    .AddNewtonsoftJson();

var app = builder.Build();
// Construct webhook route from the Route configuration parameter
// It is expected that BotController has single method accepting Update
//app.MapBotWebhookRoute<BotController>(route: helperBotConfiguration!.Route);
//app.MapBotWebhookRoute<BotController>(route: classRegistrationBotConfiguration!.Route);
app.MapControllers();
app.Run();

#pragma warning disable CA1050 // Declare types in namespaces
#pragma warning disable RCS1110 // Declare type inside namespace.
namespace TelegramBot
{
    public class BotConfiguration
#pragma warning restore RCS1110 // Declare type inside namespace.
#pragma warning restore CA1050 // Declare types in namespaces
    {
        public const string HelperBotSection = "BotConfiguration:HelperBotConfiguration";
        public const string ClassRegistrationBotSection = "BotConfiguration:ClassRegistrationBotConfiguration";
        public string BotToken { get; init; } = default!;
        public string HostAddress { get; init; } = default!;
        public string Route { get; init; } = default!;
        public string SecretToken { get; init; } = default!;
    }
}