using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlackWebhooks
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Slack Webhook Example! Creating Webhook and sending messages...");

            //First prepare the webhook url and other parameters
            string webhookUrl = "https://hooks.slack.com/services/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";
            string botname = "Robo Koala";
            string boticon = ":koala:";
            string channel = "@Lachee";

            //This will be used later on for storing success results
            bool success = false;

            //Create the webhook
            SlackWebhook webhook = new SlackWebhook(webhookUrl, botname, boticon, channel);
            webhook.forceAsyncronus = false;

            //================================= Basic Sending
            //Send a simple message
            success = webhook.Send("`Hello World!`");
            Console.WriteLine("Send: " + success);
            
            //Send a message with a title
            success = webhook.Send("Hello World!", "Welcome");
            Console.WriteLine("Send Title: " + success);

            //Send a message to specific channel. Channels can be convos with @ or normal channels with #
            success = webhook.Send("Hello World!", "", "@Lachee");          //This is a standard approach
            success = webhook.Send("Hello World!", channel: "@Lachee");    //This is a approch where you can skip unneeded variables
            Console.WriteLine("Send Channel: " + success);

            //Send it all!
            success = webhook.Send("Hello World!", "Welcome", "@Lachee");
            Console.WriteLine("Send All: " + success);

            //Here is a async example with a timmer to show that its continuing on.
            //Note while it is async, it still has a small initalization time due to HttpClient.
            var watch = System.Diagnostics.Stopwatch.StartNew();
            success = webhook.SendAsync("Hellow World!", "Async");
            watch.Stop();
            Console.WriteLine("Async Call: " + success + " @ " + watch.ElapsedMilliseconds + "ms");

            //================================= Some Formatting
            //Create a link to be used and send it
            string link = SlackHelper.CreateLink("http://google.com/", "Visit Google");
            success = webhook.Send("Be sure to " + link + "! It can be very helpfull");
            Console.WriteLine("Links: " + success + " - " + link);

            //================================= Create a basic attachment
            //Be sure to check out https://api.slack.com/docs/message-attachments for more details about each element.
            SlackAttachment attachement = new SlackAttachment("This is a attachment message", "Plaintext Fallback Message");

            //You can do them indivdually like so:
            attachement.SetColor("#36a64f");
            attachement.SetColor("danger");
            attachement.SetPretext("some text that is before block");

            //Or you can do it all inline! (Such Java like, much wow)
            attachement.SetTitle("When Dropbears Attack", "https://en.wikipedia.org/wiki/Drop_bear").SetFooter("Legit Koala Exhbit");

            //Oh, don't forget to actually send it
            success = webhook.Send("Here is a interesting facts about dropbears", "News", attachments: new SlackAttachment[] { attachement });
            Console.WriteLine("Attachments: " + success);

            Console.WriteLine("Done! Press any key to end.");
            Console.ReadKey();
        }
    }
}
