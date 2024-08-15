using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureAIDemoApp
{
    internal class Orchestrator
    {
        private readonly ChatClient _chatClient;
        private readonly int retentionCount;
        private readonly List<ChatMessage> _messages = [];

        public Orchestrator(ChatClient chatClient, string metaPrompt, int retentionCount)
        {
            _chatClient = chatClient;
            this.retentionCount = retentionCount;
            if (!string.IsNullOrWhiteSpace(metaPrompt))
            {
                _messages.Add(new SystemChatMessage(metaPrompt));
                this.retentionCount += 1;
            }
        }

        public async Task<ChatCompletion> GetResponse(string message, bool verbose = false)
        {
            AddUserMessageToMessages(message);

            if (verbose)
            {
                Console.WriteLine("Content Being Sent:");
                foreach (var m in _messages)
                {
                    Console.WriteLine(m.GetType().Name + ": " + string.Join(" ", m.Content.Select(x => x.Text)));
                }
            }

            ChatCompletion completion = await CompleteChatAsync();
            AddBotMessageToMessages(completion);
            return completion;
        }

        private void AddUserMessageToMessages(string message)
        {
            _messages.Add(new UserChatMessage(message));
            _messages.RemoveAt(1);
        }

        private async Task<ChatCompletion> CompleteChatAsync()
        {
            return await _chatClient.CompleteChatAsync(
                _messages,
                new ChatCompletionOptions()
                {
                    Temperature = (float)0.7,
                    MaxTokens = 200,
                    FrequencyPenalty = 0,
                    PresencePenalty = 0,
                }
            );
        }

        private void AddBotMessageToMessages(ChatCompletion completion)
        {
            var assistantMessages = completion.Content.Select(c => c.Text);
            _messages.Add(new AssistantChatMessage(string.Join("\n", assistantMessages)));
            _messages.RemoveAt(1);
        }
    }
}
