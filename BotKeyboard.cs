using System;
using Telegram.Bot.Types.ReplyMarkups;

namespace TGBot
{
    internal static class BotKeyboard
    {
        public static ReplyKeyboardMarkup GetMainMenu()
        {
            return new ReplyKeyboardMarkup(
            [
                ["Perplexity-mode", "Новости"],
                ["О боте"]
            ])
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = false
            };
        }

        public static ReplyKeyboardMarkup BackToMainMenu()
        {
            return new ReplyKeyboardMarkup(
            [
                ["⬅️Вернуться в меню"]
            ])
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };
        }

        public static ReplyKeyboardMarkup MessageToCreatorMenu()
        {
            return new ReplyKeyboardMarkup(
            [
                ["Оставить сообщение создателю"],
                ["⬅️Вернуться в меню"]
            ])
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };
        }
    }
}
