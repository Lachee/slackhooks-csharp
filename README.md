# Slack Hooks #
A lightweight library to send messages via C# to Slack. Simple and easy to use. All you need is the SlackWebhook.cs file and you are ready to go!

Read the Program.cs script for examples on how to use the features of the SlackWebhook class and how you can implemented it in your own projects. Further information on how specific formatting can be done can be found on slacks developers api page: https://api.slack.com/docs/messages. This project supports most of the features that slack provides except for buttons. 

Example of attachments: http://take.ms/bQfUT

Feel free to modify, add and change as you wish. I will be happy to accept any merges that add/fixes any issues.

### Sister Libraries
Be sure to check out the sister libraries that are in various different lanaguages!

Name			| Lang| Link
------------------------|-----|--------------------------------------------
Slackhooks 		| PHP | https://github.com/Lachee/slackhooks
Slackhooks CSharp 	| C#  | https://github.com/Lachee/slackhooks-csharp


## Setup ##
It is incredibly easy to use this library! 
	1.  Download or copy the contents of webhook.php to a file of your choosing.
	2.  Include the SlackWebhooks library to any code you plan to use it with
	```csharp
   	using SlackWebhooks;
	```
	3.  Profit?

To be fair, there are some other minor steps to get it working. For example, you must grab your Incoming Webhook URL from slack. It should look like this:
``` https://hooks.slack.com/services/XXXXXXXXX/YYYYYYYYY/ZZZZZZZZZZZZZZZZZZZZZZZZ ```

Once you have your hook, you are ready to go! Follow the tutorials below to find more details!

## Basic Use ##
Here is a quick tutorial for basic use. This PHP library makes it incredibly easy to use!
First of all, we need to define what our Slack Incoming Webhook URL is. We are going to use the following in all tutorials:
```csharp 
string webhookUrl = "https://hooks.slack.com/services/XXXXXXXXX/YYYYYYYYY/ZZZZZZZZZZZZZZZZZZZZZZZZ";
```
Once you have that ready, we will begin! Here is a small snippet for sending a basic message with a ghost icon, like so http://take.ms/rLTbi

```csharp
//Prepare our webhook url    
string webhookUrl = "https://hooks.slack.com/services/T00000000/B00000000/XXXXXXXXXXXXXXXXXXXXXXXX";

//Create the webhook object
//args: Webhook URL, default name, default icon, [optional] default channel
SlackWebhook webhook = new SlackWebhook(webhookUrl, "Koala Bot", ":koala:", "#-lobby-");

//Send the message
success = webhook.Send("Hello World!");
Console.WriteLine("Send: " + success);            
```
As you can see, very similar to the PHP library.

### Init

Now, here is where the fun begins. We are going to create a new SlackWebhook object. This object is what sends the messages and helps us format them too! It takes 3 parameters in its constructor and a optional 4th. More on this later.

```csharp
SlackWebhook webhook = new SlackWebhook(webhookUrl, "Lachee Bot", ":ghost:");
``` 
    
### Send
Now for the good stuff! With a SlackWebhook object, you can simply call the send() function with your message. There are lots of different ways to send messages, but this is a basic "Hello World". 
```csharp
bool result = webhook.Send("Hello World!");
```

Done! If this is all you wanted to do with your PHP script, you are ready to go! If you want to do more, continue reading!

## Advance Use
### Constructor
The constructor takes 3 arguments with a optional 4th. They are listed below and are used to customise your message slightly.

```csharp
SlackWebhook webhook = new SlackWebhook(webhookUrl, "Lachee Bot", ":ghost:", "@Lachee");
SlackWebhook webhook = new SlackWebhook(webhookUrl, "Lachee Bot", "http://example.com/myfancyicon.png", "#-lobby-");
``` 

Param  		 	|Req| Purpose
----------------|---|-------------
Webhook URL  	| Y | The URL that Slack has generated for your incoming webhooks
Bot Name  		| Y	| The default name for the bot. It will automatically get added to every message.
Bot Icon   		| Y	| The Slack Emoji icon or a URL to a icon for your bot to use as a avatar.
Default Channel | N	| A optional variable that will set the default channel to send messages to. By default its the #-lobby- channel.

### Send
The send function is the heart of operations. It will push your messages onto the Slack stack. It comes in 4 flavours.
```csharp
//Standard Use
webhook.Send("message");									//Sends "Message"
webhook.Send("message", "title");							//Appends " - title" to botname
webhook.Send("message", "title", "#channel");				//Sends it to #channel
webhook.Send("message", "title", "#channel", new SlackAttachment[] { attachment });		//Adds block attachments

//Some specifiers
webhook.Send("message", channel: "#-lobby-");
```
Param  		 	|Req| Purpose
----------------|---|-------------
Message	     	| Y | The message to send to slack.
Title    		| N	| The title to append to the botname. If left empty, nothing is appended.
Channel   		| N	| The slack channel to send too. If left empty, the default channel is used.
Attachments		| N	| An array of SlackAttachment objects. Defaults to a empty array. See [Slack Attachment](https://api.slack.com/docs/message-attachments) for more details on how they work and what formatting options are available (There are lots and lots and lots).

### SendAsync
Same as Send however asynchronously. C# default way of handling POST calls is async in nature. When you are sending a non-async, it is just sending a asyncing and waiting for the response.
SendAsync will always return true unless there is a error with HttpClient

### CreateLink
Slack uses a its own Markdown formatting for its links. You cannot simply send a href or a normal markdown. The format Slack uses is as such: ```<http://example.com/|Example>``` which would result as such: [Example](http://example.com/). SlackWebhook adds a tool that will format URL's into this markdown for you and strip away any <, > or | and replaces them with their HTML equivilant. It is recommended using this method to avoid accidental url unescaping.
```csharp
string link = SlackHelper.CreateLink("http://google.com/", "Visit Google");
Console.WriteLine(link);		//output: <http://google.com/|Visit Google>
```

### Ignore SSL
This feature is not implemented in the C# version of the library as HttpClient handles it automatically.

## Slack Attachments
This is a class in itself and I am far to lazy to explain every single feature. As a short summary, make sure you visit [Slack's Attachment API](https://api.slack.com/docs/message-attachments). 

In short, this feature allows for more complex messages to be sent to Slack such as those fancy coloured blocks you see and attachments of images. Be sure to check it out :) Here is a quick example of every function in its fullness:
```csharp
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
```

Be sure to check out the inline documentation in the SlackWebhook.cs file.
Thank you for reading all of this and I hope you make wonderfull applications. Be sure to fork and merge any fixes or suggestions you would like to see be implemented.

Dont forget to check out the sister libraries too ;)
---

