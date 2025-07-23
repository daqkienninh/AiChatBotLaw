using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Repositories.DBContext;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;



namespace Repositories
{
    public class LawRepository
    {
        private readonly IMongoClient _mongoClient;
        private readonly MongoDbSettings _settings;
        private readonly IMongoCollection<BsonDocument> _bsonCollection;

        public LawRepository(IMongoClient mongoClient, MongoDbSettings settings)
        {
            _mongoClient = mongoClient;
            _settings = settings;

            var database = _mongoClient.GetDatabase(_settings.DatabaseName);
            _bsonCollection = database.GetCollection<BsonDocument>("localLegalDocument");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chapterId"></param>
        /// <param name="clauseId"></param>
        /// <param name="newClauseText"></param>
        /// <param name="newClauseItems"></param>
        /// <param name="newPoints"></param>
        /// <returns></returns>
        public async Task<bool> UpdateClauseAsync(
            string chapterId,
            string clauseId,
            string? newClauseText = null,
            List<LegalClauseItem>? newClauseItems = null,
            List<LegalPoint>? newPoints = null)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("id", chapterId);

            var updates = new List<UpdateDefinition<BsonDocument>>();

            if (!string.IsNullOrEmpty(newClauseText))
            {
                updates.Add(Builders<BsonDocument>.Update.Set("articles.$[cl].text", newClauseText));
            }

            if (newClauseItems != null)
            {
                updates.Add(Builders<BsonDocument>.Update.Set("articles.$[cl].clauseItems", newClauseItems));
            }

            if (newPoints != null)
            {
                // Cập nhật điểm cho tất cả các khoản (nếu cần cụ thể từng khoản thì cần chỉ định ci.id)
                updates.Add(Builders<BsonDocument>.Update.Set("articles.$[cl].clauseItems.$[].points", newPoints));
            }

            if (updates.Count == 0)
                return false; // Không có gì để cập nhật

            var updateDef = Builders<BsonDocument>.Update.Combine(updates);

            var options = new UpdateOptions
            {
                ArrayFilters = new List<ArrayFilterDefinition<BsonDocument>>
        {
            new JsonArrayFilterDefinition<BsonDocument>("{ 'cl.id': '" + clauseId + "' }")
        }
            };

            var result = await _bsonCollection.UpdateOneAsync(filter, updateDef, options);
            return result.ModifiedCount > 0;
        }

        public async Task<List<LegalChapter>> GetAllLegalChapter()
        {
            var filter = Builders<BsonDocument>.Filter.Eq("type", "CHUONG");
            var chaptersBson = await _bsonCollection.Find(filter).ToListAsync();

            var chapters = new List<LegalChapter>();

            foreach (var doc in chaptersBson)
            {
                var chapter = new LegalChapter
                {
                    Id = doc.GetValue("id", "").AsString,
                    Title = doc.GetValue("title", "").AsString,
                    Clauses = new List<LegalClause>()
                };

                // Kiểm tra nếu có "articles" (tức là các Điều)
                if (doc.TryGetValue("articles", out var articlesBson) && articlesBson.IsBsonArray)
                {
                    foreach (var articleBson in articlesBson.AsBsonArray)
                    {
                        var articleDoc = articleBson.AsBsonDocument;

                        var clause = new LegalClause
                        {
                            Id = articleDoc.GetValue("id", "").AsString,
                            Title = articleDoc.GetValue("title", "").AsString,
                            ClauseItems = new List<LegalClauseItem>()
                        };

                        // Nếu có clauseItems trong điều
                        if (articleDoc.TryGetValue("clauseItems", out var clauseItemsBson) && clauseItemsBson.IsBsonArray)
                        {
                            foreach (var itemBson in clauseItemsBson.AsBsonArray)
                            {
                                var itemDoc = itemBson.AsBsonDocument;

                                var clauseItem = new LegalClauseItem
                                {
                                    Id = itemDoc.GetValue("id", "").AsString,
                                    Text = itemDoc.GetValue("text", "").AsString,
                                    Points = new List<LegalPoint>()
                                };

                                // Nếu có points trong clauseItem
                                if (itemDoc.TryGetValue("points", out var pointsBson) && pointsBson.IsBsonArray)
                                {
                                    foreach (var pointBson in pointsBson.AsBsonArray)
                                    {
                                        var pointDoc = pointBson.AsBsonDocument;

                                        clauseItem.Points.Add(new LegalPoint
                                        {
                                            Id = pointDoc.GetValue("id", "").AsString,
                                            Text = pointDoc.GetValue("text", "").AsString
                                        });
                                    }
                                }

                                clause.ClauseItems.Add(clauseItem);
                            }
                        }

                        chapter.Clauses.Add(clause);
                    }
                }

                chapters.Add(chapter);
            }

            return chapters;
        }





        public class SyncResult
        {
            public int Inserted { get; set; }
            public int Updated { get; set; }
            public int Skipped { get; set; }
            public List<string> ChangeLogs { get; set; } = new();
        }
    }
}
