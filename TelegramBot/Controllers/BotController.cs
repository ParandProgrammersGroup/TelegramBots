using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using TelegramBot.Filters;
using TelegramBot.Services;

namespace TelegramBot.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BotController : ControllerBase
{
    [Route("HelperBot")]
    [HttpPost]
    [ValidateTelegramBot]
    public async Task<IActionResult> HelperBotPost(
        [FromBody] Update update,
        [FromServices] UpdateHandlers handleUpdateService,
        CancellationToken cancellationToken)
    {
        await handleUpdateService.HandleHelperBotUpdateAsync(update, cancellationToken);
        return Ok();
    }

    [Route("ClassRegistrationBot")]
    [HttpPost]
    [ValidateTelegramBot]
    public async Task<IActionResult> ClassRegistrationBotPost(
        [FromBody] Update update,
        [FromServices] UpdateHandlers handleUpdateService,
        CancellationToken cancellationToken)
    {
        await handleUpdateService.HandleClassRegBotUpdateAsync(update, cancellationToken);
        return Ok();
    }
}