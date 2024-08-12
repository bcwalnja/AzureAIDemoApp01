using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.Text.Json;

var filepath = @"C:\Users\User\.secrets\secrets.json";
var json = File.OpenRead(filepath);
var secrets = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
var endpoint = secrets["TargetURI"];
var key = secrets["Key"];

AzureKeyCredential credential = new AzureKeyCredential(key);
AzureOpenAIClient azureClient = new(new Uri(endpoint), credential);
ChatClient chatClient = azureClient.GetChatClient("gpt-35-turbo");

var message = "How do I install Visual Studio?";

ChatCompletion completion = chatClient.CompleteChat( [ new SystemChatMessage(message) ],
  new ChatCompletionOptions()
  {
      Temperature = (float)0.7,
      MaxTokens = 800,
      FrequencyPenalty = 0,
      PresencePenalty = 0,
  }
);

foreach (var content in completion.Content)
{
    Console.WriteLine($"{content.Kind}: { content.Text}"); 
}

