using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace TelegramBot.Filters;

/// <summary>
/// Check for "X-Telegram-Bot-Api-Secret-Token"
/// Read more: <see href="https://core.telegram.org/bots/api#setwebhook"/> "secret_token"
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class ValidateTelegramBotAttribute() : TypeFilterAttribute(typeof(ValidateTelegramBotFilter))
{
    private class ValidateTelegramBotFilter(IOptionsSnapshot<BotConfiguration> options) : IActionFilter
    {
        private readonly string _helperBotSecretToken =
            options.Get(nameof(BotConfiguration.HelperBotSection)).SecretToken;

        private readonly string _classRegBotSecretToken =
            options.Get(nameof(BotConfiguration.ClassRegistrationBotSection)).SecretToken;

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (IsValidRequest(context.HttpContext.Request)) return;
            context.Result = new ObjectResult("\"X-Telegram-Bot-Api-Secret-Token\" is invalid")
            {
                StatusCode = 403
            };
        }

        private bool IsValidRequest(HttpRequest request)
        {
            var isSecretTokenProvided =
                request.Headers.TryGetValue("X-Telegram-Bot-Api-Secret-Token", out var secretTokenHeader);
            
            var isSecretTokensEqual = string.Equals(secretTokenHeader, _helperBotSecretToken, StringComparison.Ordinal)
                                      || string.Equals(secretTokenHeader, _classRegBotSecretToken, StringComparison.Ordinal);
            
            return isSecretTokenProvided && isSecretTokensEqual;
        }
    }
}