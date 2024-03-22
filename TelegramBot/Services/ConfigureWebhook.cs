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

    private static readonly string[] Bots = [nameof(BotConfiguration.HelperBotSection), nameof(BotConfiguration.ClassRegistrationBotSection)];
    private readonly List<BotConfiguration> _botConfigs =
    [
        botOptions.Get(Bots[0]),
        botOptions.Get(Bots[1])
    ];

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var botClients = scope.ServiceProvider.GetRequiredService<IReadOnlyDictionary<string, ITelegramBotClient>>();

        // Configure custom endpoint per Telegram API recommendations:
        // https://core.telegram.org/bots/api#setwebhook
        // If you'd like to make sure that the webhook was set by you, you can specify secret data
        // in the parameter secret_token. If specified, the request will contain a header
        // "X-Telegram-Bot-Api-Secret-Token" with the secret token as content.
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
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        // Remove webhook on app shutdown
        _logger.LogInformation("Removing webhook");
        await botClient.DeleteWebhookAsync(cancellationToken: cancellationToken);
    }
}