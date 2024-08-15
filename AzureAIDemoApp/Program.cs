using Azure;
using Azure.AI.OpenAI;
using AzureAIDemoApp;
using OpenAI.Chat;
using System.Text.Json;


Console.WriteLine("Loading URI and Access Key");
var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
var filepath = Path.Combine(appdata, "secrets.json");
var json = File.OpenRead(filepath);
var secrets = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
var endpoint = secrets["TargetURI"];
var key = secrets["Key"];

Console.WriteLine("Creating Chat Client");
AzureKeyCredential credential = new AzureKeyCredential(key);
AzureOpenAIClient azureClient = new(new Uri(endpoint), credential);
ChatClient chatClient = azureClient.GetChatClient("gpt-35-turbo");

// ADJUST RETENTION AS NEEDED
var metaPrompt = "You are a friendly AI assistant.";
var rententionCount = 3;
Console.WriteLine("Starting Chat Orchestrator");
Console.WriteLine("Meta Prompt: " + metaPrompt);
Console.WriteLine("Retention Count: " + rententionCount);
var orchestrator = new Orchestrator(chatClient, metaPrompt, rententionCount);
Console.WriteLine("Type 'exit' to quit.");

while (true)
{
    // add logging to this code
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