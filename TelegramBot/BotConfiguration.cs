#pragma warning disable CA1050 // Declare types in namespaces
#pragma warning disable RCS1110 // Declare type inside namespace.
namespace TelegramBot;
public class BotConfiguration()
#pragma warning restore RCS1110 // Declare type inside namespace.
#pragma warning restore CA1050 // Declare types in namespaces
{
    public static readonly string HelperBotSection = "BotConfiguration:HelperBotConfiguration";
    public static readonly string ClassRegistrationBotSection = "BotConfiguration:ClassRegistrationBotConfiguration";
    public string BotToken { get; init; } = default!;
    public string HostAddress { get; init; } = default!;
    public string Route { get; init; } = default!;
    public string SecretToken { get; init; } = default!;
}