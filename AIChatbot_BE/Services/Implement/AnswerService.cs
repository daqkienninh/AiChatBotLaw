using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
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

namespace Services.Implement
{
    public class AnswerService : IAnswerService
    {
        private readonly AnswerRepository _answerRepository;
        private readonly MongoClient _mongoClient;
        private readonly MongoDbSettings _settings;
        public AnswerService(IOptions<MongoDbSettings> settings) 
        { 
            _answerRepository = new AnswerRepository();
            _settings = settings.Value;
            _mongoClient = new MongoClient(_settings.ConnectionString);
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
            var (matchedClauseText, score, matchedClauseId) = await GetBestMatchClauseAsync(questionEmbedding, mongoDb);

            const float threshold = 0.3f;

            string generatedAnswer;
            if (string.IsNullOrWhiteSpace(matchedClauseText) || score < threshold)
            {
                generatedAnswer = "Hiện tại hệ thống chưa tìm được quy định pháp luật phù hợp với câu hỏi.";
            }
            else
            {
                generatedAnswer = $"Theo quy định pháp luật: {matchedClauseText}";
            }
            // Ghi vào SQL
            var answer = new Answer
            {
                AnswerId = question.QuestionId,
                QuestionId = question.QuestionId,
                AnsContent = generatedAnswer,
                LegalclauseId = matchedClauseId,
                AnsCreateAt = DateTime.Now
            };

            _answerRepository.AddAnswer(answer);
        }

        private async Task<(string bestText, float bestScore, string bestId)> GetBestMatchClauseAsync(List<float> questionEmbedding, IMongoDatabase mongoDb)
        {
            var clauseCollection = mongoDb.GetCollection<BsonDocument>("LegalDocument");
            float bestScore = -1f;
            string bestText = "";
            string bestId = "";

            var documents = await clauseCollection.Find(Builders<BsonDocument>.Filter.Empty).ToListAsync();

            foreach (var doc in documents)
            {
                if (!doc.Contains("articles")) continue;

                foreach (var article in doc["articles"].AsBsonArray)
                {
                    var articleDoc = article.AsBsonDocument;

                    // === Ưu tiên 1: ĐIỂM ===
                    if (articleDoc.Contains("clauses"))
                    {
                        foreach (var clause in articleDoc["clauses"].AsBsonArray)
                        {
                            var clauseDoc = clause.AsBsonDocument;

                            if (clauseDoc.Contains("points"))
                            {
                                foreach (var point in clauseDoc["points"].AsBsonArray)
                                {
                                    var pointDoc = point.AsBsonDocument;
                                    if (pointDoc.Contains("embedding") && pointDoc.Contains("text") && pointDoc.Contains("id"))
                                    {
                                        var embedding = pointDoc["embedding"].AsBsonArray.Select(x => (float)x.AsDouble).ToList();
                                        var score = CosineSimilarity(questionEmbedding, embedding);

                                        if (score > bestScore)
                                        {
                                            bestScore = score;
                                            bestText = pointDoc["text"].AsString;
                                            bestId = pointDoc["id"].AsString;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // === Ưu tiên 2: KHOẢN ===
                    foreach (var clause in articleDoc.GetValue("clauses", new BsonArray()).AsBsonArray)
                    {
                        var clauseDoc = clause.AsBsonDocument;
                        if (clauseDoc.Contains("embedding") && clauseDoc.Contains("text") && clauseDoc.Contains("id"))
                        {
                            var embedding = clauseDoc["embedding"].AsBsonArray.Select(x => (float)x.AsDouble).ToList();
                            var score = CosineSimilarity(questionEmbedding, embedding);

                            if (score > bestScore)
                            {
                                bestScore = score;
                                bestText = clauseDoc["text"].AsString;
                                bestId = clauseDoc["id"].AsString;
                            }
                        }
                    }

                    // === Ưu tiên 3: ĐIỀU ===
                    if (articleDoc.Contains("embedding") && articleDoc.Contains("title") && articleDoc.Contains("id"))
                    {
                        var embedding = articleDoc["embedding"].AsBsonArray.Select(x => (float)x.AsDouble).ToList();
                        var score = CosineSimilarity(questionEmbedding, embedding);

                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestText = articleDoc["title"].AsString;
                            bestId = articleDoc["id"].AsString;
                        }
                    }
                }
            }

            return (bestText, bestScore, bestId);
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



        private float CosineSimilarity(List<float> a, List<float> b)
        {
            if (a == null || b == null || a.Count == 0 || b.Count == 0)
                return -1f;

            if (a.Count != b.Count)
            {
                Console.WriteLine($"⚠️ Length mismatch: a = {a.Count}, b = {b.Count}");
                return -1f; // hoặc return 0f nếu muốn bỏ qua
            }

            float dot = 0f, normA = 0f, normB = 0f;

            for (int i = 0; i < a.Count; i++)
            {
                dot += a[i] * b[i];
                normA += a[i] * a[i];
                normB += b[i] * b[i];
            }

            return (float)(dot / (Math.Sqrt(normA) * Math.Sqrt(normB) + 1e-10)); // Tránh chia 0
        }

        public Answer GetAnswerById(string answerId)
        {
            return _answerRepository.GetAnswerById(answerId);
        }
    }
}
