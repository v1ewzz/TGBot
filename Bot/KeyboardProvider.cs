using Telegram.Bot.Types.ReplyMarkups;
using TGBot.Text;

namespace TGBot.Bot;

public sealed class KeyboardProvider
{
    public ReplyKeyboardMarkup MainMenu() => new(
        [
            [BotText.AiModeButton, BotText.NewsButton],
            [BotText.AboutButton]
        ])
    {
        ResizeKeyboard = true,
        OneTimeKeyboard = false
    };

    public ReplyKeyboardMarkup BackToMainMenu() => new(
        [
            [BotText.BackToMenuButton]
        ])
    {
        ResizeKeyboard = true,
        OneTimeKeyboard = true
    };

    public ReplyKeyboardMarkup MessageToCreatorMenu() => new(
        [
            [BotText.FeedbackButton],
            [BotText.BackToMenuButton]
        ])
    {
        ResizeKeyboard = true,
        OneTimeKeyboard = true
    };
}