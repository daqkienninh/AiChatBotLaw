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
        private readonly IMongoCollection<LegalChapter> _chapterCollection;
        private readonly IMongoCollection<BsonDocument> _bsonCollection;

        public LawRepository(IMongoClient mongoClient, MongoDbSettings settings)
        {
            _mongoClient = mongoClient;
            _settings = settings;

            var database = _mongoClient.GetDatabase(_settings.DatabaseName);
            _bsonCollection = database.GetCollection<BsonDocument>("localLegalDocument");
            _chapterCollection = database.GetCollection<LegalChapter>("localLegalChapter");
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

        public async Task<SyncResult> SyncTreeAsync(List<LegalChapter> chapters)
        {
            int inserted = 0, updated = 0, skipped = 0;
            var result = new SyncResult();

            foreach (var newChapter in chapters)
            {
                var existingChapter = await _chapterCollection
                    .Find(c => c.Type == "CHUONG" && c.Id == newChapter.Id)
                    .FirstOrDefaultAsync();

                if (existingChapter == null)
                {
                    await _chapterCollection.InsertOneAsync(newChapter);
                    inserted++;
                    result.ChangeLogs.Add($"➕ Thêm mới Chương {newChapter.Id}: \"{newChapter.Title}\"");
                    continue;
                }

                bool isDifferent = false;

                if (newChapter.Title != existingChapter.Title)
                {
                    result.ChangeLogs.Add($"📝 Cập nhật tiêu đề Chương {newChapter.Id}: \"{existingChapter.Title}\" → \"{newChapter.Title}\"");
                    isDifferent = true;
                }

                if (isDifferent && newChapter.Clauses?.Count == existingChapter.Clauses?.Count)
                {
                    for (int i = 0; i < newChapter.Clauses.Count; i++)
                    {
                        var newClause = newChapter.Clauses[i];
                        var existingClause = existingChapter.Clauses[i];

                        if (newClause.Id != existingClause.Id || newClause.Title != existingClause.Title)
                        {
                            result.ChangeLogs.Add($"📝 Cập nhật Điều {existingClause.Id} trong Chương {newChapter.Id}: \"{existingClause.Title}\" → \"{newClause.Title}\"");
                            isDifferent = true;
                            break;
                        }

                        if (newClause.ClauseItems?.Count != existingClause.ClauseItems?.Count)
                        {
                            result.ChangeLogs.Add($"⚠️ Số khoản thay đổi trong Điều {newClause.Id} của Chương {newChapter.Id}");
                            isDifferent = true;
                            break;
                        }

                        for (int j = 0; j < newClause.ClauseItems.Count; j++)
                        {
                            var newItem = newClause.ClauseItems[j];
                            var existingItem = existingClause.ClauseItems[j];

                            if (newItem.Id != existingItem.Id || newItem.Text != existingItem.Text)
                            {
                                result.ChangeLogs.Add($"📝 Cập nhật Khoản {existingItem.Id} trong Điều {existingClause.Id} (Chương {newChapter.Id})");
                                isDifferent = true;
                                break;
                            }

                            if (newItem.Points?.Count != existingItem.Points?.Count)
                            {
                                result.ChangeLogs.Add($"⚠️ Số điểm thay đổi trong Khoản {newItem.Id} (Điều {existingClause.Id}, Chương {newChapter.Id})");
                                isDifferent = true;
                                break;
                            }

                            for (int k = 0; k < newItem.Points.Count; k++)
                            {
                                var newPoint = newItem.Points[k];
                                var existingPoint = existingItem.Points[k];

                                if (newPoint.Id != existingPoint.Id || newPoint.Text != existingPoint.Text)
                                {
                                    result.ChangeLogs.Add($"📝 Cập nhật Điểm {existingPoint.Id} trong Khoản {existingItem.Id} (Điều {existingClause.Id}, Chương {newChapter.Id})");
                                    isDifferent = true;
                                    break;
                                }
                            }

                            if (isDifferent) break;
                        }

                        if (isDifferent) break;
                    }
                }
                else if (!isDifferent)
                {
                    result.ChangeLogs.Add($"⚠️ Số lượng Điều thay đổi trong Chương {newChapter.Id}");
                    isDifferent = true;
                }

                if (isDifferent)
                {
                    var update = Builders<LegalChapter>.Update
                        .Set(c => c.Clauses, newChapter.Clauses)
                        .Set(c => c.Title, newChapter.Title);

                    await _chapterCollection.UpdateOneAsync(c => c.Id == newChapter.Id, update);
                    updated++;
                }
                else
                {
                    skipped++;
                }
            }

            result.Inserted = inserted;
            result.Updated = updated;
            result.Skipped = skipped;

            result.ChangeLogs.Add($"✅ Hoàn tất: {inserted} chương mới, {updated} chương cập nhật, {skipped} chương giữ nguyên");

            return result;
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
