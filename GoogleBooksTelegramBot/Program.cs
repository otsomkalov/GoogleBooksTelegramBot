using System;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Books.v1;
using Google.Apis.Books.v1.Data;
using Google.Apis.Services;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace GoogleBooksTelegramBot
{
    internal static class Program
    {
        private static TelegramBotClient _bot;
        private static readonly BooksService BooksService = new BooksService(new BaseClientService.Initializer());

        private static async Task Main(string[] args)
        {
            if (args.Length == 0) throw new ArgumentNullException("Token");

            _bot = new TelegramBotClient(args[0]);

            _bot.OnInlineQuery += OnInlineQueryAsync;
            _bot.OnMessage += OnOnMessageAsync;

            _bot.StartReceiving();

            Console.WriteLine("Bot started");

            while (true) await Task.Delay(int.MaxValue);
        }

        private static async void OnOnMessageAsync(object sender, MessageEventArgs messageEventArgs)
        {
            try
            {
                var message = messageEventArgs.Message;

                if (message.Text.StartsWith("/start"))
                    await _bot.SendTextMessageAsync(new ChatId(message.From.Id),
                        "This bot can help you find and share books. It is works in any chat, just write @GBooksBot " +
                        "in the text field. Let's try!",
                        replyMarkup: new InlineKeyboardMarkup(new[]
                        {
                            InlineKeyboardButton.WithSwitchInlineQueryCurrentChat("🔍 Search books"),
                            InlineKeyboardButton.WithSwitchInlineQuery("🔗 Find and share book with friends")
                        }));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static async void OnInlineQueryAsync(object sender, InlineQueryEventArgs inlineQueryEventArgs)
        {
            var inlineQuery = inlineQueryEventArgs.InlineQuery;

            if (string.IsNullOrEmpty(inlineQuery.Query) || string.IsNullOrWhiteSpace(inlineQuery.Query)) return;

            var listBooksRequest = BooksService.Volumes.List(inlineQuery.Query);
            var books = await listBooksRequest.ExecuteAsync();

            if (books?.Items == null || !books.Items.Any()) return;

            var response = books.Items.Select(volume =>
            {
                var article =
                    new InlineQueryResultArticle(volume.Id.ToString(),
                        volume.VolumeInfo.Title,
                        new InputTextMessageContent(GetMarkdown(volume)) {ParseMode = ParseMode.Html});

                if (volume.VolumeInfo.Authors != null)
                    article.Description = string.Join(", ", volume.VolumeInfo.Authors);

                if (volume.VolumeInfo.ImageLinks != null) article.ThumbUrl = volume.VolumeInfo.ImageLinks.Thumbnail;

                return article;
            });

            await _bot.AnswerInlineQueryAsync(inlineQuery.Id, response);
        }

        private static string GetMarkdown(Volume volume)
        {
            var resultString =
                $"<a href=\"https://books.google.com.ua/books?id={volume.Id}\">{volume.VolumeInfo.Title}</a>\n";

            if (volume.VolumeInfo.Authors != null)
                resultString += $"Author(s): {string.Join(", ", volume.VolumeInfo.Authors)}\n";

            return resultString + $"Rating: {new string('⭐', Convert.ToInt32(volume.VolumeInfo.AverageRating))}\n";
        }
    }
}