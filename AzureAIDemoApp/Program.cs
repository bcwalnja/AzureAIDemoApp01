using Azure;
using Azure.AI.OpenAI;
using AzureAIDemoApp;
using OpenAI.Chat;
using System.Text.Json;

var chatClients = LoadAvailableChatClients();

Console.WriteLine($"Select a chat client: [0-{chatClients.Count - 1}]");

var userChoice = Console.ReadLine();
var chatClient = chatClients[int.Parse(userChoice!)];

// **** Try adjusting the meta prompt

var metaPrompt = string.Empty;
//var metaPrompt = "You are a short-tempered AI assistant.";
//var metaPrompt = "You are an extremely cheerful AI assistant. " +
//    "Help people and improve their day!";
//var metaPrompt = "You are an AI assistant. You respond via SMS. " +
//    "You must never exceed 140 characters.";

// **** Try adjusting the retention count
var rententionCount = 3;
//var rententionCount = 6;

// **** Try adjusting verbose
var verbose = false;


Console.WriteLine("Starting Chat Orchestrator");
Console.WriteLine("Meta Prompt: " + metaPrompt);
Console.WriteLine("Retention Count: " + rententionCount);

var orchestrator = new Orchestrator(chatClient, metaPrompt, rententionCount);

Console.WriteLine("Type 'exit' to quit.");

while (true)
{
    Console.Write("You: ");
    string message = Console.ReadLine()!;
    if (message == "exit")
    {
        break;
    }

    //optional verbose flag
    ChatCompletion completion = await orchestrator.GetResponse(message, verbose);
    foreach (var value in completion.Content)
    {
        Console.WriteLine($"Bot: {value.Text}");
    }
}

static List<ChatClient> LoadAvailableChatClients()
{
    Console.WriteLine("Loading URI and Access Key");
    var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    var filepath = Path.Combine(appdata, "secrets.json");
    var json = File.OpenRead(filepath);
    var secrets = JsonSerializer.Deserialize<JsonElement>(json);
    var endpoints = secrets.GetProperty("Endpoints");
    var chatClients = new List<ChatClient>();

    Console.WriteLine("Loading Chat Clients");

    foreach (var endpoint in endpoints.EnumerateArray())
    {
        var endpointName = endpoint.GetProperty("Name").GetString();
        var endpointDeployment = endpoint.GetProperty("Deployment").GetString();
        var endpointUri = endpoint.GetProperty("TargetURI").GetString();
        var endpointKey = endpoint.GetProperty("Key").GetString();
        Console.WriteLine($"{endpointName}: {endpointDeployment}");
        AzureKeyCredential credential = new AzureKeyCredential(endpointKey);
        AzureOpenAIClient azureClient = new AzureOpenAIClient(new Uri(endpointUri), credential);
        chatClients.Add(azureClient.GetChatClient(endpointDeployment));
    }

    return chatClients;
}