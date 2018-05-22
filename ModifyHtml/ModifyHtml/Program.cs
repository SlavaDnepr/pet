using System.Threading.Tasks;
using Com.CloudRail.SI;
using HtmlAgilityPack;
using Telegram.Bot.Types;
using File = System.IO.File;

namespace Cantaloupe.ModifyHtml
{
    class Program
    {
        public static void Main()
        {



            ModifyHtml();
        }

        private static void SendMessage()
        {
            CloudRail.AppKey = "5af98a878de7127c203f08f0";

            var service = new Com.CloudRail.SI.Services.Telegram(null, "526101740:AAGDH_XEI-2H5uRDe2hDyS_Jea9W1fThAJk", "");
            service.SendMessage("395421232", "test");
            // var bot = new Telegram.Bot.TelegramBotClient("526101740:AAGDH_XEI-2H5uRDe2hDyS_Jea9W1fThAJk");
            // return await bot.SendTextMessageAsync("395421232", "test message");
        }

        private static void ModifyHtml()
        {
            var doc1 = new HtmlDocument();
            var text1 = File.ReadAllText(@"C:\Temp\VaccineMonitoring\20180517095738998-lastResult");
            doc1.LoadHtml(text1);

            //var node = doc1.DocumentNode.SelectSingleNode("//form[@id = 'product_addtocart_form']/@action");
            var node1 = doc1.DocumentNode.SelectSingleNode("//input[@type=\"hidden\" and @name=\"form_key\"]/@value");
            var toRemove1 = node1.Attributes[2].Value;

            var doc2 = new HtmlDocument();
            var text2 = File.ReadAllText(@"C:\Temp\VaccineMonitoring\20180517095738998-result");
            doc2.LoadHtml(text2);

            var node2 = doc2.DocumentNode.SelectSingleNode("//input[@type=\"hidden\" and @name=\"form_key\"]/@value");
            var toRemove2 = node2.Attributes[2].Value;

            var a = text1.Replace(toRemove1, string.Empty) == text2.Replace(toRemove2, string.Empty);
        }
    }
}
