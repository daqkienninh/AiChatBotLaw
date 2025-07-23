using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using OpenAI;
using OpenAI.GPT3;
using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.Managers;
using Repositories;
using Repositories.DBContext;
using Repositories.Models;
using Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.ObjectModels;


namespace Services.Implement
{
    public class AnswerService : IAnswerService
    {
        private readonly AnswerRepository _answerRepository;
        private readonly MongoClient _mongoClient;
        private readonly MongoDbSettings _settings;
        private readonly OpenAIOptions _openAiOptions;
        private readonly IOpenAIService _openAIService;
        public AnswerService(IOptions<MongoDbSettings> settings, IOptions<OpenAIOptions> openAiOptions) 
        { 
            _answerRepository = new AnswerRepository();
            _settings = settings.Value;
            _mongoClient = new MongoClient(_settings.ConnectionString);
            _openAiOptions = openAiOptions.Value;
            _openAIService = new OpenAIService(new OpenAiOptions
        {
            ApiKey = _openAiOptions.ApiKey
        });
        }

        public async Task CreateAnswerFromQuestionAsync(Question question)
        {
            if (string.IsNullOrWhiteSpace(question.QuestionContent))
                throw new ArgumentException("Question content is empty.");

            var questionEmbedding = ParseEmbedding(question.Embedding);
            if (questionEmbedding.Count == 0)
                throw new InvalidOperationException("Question embedding is missing or invalid.");

            // Get MongoDB
            var mongoDb = _mongoClient.GetDatabase(_settings.DatabaseName);

            // Tìm điều khoản phù hợp nhất
            var (matchedClauseText, score, matchedClauseId, clauseReference) = await GetBestMatchClauseAsync(questionEmbedding, mongoDb);

            const float threshold = 0.3f;

            // Tạo prompt cho mô hình fine-tuned với trích dẫn cụ thể
            string prompt = $"Bạn là một trợ lý pháp luật, trả lời chính xác dựa trên luật Việt Nam. Dựa trên câu hỏi: '{question.QuestionContent}'. ";
            if (!string.IsNullOrWhiteSpace(matchedClauseText) && score >= threshold)
            {
                prompt += $"Trích dẫn pháp luật: '{clauseReference}' - Nội dung: '{matchedClauseText}'. Hãy trả lời tự nhiên và trích dẫn điều khoản hoặc điểm cụ thể khi phù hợp.";
            }
            else
            {
                prompt += "Hiện tại không có quy định pháp luật phù hợp. Hãy trả lời một cách tổng quát và hữu ích.";
            }

            // Gọi mô hình fine-tuned để tạo câu trả lời
            var completionRequest = new ChatCompletionCreateRequest
            {
                Model = _openAiOptions.ChatModel, // Sử dụng mô hình fine-tuned
                Messages = new List<ChatMessage>
            {
                ChatMessage.FromSystem(prompt),
                ChatMessage.FromUser(question.QuestionContent)
            },
                Temperature = 0.2f,
                MaxTokens = 512
            };

            string generatedAnswer;
            try
            {
                var completionResult = await _openAIService.ChatCompletion.CreateCompletion(completionRequest);
                if (completionResult == null || completionResult.Choices == null || !completionResult.Choices.Any())
                {
                    generatedAnswer = "Hiện tại hệ thống chưa tìm được quy định pháp luật phù hợp với câu hỏi.";
                }
                else
                {
                    generatedAnswer = completionResult.Choices.First().Message.Content?.Trim() ??
                                    "Hiện tại hệ thống chưa tìm được quy định pháp luật phù hợp với câu hỏi.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi gọi API OpenAI: {ex.Message}");
                generatedAnswer = "Xin lỗi, hệ thống gặp sự cố khi tạo câu trả lời.";
            }

            // Ghi vào SQL
            var answer = new Answer
            {
                AnswerId = question.QuestionId, // Tạo ID mới cho câu trả lời
                QuestionId = question.QuestionId,
                AnsContent = generatedAnswer + "Trích " + clauseReference,
                LegalclauseId = matchedClauseId,
                AnsCreateAt = DateTime.Now
            };

            _answerRepository.AddAnswer(answer);
        }


        private float CosineSimilarity(List<float> vector1, List<float> vector2)
        {
            if (vector1.Count != vector2.Count) return 0f;

            float dotProduct = 0f, magnitude1 = 0f, magnitude2 = 0f;
            for (int i = 0; i < vector1.Count; i++)
            {
                dotProduct += vector1[i] * vector2[i];
                magnitude1 += vector1[i] * vector1[i];
                magnitude2 += vector2[i] * vector2[i];
            }

            magnitude1 = (float)Math.Sqrt(magnitude1);
            magnitude2 = (float)Math.Sqrt(magnitude2);

            return magnitude1 == 0 || magnitude2 == 0 ? 0f : dotProduct / (magnitude1 * magnitude2);
        }

