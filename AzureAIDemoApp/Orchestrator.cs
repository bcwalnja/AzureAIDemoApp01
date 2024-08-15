﻿using OpenAI.Chat;
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
        private readonly string metaPrompt;
        private readonly int retentionCount;
        private readonly List<ChatMessage> _messages = [];

        public Orchestrator(ChatClient chatClient, string metaPrompt, int retentionCount)
        {
            _chatClient = chatClient;
            this.metaPrompt = metaPrompt;
            this.retentionCount = retentionCount;
            _messages.Add(new SystemChatMessage(metaPrompt));
        }

        public async Task<ChatCompletion> GetResponse(string message)
        {
            AddUserMessageToMessages(message);
            ChatCompletion completion = await CompleteChatAsync();
            AddBotMessageToMessages(completion);
            return completion;
        }

        private void AddUserMessageToMessages(string message)
        {
            _messages.Add(new UserChatMessage(message));

            if (_messages.Count > retentionCount)
            {
                _messages.RemoveAt(1);
            }
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

            if (_messages.Count > retentionCount)
            {
                _messages.RemoveAt(1);
            }
        }
    }
}
