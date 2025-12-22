using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using BahyWay.SharedKernel.Application.Abstractions;

namespace BahyWay.SharedKernel.Infrastructure
{
    public class JsonMessageResolver : IMessageResolver
    {
        private Dictionary<string, string> _errors = new();
        private Dictionary<string, string> _scores = new();
        private readonly string _filePath;

        public JsonMessageResolver(string filePath)
        {
            _filePath = filePath;
            LoadMessages();
        }

        private void LoadMessages()
        {
            if (!File.Exists(_filePath)) return;

            var json = File.ReadAllText(_filePath);
            var root = JsonSerializer.Deserialize<MessageRoot>(json);

            if (root != null)
            {
                _errors = root.Errors ?? new();
                _scores = root.Scores ?? new();
            }
        }

        public string GetError(string code) =>
            _errors.ContainsKey(code) ? _errors[code] : $"Unknown Error [{code}]";

        public string GetError(string code, params object[] args) =>
            string.Format(GetError(code), args);

        public string GetScoreMessage(string code) =>
            _scores.ContainsKey(code) ? _scores[code] : $"Unknown Score Code [{code}]";

        public string GetScoreMessage(string code, params object[] args) =>
            string.Format(GetScoreMessage(code), args);

        // Helper Class for JSON Deserialization
        private class MessageRoot
        {
            public Dictionary<string, string> Errors { get; set; }
            public Dictionary<string, string> Scores { get; set; }
        }
    }
}