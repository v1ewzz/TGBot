using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TGBot;

class Program
{
    private static ITelegramBotClient botClient;
    private static String TELEGRAM_BOT_API_KEY = Environment.GetEnvironmentVariable("TELEGRAM_BOT_API_KEY");
    private static String PERPLEXITY_API_KEY = Environment.GetEnvironmentVariable("PERPLEXITY_API_KEY");
    private static ReceiverOptions receiverOptions;
    private static readonly HttpClient httpClient = new HttpClient();
    private static PerplexityService perplexityService = new PerplexityService(PERPLEXITY_API_KEY, httpClient);
    private static Boolean isPerplexityModeActivated = false;
    private static Boolean isMessageToCreatorModeActivated = false;
    private static String startMessage = File.ReadAllText("D:\\TGBot\\StartMessage.txt");
    private static String aboutBotMessage = File.ReadAllText("D:\\TGBot\\AboutBotMessage.txt");
    private static ReplyKeyboardMarkup mainMenuKeyboard = BotKeyboard.GetMainMenu();
    private static ReplyKeyboardMarkup backToMainMenuButton = BotKeyboard.BackToMainMenu();
    private static ReplyKeyboardMarkup messageToCreatorMenu = BotKeyboard.MessageToCreatorMenu();
    private static readonly UserRequestsService _requestsService = new UserRequestsService();
    private static readonly NewsService _newsService = new NewsService();


    static async Task Main()
    {
        botClient = new TelegramBotClient(TELEGRAM_BOT_API_KEY);
        receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new UpdateType[] { UpdateType.Message },
            DropPendingUpdates = true
        };
        using var cts = new CancellationTokenSource();
        botClient.StartReceiving(UpdateHandler, ErrorHandler, receiverOptions, cts.Token);
        var me = await botClient.GetMe();
        Console.WriteLine($"Бот {me.FirstName} (@{me.Username}) запущен");
        Console.ReadLine();
        cts.Cancel();
    }

    private static async Task UpdateHandler(ITelegramBotClient bot, Update update, CancellationToken token)
    {

        String? messageText = null;
        var userId = update.Message.From.Id;
        long chatId = 0;
        if (update.Type == UpdateType.Message)
        {
            var message = update.Message;
            messageText = message?.Text;
            chatId = message.Chat.Id;
        }
        else return;
        Console.WriteLine(update.Message.Chat.Username);
        if (messageText.StartsWith("/") || messageText == "Perplexity-mode")
        {
            switch (messageText)
            {
                case "/perplexity":
                case "Perplexity-mode":
                    {                    
                        if (!await _requestsService.CanMakeRequestAsync(userId))
                        {
                            await botClient.SendMessage(chatId, "У вас закончились запросы! " +
                                "Чтобы продолжить пользоваться этим режимом - пополните баланс!");
                            await bot.SendMessage(chatId, "Выберите пункт меню:", replyMarkup: mainMenuKeyboard);
                            break;
                            
                        }
                        isPerplexityModeActivated = true;
                        await botClient.SendMessage(chatId, "Включен режим ИИ-помощника!", replyMarkup: backToMainMenuButton);
                        break;
                    }
                case "/start":
                    await botClient.SendMessage(chatId, startMessage);
                    await botClient.SendMessage(chatId, "Выберите пункт меню:", replyMarkup: mainMenuKeyboard);
                    break;
                default:
                    await botClient.SendMessage(chatId, "Введена неправильная команда!");
                    break;
            }
        }
        else
        {
            if (isMessageToCreatorModeActivated)
            {
                await bot.SendMessage(chatId, "Спасибо за ваш отзыв!", replyMarkup: mainMenuKeyboard);
                await bot.SendMessage(0, "Вам пришло сообщение от @" + update.Message.Chat.Username + ":\n\n" + messageText);
                isMessageToCreatorModeActivated = false;
            }
            switch (messageText)
            {
                case "⬅️Вернуться в меню":
                    isPerplexityModeActivated = false;
                    await bot.SendMessage(chatId, "Выберите пункт меню:", replyMarkup: mainMenuKeyboard);
                    break;
                case "О боте":
                    await bot.SendMessage(chatId, aboutBotMessage, replyMarkup: messageToCreatorMenu);
                    break;
                case "Оставить сообщение создателю":
                    isMessageToCreatorModeActivated = true;
                    await bot.SendMessage(chatId, "Напишите свое сообщение ниже:");
                    break;
                case "Новости":
                    await _newsService.ShowNews(botClient, chatId, token);
                    break;
            }
            if (await _requestsService.CanMakeRequestAsync(userId))
            {
                if (isPerplexityModeActivated)
                {
                    await bot.SendChatAction(chatId, Telegram.Bot.Types.Enums.ChatAction.Typing, cancellationToken: token);
                    string responseText = await perplexityService.QueryPerplexityChat(messageText);
                    responseText = FilterBotAnswer.ConvertText(responseText);
                    await perplexityService.SendLongMessage(bot, chatId, responseText, token);
                    await _requestsService.IncrementRequestAsync(userId);
                }
            }
            else if (isPerplexityModeActivated && !await _requestsService.CanMakeRequestAsync(userId))
            {
                await botClient.SendMessage(chatId, "У вас закончились запросы! " +
                "Чтобы продолжить пользоваться этим режимом - пополните баланс!");           
            }

            //if (messageText == "⬅️Обратно в меню")
            //{
            //    isPerplexityModeActivated = false;
            //    await bot.SendMessage(chatId, "Выберите пункт меню:", replyMarkup: mainMenuKeyboard);

            //}
            //if (isPerplexityModeActivated)
            //{
            //    string responseText = await perplexityService.QueryPerplexityChat(messageText);
            //    responseText = FilterBotAnswer.ConvertText(responseText);
            //    await perplexityService.SendLongMessage(bot, chatId, responseText, token);
            //}
            //else
            //{
            //    await bot.SendMessage(chatId, "Выберите пункт меню:", replyMarkup: mainMenuKeyboard);
            //}
        }
    }

    private static Task ErrorHandler(ITelegramBotClient bot, Exception error, CancellationToken token)
    {
        var errorMsg = error is Telegram.Bot.Exceptions.ApiRequestException apiEx
            ? $"Telegram API Error: [{apiEx.ErrorCode}] {apiEx.Message}"
            : error.ToString();
        Console.WriteLine(errorMsg);
        return Task.CompletedTask;
    }

}
