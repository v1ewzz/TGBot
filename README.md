# 🤖 TGBot — Telegram-бот с ИИ-ассистентом

Telegram-бот, который совмещает функции **ИИ-помощника на базе Groq**, **ленты новостей с Habr**, **обратной связи с пользователями** и **системы лимитов на бесплатные запросы**.

Написан на **C# / .NET 8** по принципам **SOLID и DRY** в слоистой архитектуре: абстракции, сервисы, данные, Telegram-слой и команды.

---

## ✨ Возможности

- **🧠 ИИ-помощник** — диалог с нейросетью через [Groq](https://console.groq.com) (модель `openai/gpt-oss-120b`). Ответы приходят в Telegram с поддержкой MarkdownV2.
- **📰 Новости** — последние 5 публикаций с Хабра (RSS) прямо в боте.
- **✉️ Обратная связь** — пользователь оставляет сообщение, которое пересылается владельцу бота.
- **🔢 Лимит запросов** — до **3 запросов к ИИ** на пользователя (хранится в SQL Server, учитывается атомарно).
- **👤 Режимы per-user** — состояние ИИ/обратной связи изолировано для каждого пользователя, без глобальных флагов.

---

## 🧱 Стек технологий

| Технология | Назначение |
|---|---|
| **.NET 8** (C# 12) | Язык и платформа |
| **Telegram.Bot** 22.x | Работа с Bot API (polling) |
| **Groq API** | Бесплатное инференс-ядро ИИ (OpenAI-совместимый формат) |
| **Entity Framework Core** 8 | Доступ к БД |
| **SQL Server / SQL Express** | Хранение счётчиков запросов |
| **System.ServiceModel.Syndication** | Парсинг RSS-новостей (Habr) |

---

## 📁 Структура проекта

```
TGBot/
├── Abstractions/          # Интерфейсы (IChatCompletionService, IRequestLimiter, INewsService)
├── Bot/                   # Telegram-слой
│   ├── Commands/          # Команды по паттерну Command (Start, AiMode, News, ...)
│   ├── UpdateHandler.cs   # Маршрутизация входящих сообщений
│   ├── AiRequestHandler.cs
│   ├── UserSessionStore.cs# Per-user состояние (ИИ / обратная связь)
│   ├── MessageSender.cs   # Отправка длинных сообщений (разбиение по 4000 символов)
│   └── KeyboardProvider.cs# Клавиатуры меню
├── Services/              # Реализации: GroqChatService, UserRequestsService, HabrNewsService
├── Data/                  # Сущность UserRequest + BotDbContext
├── Settings/              # BotSettings (record из env), EnvLoader (.env)
├── Text/                  # BotText (строки), MarkdownFormatter (экранирование)
├── Program.cs             # Композиционный корень
├── .env.example           # Шаблон переменных окружения
├── StartMessage.txt       # Приветствие бота (редактируется)
└── AboutBotMessage.txt    # Описание бота (редактируется)
```

---

## 🚀 Установка

### Требования

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) или новее
- SQL Server (локальный **SQL Express** подойдёт)
- Учётная запись [Groq](https://console.groq.com) и API-ключ
- Telegram-бот, созданный через [@BotFather](https://t.me/BotFather)

### Клонирование

```bash
git clone https://github.com/your-username/TGBot.git
cd TGBot
```

### Восстановление пакетов

```bash
dotnet restore
```

---

## 🔐 Настройка переменных окружения

Скопируй шаблон и заполни значения:

```bash
cp .env.example .env
```

| Переменная | Обязательна | Описание |
|---|---|---|
| `TELEGRAM_BOT_API_KEY` | ✅ | Токен бота от @BotFather |
| `GROQ_API_KEY` | ✅ | API-ключ Groq (`console.groq.com/keys`) |
| `DB_CONNECTION_STRING` | ❌ | Строка подключения к SQL Server. Если не задана — используется локальный `SQLEXPRESS` |
| `CREATOR_CHAT_ID` | ❌ | Telegram `chat_id` владельца (куда пересылаются отзывы). По умолчанию `0` |

> 💡 Файл `.env` **не коммитится** в git — он добавлен в `.gitignore`. Все ключи читаются только из окружения, в коде и репозитории секретов нет.

Пример `.env`:

```dotenv
TELEGRAM_BOT_API_KEY=123456:ABC-DEF...
GROQ_API_KEY=gsk_xxxxxxxxxxxxxxxx
DB_CONNECTION_STRING=Server=localhost;Database=TGbot;User Id=sa;Password=yourpass;TrustServerCertificate=true;
CREATOR_CHAT_ID=123456789
```

> Таблица `user_requests` и уникальный индекс на `user_id` создаются **автоматически** при первом запуске — миграции вручную не нужны.

---

## ▶️ Запуск локально

```bash
dotnet run
```

Ожидаемый вывод:

```
Бот MyBot (@my_bot) запущен
```

Бот работает, пока терминал открыт. Для остановки — `Ctrl+C`.

### Деплой на сервер (Linux / Codespaces)

```bash
# установить .NET runtime
sudo apt install dotnet-sdk-8.0

# собрать
dotnet publish -c Release -o out

# запустить
cd out && ASPNETCORE_ENVIRONMENT=Production dotnet TGBot.dll
```

> Для Linux-сервера используй `User Id`/`Password` в `DB_CONNECTION_STRING` — Windows-аутентификация (`Trusted_Connection=true`) там не работает.

---

## 💬 Пример использования

1. Открой бота в Telegram и нажми **Start** (`/start`).
2. Нажми кнопку **«ИИ-помощник»** (или отправь `/ai`):

   ```
   🤖 Включен режим ИИ-помощника!
   ```

3. Задай вопрос:

   ```
   Объясни разницу между SOLID и DRY простыми словами
   ```

   Бот покажет индикатор «печатает…» и ответит текстом с форматированием (жирный текст из `**...**` корректно рендерится в Telegram).

4. Кнопка **«Новости»** покажет последние 5 статей с Хабра:

   ```
   ✅Источник - Habr

   📰Последние новости:
   [1. Название статьи](https://habr.com/...)  ← кликабельная кнопка
   ```

5. **«Оставить сообщение создателю»** — напиши отзыв, он уйдёт владельцу бота.

6. Вернуться в главное меню — кнопка **«⬅️Вернуться в меню»**.

> ⚠️ У каждого пользователя **3 бесплатных запроса** к ИИ. После исчерпания лимита бот предложит «пополнить баланс».

---

## 🛡️ Безопасность

- Ключи — только в переменных окружения, не в коде.
- SQL-запросы параметризованы (`ExecuteSqlInterpolatedAsync`) — защита от SQL-инъекций.
- RSS-парсер работает с `DtdProcessing.Prohibit` — защита от XXE.
- JSON-ответы ИИ парсятся через потоковый `JsonDocument` — без небезопасной десериализации.

---

## 📄 Лицензия

MIT