        private async Task<(string matchedClauseText, float score, string matchedClauseId, string clauseReference)> GetBestMatchClauseAsync(List<float> questionEmbedding, IMongoDatabase mongoDb)
        {
            var clauseCollection = mongoDb.GetCollection<BsonDocument>("LegalDocument");
            float bestScore = -1f;
            string matchedClauseText = null;
            string matchedClauseId = null;
            string clauseReference = null;

            var documents = await clauseCollection.Find(Builders<BsonDocument>.Filter.Empty).ToListAsync();

            foreach (var doc in documents)
            {
                if (!doc.Contains("articles")) continue;

                var articles = doc["articles"].AsBsonArray;
                foreach (var article in articles)
                {
                    var articleDoc = article.AsBsonDocument;

                    // Ưu tiên 1: ĐIỂM
                    if (articleDoc.Contains("clauses"))
                    {
                        var clausesinDoc = articleDoc["clauses"].AsBsonArray;
                        foreach (var clause in clausesinDoc)
                        {
                            var clauseDoc = clause.AsBsonDocument;
                            if (clauseDoc.Contains("points"))
                            {
                                var points = clauseDoc["points"].AsBsonArray;
                                foreach (var point in points)
                                {
                                    var pointDoc = point.AsBsonDocument;
                                    if (pointDoc.Contains("embedding") && pointDoc.Contains("text") && pointDoc.Contains("id"))
                                    {
                                        var embedding = pointDoc["embedding"].AsBsonArray.Select(x => (float)x.AsDouble).ToList();
                                        var score = CosineSimilarity(questionEmbedding, embedding);

                                        if (score > bestScore)
                                        {
                                            bestScore = score;
                                            matchedClauseText = pointDoc["text"].AsString;
                                            matchedClauseId = pointDoc["id"].AsString;
                                            clauseReference = ConstructReference(articleDoc, clauseDoc, pointDoc); // Trích dẫn đầy đủ
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Ưu tiên 2: KHOẢN
                    var clauses = articleDoc.GetValue("clauses", new BsonArray()).AsBsonArray;
                    foreach (var clause in clauses)
                    {
                        var clauseDoc = clause.AsBsonDocument;
                        if (clauseDoc.Contains("embedding") && clauseDoc.Contains("text") && clauseDoc.Contains("id"))
                        {
                            var embedding = clauseDoc["embedding"].AsBsonArray.Select(x => (float)x.AsDouble).ToList();
                            var score = CosineSimilarity(questionEmbedding, embedding);

                            if (score > bestScore)
                            {
                                bestScore = score;
                                matchedClauseText = clauseDoc["text"].AsString;
                                matchedClauseId = clauseDoc["id"].AsString;
                                clauseReference = ConstructReference(articleDoc, clauseDoc); // Trích dẫn đầy đủ
                            }
                        }
                    }

                    // Ưu tiên 3: ĐIỀU
                    if (articleDoc.Contains("embedding") && articleDoc.Contains("title") && articleDoc.Contains("id"))
                    {
                        var embedding = articleDoc["embedding"].AsBsonArray.Select(x => (float)x.AsDouble).ToList();
                        var score = CosineSimilarity(questionEmbedding, embedding);

                        if (score > bestScore)
                        {
                            bestScore = score;
                            matchedClauseText = articleDoc["title"].AsString;
                            matchedClauseId = articleDoc["id"].AsString;
                            clauseReference = ConstructReference(articleDoc); // Trích dẫn chỉ Điều
                        }
                    }
                }
            }

            return (matchedClauseText, bestScore, matchedClauseId, clauseReference ?? "");
        }

        private string ConstructReference(BsonDocument articleDoc, BsonDocument clauseDoc = null, BsonDocument pointDoc = null)
        {
            var referenceParts = new List<string>();

            // Thêm Điều (từ articleDoc)
            if (articleDoc.Contains("type"))
            {
                var title = articleDoc["id"].AsString.Trim();
                if (!string.IsNullOrEmpty(title))
                {
                    referenceParts.Add($"Điều {title}"); // Ví dụ: "Điều 1"
                }
            }

            // Thêm Khoản (từ clauseDoc)
            if (clauseDoc != null && clauseDoc.Contains("id"))
            {
                var clauseText = clauseDoc["id"].AsString.Trim();
                if (!string.IsNullOrEmpty(clauseText))
                {
                    referenceParts.Add($"Khoản {clauseText}"); // Ví dụ: "Khoản 1"
                }
            }

            // Thêm Điểm (từ pointDoc)
            if (pointDoc != null && pointDoc.Contains("id"))
            {
                var pointText = pointDoc["id"].AsString.Trim();
                if (!string.IsNullOrEmpty(pointText))
                {
                    referenceParts.Add($"Điểm {pointText}");
                }
            }

            return string.Join(", ", referenceParts.Where(p => !string.IsNullOrEmpty(p)));
        }


        private List<float> ParseEmbedding(string embeddingJson)
        {
            try
            {
                var wrapper = JsonSerializer.Deserialize<EmbeddingWrapper>(embeddingJson);
                return wrapper?.Result ?? new List<float>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ParseEmbedding] Lỗi khi parse embedding: {ex.Message}");
                return new List<float>();
            }
        }

        private class EmbeddingWrapper
        {
            public List<float> Result { get; set; }
        }

        public Answer GetAnswerById(string answerId)
        {
            return _answerRepository.GetAnswerById(answerId);
        }
    }
}
