using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

// ReSharper disable ReplaceWithPrimaryConstructorParameter

namespace TelegramBot.Services;

public class ConfigureWebhook(
    ILogger<ConfigureWebhook> logger,
    IServiceProvider serviceProvider,
    IOptionsMonitor<BotConfiguration> botOptions) : IHostedService
{
    private readonly ILogger<ConfigureWebhook> _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    private static readonly string[] Bots =
        [BotConfiguration.HelperBotSection, BotConfiguration.ClassRegistrationBotSection];

    private readonly IReadOnlyList<BotConfiguration> _botConfigs =
    [
        botOptions.Get(Bots[0]),
        botOptions.Get(Bots[1])
    ];

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var botClients = scope.ServiceProvider.GetRequiredService<IReadOnlyDictionary<string, ITelegramBotClient>>();

        for (var i = 0; i < botClients.Count; ++i)
        {
            var webhookAddress = $"{_botConfigs[i].HostAddress}/{_botConfigs[i].Route}";
            _logger.LogInformation("Setting webhook: {WebhookAddress}", webhookAddress);
            await botClients[Bots[i]].SetWebhookAsync(
                url: webhookAddress,
                allowedUpdates: Array.Empty<UpdateType>(),
                secretToken: _botConfigs[i].SecretToken,
                cancellationToken: cancellationToken);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var botClients = scope.ServiceProvider.GetRequiredService<IReadOnlyDictionary<string, ITelegramBotClient>>();

        // Remove webhooks on app shutdown
        for (var i = 0; i < botClients.Count; ++i)
        {
            _logger.LogInformation("Removing bot: {BotName} webhook",
                await botClients[Bots[i]].GetMeAsync(cancellationToken));
            await botClients[Bots[i]].DeleteWebhookAsync(cancellationToken: cancellationToken);
        }
    }
}