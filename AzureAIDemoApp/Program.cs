using Azure;
using Azure.AI.OpenAI;
using AzureAIDemoApp;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

Console.WriteLine("Loading URI and Access Key");

var builder = new ConfigurationBuilder()
    .AddUserSecrets<Program>();
var configuration = builder.Build();
var endpoints = configuration.GetSection("Endpoints").GetChildren();
var chatClients = new List<ChatClient>();

Console.WriteLine("Loading Chat Clients");

foreach (var endpoint in endpoints)
{
    var endpointName = endpoint["Name"];
    var endpointDeployment = endpoint["Deployment"];
    var endpointUri = endpoint["TargetURI"];
    var endpointKey = endpoint["Key"];
    Console.WriteLine($"{endpointName}: {endpointDeployment}");
    AzureKeyCredential credential = new AzureKeyCredential(endpointKey);
    AzureOpenAIClient azureClient = new AzureOpenAIClient(new Uri(endpointUri), credential);
    chatClients.Add(azureClient.GetChatClient(endpointDeployment));
}

Console.WriteLine($"Select a chat client: [0-{chatClients.Count - 1}]");

var userChoice = Console.ReadLine();
var chatClient = chatClients[int.Parse(userChoice)];

// ADJUST RETENTION AS NEEDED
var metaPrompt = "As an SMS bot, you must never exceed 250 characters.";
var rententionCount = 3;
Console.WriteLine("Starting Chat Orchestrator");
Console.WriteLine("Meta Prompt: " + metaPrompt);
Console.WriteLine("Retention Count: " + rententionCount);
var orchestrator = new Orchestrator(chatClient, metaPrompt, rententionCount);
Console.WriteLine("Type 'exit' to quit.");

while (true)
{
    Console.Write("You: ");
    string message = Console.ReadLine();
    if (message == "exit")
    {
        break;
    }

    //optional verbose flag
    try
    {
        ChatCompletion completion = await orchestrator.GetResponse(message);
        foreach (var choice in completion.Content)
        {
            Console.WriteLine($"Bot: {choice.Text}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}