using Azure;
using Azure.AI.OpenAI;
using AzureAIDemoApp;
using OpenAI.Chat;
using System.Text.Json;


var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
var filepath = Path.Combine(appdata, "secrets.json");
var json = File.OpenRead(filepath);
var secrets = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
var endpoint = secrets["TargetURI"];
var key = secrets["Key"];

AzureKeyCredential credential = new AzureKeyCredential(key);
AzureOpenAIClient azureClient = new(new Uri(endpoint), credential);
ChatClient chatClient = azureClient.GetChatClient("gpt-35-turbo");

// ADJUST RETENTION AS NEEDED
var rententionCount = 3;
var orchestrator = new Orchestrator(chatClient, rententionCount);

while (true)
{
    Console.Write("You: ");
    string message = Console.ReadLine();
    if (message == "exit")
    {
        break;
    }

    ChatCompletion completion = await orchestrator.GetResponse(message);
    foreach (var choice in completion.Content)
    {
        Console.WriteLine($"Bot: {choice.Text}");
    }
}