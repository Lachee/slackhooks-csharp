using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SlackWebhooks
{
    /// <summary>
    /// The Slackhook class
    /// </summary>
    class SlackWebhook
    {
        //The link to slack
        private string webhook;

        /// <summary>
        /// The name of the webhook bot
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The emoji or URL of the webhook bot's avatar
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// The default channel used by the webhook
        /// </summary>
        public string DefaultChannel { get; set; }

        /// <summary>
        /// Should it use the Markdown formatting?
        /// </summary>
        public bool UseMarkdown { get; set; }

        /// <summary>
        /// Should we force this client to always send asyncronous messages? This will make Send(); and SendAsync(); do exactly the same thing.
        /// </summary>
        public bool forceAsyncronus = false;

        /// <summary>
        /// Creates a new instance of a SlackWebhook
        /// </summary>
        /// <param name="WebhookUrl">Unique URL that is generated from target Slack Incomming Webhook</param>
        /// <param name="name">Name of the Webhook bot</param>
        /// <param name="icon">A emoji or URL of the icon</param>
        /// <param name="defaultChannel">What channel to send all undirected messages too? Default: #-lobby-</param>
        public SlackWebhook(string WebhookUrl, string name = "", string icon = "", string defaultChannel = "#-lobby-")
        {
            this.webhook = WebhookUrl;
            this.Name = name;
            this.Icon = icon;
            this.DefaultChannel = defaultChannel;
        }

        /// <summary>
        /// Does this Webhook have a emoji icon? If not, it must be using a URL icon.
        /// </summary>
        /// <returns>true if emoji</returns>
        public bool UsingEmoji()
        {
            return !Icon.Contains("http");
        }

        /// <summary>
        /// Sends a message to Slack.
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <param name="title">The optional title to append to the bots name</param>
        /// <param name="channel">The channel to send the message to. If empty, defaultChannel is used instead.</param>
        /// <param name="attachments">A array of SlackAttachments.</param>
        /// <returns>true on success</returns>
        public bool Send(string message, string title = "", string channel = "", SlackAttachment[] attachments = null)
        {
            string payload = PreparePayload(message, title, channel, attachments);
            return SendPayload(payload, forceAsyncronus);
        }

        /// <summary>
        /// Sends a message to Slack asyncronously.
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <param name="title">The optional title to append to the bots name</param>
        /// <param name="channel">The channel to send the message to. If empty, defaultChannel is used instead.</param>
        /// <param name="attachments">A array of SlackAttachments.</param>
        /// <returns>true on success</returns>
        public bool SendAsync(string message, string title = "", string channel = "", SlackAttachment[] attachments = null)
        {
            string payload = PreparePayload(message, title, channel, attachments);
            return SendPayload(payload, true);
        }

        #region Internal Helpers
        private bool SendPayload(string payload, bool async)
        {
            try
            {
                //Create the HTTP Client
                HttpClient client = new HttpClient();
                
                //Add the payload contents
                List<KeyValuePair<string, string>> fields = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("payload", payload)
                };

                //Send the async call
                var asyncpost = client.PostAsync(this.webhook, new FormUrlEncodedContent(fields));
                if (!async)
                {
                    //If we are not in async mode, we are going to wait for a response
                    var response = asyncpost.Result;

                    //We have failed somehow
                    if (!response.IsSuccessStatusCode)
                        return false;

                    //See if slack was successfull
                    return response.Content.ReadAsStringAsync().Result == "ok";
                }

                //We are in async mode, so we are always successfull because screw callbacks... am I rite?!
                return true;
            }
            catch 
            {                
                //A error has occured, so return false.
                return false;
            }
        }
        private string PreparePayload(string message, string title, string channel, SlackAttachment[] attachements)
        {
            //Prepare the string builder
            StringBuilder sb = new StringBuilder();

            //Prepare the name
            string username = this.Name;
            if (title != "") username += " - " + title;
            if (this.Name == "") username = title;
            sb.Append(SlackHelper.SimpleJSONString("username", username)); sb.Append(", ");

            //Prepare the attachments
            if (attachements != null && attachements.Length > 0)
            {
                //Temporary holder of all attachments
                string attjson = "";

                //Iterate over each attachment, adding it to the temp json
                foreach (SlackAttachment attachment in attachements)
                    attjson += (attjson.Length != 0 ? ", " : "") + attachment.GenerateJSON();
                
                //Append the temporary json to the main body
                sb.Append(SlackHelper.SimpleJSONValue("attachments", "[" + attjson + "]")); sb.Append(", ");
            }

            //Prepare the icon
            sb.Append(SlackHelper.SimpleJSONString(UsingEmoji() ? "icon_emoji" : "icon_url", this.Icon)); sb.Append(", ");

            //Prepare the channel
            if (channel == "") channel = DefaultChannel;
            sb.Append(SlackHelper.SimpleJSONString("channel", channel)); sb.Append(", ");

            //Prepare the message
            sb.Append(SlackHelper.SimpleJSONString("text", message)); sb.Append(", ");
            sb.Append(SlackHelper.SimpleJSONValue("mrkdown", UseMarkdown));

            //return the formated json
            return "{" + sb.ToString() + "}";
        }
        #endregion

    }

    /// <summary>
    /// Attachments for slack messages
    /// </summary>
    class SlackAttachment
    {
        #region Variables
        private string fallback = "";
        private string text = "";
        private int timestamp;

        private string pretext = "";
        private string color = "";

        private string image_url = "";
        private string thumb_url = "";

        private string title = "";
        private string title_link = "";

        private string footer = "";
        private string footer_icon = "";

        private string author_name = "", author_link = "", author_icon = "";
        
        private List<SlackField> fields = new List<SlackField>();
        #endregion
        
        /// <summary>
        /// Creates a new attachment object
        /// </summary>
        /// <param name="text">The main body of the attachment</param>
        /// <param name="fallback">The fallback text of the body</param>
        public SlackAttachment(string text, string fallback)
        {
            this.text = text;
            this.fallback = fallback;

            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            this.timestamp = (int)t.TotalSeconds; 
        }

        /// <summary>
        /// Fields are displayed as a table in the attachment block
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="text">The text within the field</param>
        /// <param name="isShort">Does it take up 1 space or 2? If true, it will take up 1 space.</param>
        public SlackAttachment AddField(string title, string text, bool isShort = false)
        {
            SlackField field = new SlackField() { title = title, text = text, isShort = isShort };
            fields.Add(field);
            return this;
        }
       
        
        /// <summary>
        /// Sets the epoch timestamp of the message. 
        /// </summary>
        /// <param name="timestamp"></param>
        public SlackAttachment SetTimestamp(int timestamp)
        {
            this.timestamp = timestamp;
            return this;
        }

        /// <summary>
        /// Some text that comes before the attachment block.
        /// </summary>
        /// <param name="pretext"></param>
        public SlackAttachment SetPretext(string pretext)
        {
            this.pretext = pretext;
            return this;
        }

        /// <summary>
        /// Sets the color of the attachment block sidebar. Accepts hexadecimal encoded colors. 'good', 'warning' and 'danger' are also accepted.
        /// </summary>
        /// <param name="color"></param>
        public SlackAttachment SetColor(string color)
        {
            this.color = color;
            return this;
        }

        /// <summary>
        /// Adds a image to the attachment.
	    /// A valid URL to an image file that will be displayed inside a message attachment.We currently support the following formats: GIF, JPEG, PNG, and BMP.
	    /// Large images will be resized to a maximum width of 400px or a maximum height of 500px, while still maintaining the original aspect ratio.
        /// </summary>
        /// <param name="url"></param>
        public SlackAttachment SetImage(string url)
        {
            this.image_url = url;
            return this;
        }

        /// <summary>
        /// Adds a icon to the attachment
	 	/// A valid URL to an image file that will be displayed as a thumbnail on the right side of a message attachment.We currently support the following formats: GIF, JPEG, PNG, and BMP.
	    /// The thumbnail's longest dimension will be scaled down to 75px while maintaining the aspect ratio of the image. The filesize of the image must also be less than 500 KB.
	    /// For best results, please use images that are already 75px by 75px.
        /// </summary>
        /// <param name="url"></param>
        public SlackAttachment SetThumbnail(string url)
        {
            this.thumb_url = url;
            return this;
        }

        /// <summary>
        /// Sets the title of the attachment with a optional URL
        /// </summary>
        /// <param name="title">The heading of the attachment</param>
        /// <param name="url">Link for the title</param>
        public SlackAttachment SetTitle(string title, string url = "")
        {
            this.title = title;
            this.title_link = url;
            return this;
        }

        /// <summary>
        /// Sets the footer of the attachment with a optional icon.
        /// </summary>
        /// <param name="footer"></param>
        /// <param name="iconURL"></param>
        public SlackAttachment SetFooter(string footer, string iconURL = "")
        {
            this.footer = footer;
            this.footer_icon = iconURL;
            return this;
        }
        
        /// <summary>
        /// Sets the author of the attachment with a optional link and URL.
        /// </summary>
        /// <param name="author"></param>
        /// <param name="url"></param>
        /// <param name="iconURL"></param>
        public SlackAttachment SetAuthor(string author, string url = "", string iconURL = "")
        {
            this.author_name = author;
            this.author_link = url;
            this.author_icon = iconURL;
            return this;
        }

        /// <summary>
        /// Generates a JSON String repesentation of the attachment with Slack formatting.
        /// </summary>
        /// <returns></returns>
        public string GenerateJSON()
        {
            StringBuilder sb = new StringBuilder();

            //Requirements
            sb.Append(SlackHelper.SimpleJSONString("fallback", this.fallback)); sb.Append(",");
            sb.Append(SlackHelper.SimpleJSONString("text", this.text)); sb.Append(",");
            sb.Append(SlackHelper.SimpleJSONValue("ts", this.timestamp)); sb.Append(",");

            //Slack fields
            //Prepare the field text
            string fieldjson = "";
            foreach (SlackField field in fields)
            {
                string json = "";
                json += SlackHelper.SimpleJSONString("title", field.title) + ",";
                json += SlackHelper.SimpleJSONString("value", field.text) + ",";
                json += SlackHelper.SimpleJSONValue("short", field.isShort);

                fieldjson += (fieldjson.Length != 0 ? ", " : "") + "{" + json + "}";
            }

            //Append it
            sb.Append(SlackHelper.SimpleJSONValue("fields", "[" + fieldjson + "]"));  sb.Append(",");

            //Formating and decoratives
            if (color != "")
            {
                sb.Append(SlackHelper.SimpleJSONString("color", this.color));
                sb.Append(",");
            }
            if (pretext != "") { 
                sb.Append(SlackHelper.SimpleJSONString("pretext", this.pretext));
                sb.Append(",");
            }
            if (image_url != "")
            {
                sb.Append(SlackHelper.SimpleJSONString("image_url", this.image_url));
                sb.Append(",");
            }
            if (thumb_url != "")
            {
                sb.Append(SlackHelper.SimpleJSONString("thumb_url", this.thumb_url));
                sb.Append(",");
            }

            //Titles
            if (title != "")
            {
                sb.Append(SlackHelper.SimpleJSONString("title", this.title));
                sb.Append(",");
            }
            if (title_link != "")
            {
                sb.Append(SlackHelper.SimpleJSONString("title_link", this.title_link));
                sb.Append(",");
            }

            //Footer
            if (footer != "")
            {
                sb.Append(SlackHelper.SimpleJSONString("footer", this.footer));
                sb.Append(",");
            }
            if (footer_icon != "")
            {
                sb.Append(SlackHelper.SimpleJSONString("footer_icon", this.footer_icon));
                sb.Append(",");
            }

            //Author
            if (author_name != "")
            {
                sb.Append(SlackHelper.SimpleJSONString("author_name", this.author_name));
                sb.Append(",");
            }
            if (author_link != "")
            {
                sb.Append(SlackHelper.SimpleJSONString("author_link", this.author_link));
                sb.Append(",");
            }
            if (author_icon != "")
            {
                sb.Append(SlackHelper.SimpleJSONString("author_icon", this.author_icon));
                sb.Append(",");
            }

            //Remove the last comma
            sb.Remove(sb.Length - 1, 1);

            return "{" + sb.ToString() + "}";
        }

        struct SlackField
        {
            public string title;
            public string text;
            public bool isShort;
        }
    }
    
    /// <summary>
    /// A helper class that has basic JSON formating functions a Slack helper functions
    /// </summary>
    class SlackHelper
    {
        /// <summary>
        /// Encapsulates a name and value into JSON formatting.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string SimpleJSONValue(string name, object value)
        {
            name = name.Replace("\\", "\\\\").Replace("\"", "\\\"");

            string vt = value.ToString();
            if (value is bool) vt = vt.ToLower();

            return "\"" + name + "\": " + vt;
        }

        /// <summary>
        /// Encapsulates a name and value into JSON formatting. The value is escaped and ecapsulated in quotations.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string SimpleJSONString(string name, string value)
        {
            value = value.Replace("\\", "\\\\").Replace("\"", "\\\"");
            return SimpleJSONValue(name, "\"" + value + "\"");
        }

        /// <summary>
        /// Creates a Slack Message Formatted link to be used in messages.
        /// </summary>
        /// <param name="url">URL of the link</param>
        /// <param name="name">Optional title for the link</param>
        /// <returns></returns>
        public static string CreateLink(string url, string name = "")
        {
            //Format the URL
            url = url.Replace("<", "%3C").Replace(">", "%3E").Replace("|", "%7C");

            //Format the name
            name = name == "" ? url : name.Replace("<", "%3C").Replace(">", "%3E").Replace("|", "%7C");

            //Return the formated link
            return "<" + url + "|" + name + ">";
        }
    }
}
