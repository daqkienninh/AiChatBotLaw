using MongoDB.Bson;
using MongoDB.Driver;
using Repositories.DBContext;
using Repositories.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Repositories
{
    public class LawRepository
    {
        private readonly IMongoClient _mongoClient;
        private readonly MongoDbSettings _settings;
        private readonly IMongoCollection<LegalDocument> _chapterCollection;
        private readonly IMongoCollection<BsonDocument> _bsonCollection;

        public LawRepository(IMongoClient mongoClient, MongoDbSettings settings)
        {
            _mongoClient = mongoClient;
            _settings = settings;

            var database = _mongoClient.GetDatabase(_settings.DatabaseName);
            _chapterCollection = database.GetCollection<LegalDocument>("localLegalDocument");
            _bsonCollection = database.GetCollection<BsonDocument>("localLegalDocument");
        }

        /// <summary>
        /// Update Point Text by ChapterId, ClauseId, ClauseItemId and PointId
        /// </summary>
        /// <param name="chapterId"></param>
        /// <param name="clauseId"></param>
        /// <param name="clauseItemId"></param>
        /// <param name="pointId"></param>
        /// <param name="newText"></param>
        /// <returns></returns>
        public async Task<bool> UpdatePointTextAsync(string chapterId, string clauseId, string clauseItemId, string pointId, string newText)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("id", chapterId);

            var update = Builders<BsonDocument>.Update
                .Set("articles.$[cl].clauseItems.$[ci].points.$[pt].text", newText);

            var options = new UpdateOptions
            {
                ArrayFilters = new List<ArrayFilterDefinition<BsonDocument>>
                {
                    new JsonArrayFilterDefinition<BsonDocument>("{ 'cl.id': '" + clauseId + "' }"),
                    new JsonArrayFilterDefinition<BsonDocument>("{ 'ci.id': '" + clauseItemId + "' }"),
                    new JsonArrayFilterDefinition<BsonDocument>("{ 'pt.id': '" + pointId + "' }")
                }
            };

            var result = await _bsonCollection.UpdateOneAsync(filter, update, options);
            return result.ModifiedCount > 0;
        }

        /// <summary>
        ///  Update Clause Text by ChapterId, ClauseId and ClauseItemId
        /// </summary>
        /// <param name="chapterId"></param>
        /// <param name="clauseId"></param>
        /// <param name="clauseItemId"></param>
        /// <param name="newText"></param>
        /// <returns></returns>
        public async Task<bool> UpdateClauseItemTextAsync(string chapterId, string clauseId, string clauseItemId, string newText)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("id", chapterId);

            var update = Builders<BsonDocument>.Update
                .Set("articles.$[cl].clauseItems.$[ci].text", newText);

            var options = new UpdateOptions
            {
                ArrayFilters = new List<ArrayFilterDefinition<BsonDocument>>
                {
                    new JsonArrayFilterDefinition<BsonDocument>("{ 'cl.id': '" + clauseId + "' }"),
                    new JsonArrayFilterDefinition<BsonDocument>("{ 'ci.id': '" + clauseItemId + "' }")
                }
            };

            var result = await _bsonCollection.UpdateOneAsync(filter, update, options);
            return result.ModifiedCount > 0;
        }

        /// <summary>
        /// Update Articles by ChapterId and ClauseId
        /// </summary>
        /// <param name="chapterId"></param>
        /// <param name="clauseId"></param>
        /// <param name="newTitle"></param>
        /// <returns></returns>
        public async Task<bool> UpdateClauseTitleAsync(string chapterId, string clauseId, string newTitle)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("id", chapterId);

            var update = Builders<BsonDocument>.Update
                .Set("articles.$[art].title", newTitle);

            var options = new UpdateOptions
            {
                ArrayFilters = new List<ArrayFilterDefinition<BsonDocument>>
                {
                    new JsonArrayFilterDefinition<BsonDocument>("{ 'art.id': '" + clauseId + "' }")
                }
            };

            var result = await _bsonCollection.UpdateOneAsync(filter, update, options);
            return result.ModifiedCount > 0;
        }
        /// <summary>
        /// Dictionary Clause by ChapterId
        /// </summary>
        /// <param name="chapterId"></param>
        /// <returns></returns>
        public async Task<List<Dictionary<string, string>>> GetLegalClauseByChapterIdAsync(string chapterId)
        {
            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("type", "CHAPTER"),
                Builders<BsonDocument>.Filter.Eq("id", chapterId)
            );

            var projection = Builders<BsonDocument>.Projection.Include("articles").Exclude("_id");

            var result = await _bsonCollection.Find(filter).Project(projection).FirstOrDefaultAsync();

            var clauseList = new List<Dictionary<string, string>>();

            if (result != null && result.Contains("articles"))
            {
                foreach (var article in result["articles"].AsBsonArray)
                {
                    var articleDoc = article.AsBsonDocument;

                    var dict = new Dictionary<string, string>
                    {
                        { "id", articleDoc.GetValue("id", "").AsString },
                        { "title", articleDoc.GetValue("title", "").AsString }
                    };

                    clauseList.Add(dict);
                }
            }

            return clauseList;
        }

        /// <summary>
        /// Get all legal document
        /// </summary>
        /// <returns></returns>
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
                            Type = articleDoc.GetValue("type", "").AsString,
                            ClauseItems = new List<LegalClauseItem>()
                        };

                        // Nếu có clauseItems trong điều
                        if (articleDoc.TryGetValue("clauses", out var clauseItemsBson) && clauseItemsBson.IsBsonArray)
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

        /// <summary>
        /// Create Chapter
        /// </summary>
        /// <param name="newChapter"></param>
        /// <returns></returns>
        public async Task<bool> CreateChapterAsync(LegalChapter legalChapter)
        {
            var bsonDoc = legalChapter.ToBsonDocument(); // Chuyển từ object sang BsonDocument
            await _bsonCollection.InsertOneAsync(bsonDoc);
            return true;
        }
        /// <summary>
        /// Add Clase to Article
        /// </summary>
        /// <param name="chapterId"></param>
        /// <param name="clauseId"></param>
        /// <param name="newClauseItem"></param>
        /// <returns></returns>
        public async Task<bool> AddClauseItemAsync(string chapterId, string clauseId, LegalClauseItem newClauseItem)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("id", chapterId);

            var update = Builders<BsonDocument>.Update.Push("articles.$[art].clauseItems", newClauseItem);

            var options = new UpdateOptions
            {
                ArrayFilters = new List<ArrayFilterDefinition<BsonDocument>>
        {
            new JsonArrayFilterDefinition<BsonDocument>("{ 'art.id': '" + clauseId + "' }")
        }
            };

            var result = await _bsonCollection.UpdateOneAsync(filter, update, options);
            return result.ModifiedCount > 0;
        }
        /// <summary>
        /// Add Point to Clause
        /// </summary>
        /// <param name="chapterId"></param>
        /// <param name="clauseId"></param>
        /// <param name="clauseItemId"></param>
        /// <param name="newPoint"></param>
        /// <returns></returns>
        public async Task<bool> AddPointAsync(string chapterId, string clauseId, string clauseItemId, LegalPoint newPoint)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("id", chapterId);

            var update = Builders<BsonDocument>.Update.Push("articles.$[art].clauseItems.$[ci].points", newPoint);

            var options = new UpdateOptions
            {
                ArrayFilters = new List<ArrayFilterDefinition<BsonDocument>>
                {
                    new JsonArrayFilterDefinition<BsonDocument>("{ 'art.id': '" + clauseId + "' }"),
                    new JsonArrayFilterDefinition<BsonDocument>("{ 'ci.id': '" + clauseItemId + "' }")
                }
            };

            var result = await _bsonCollection.UpdateOneAsync(filter, update, options);
            return result.ModifiedCount > 0;
        }

        /// <summary>
        /// Add Article to Chapter
        /// </summary>
        /// <param name="chapterId"></param>
        /// <param name="newClause"></param>
        /// <returns></returns>
        public async Task<bool> AddClauseToChapterAsync(string chapterId, LegalClause newClause)
        {
            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("type", "CHUONG"),
                Builders<BsonDocument>.Filter.Eq("id", chapterId)
            );

            var newClauseBson = new BsonDocument
            {
                { "id", newClause.Id },
                { "title", newClause.Title },
                { "clause", new BsonArray(newClause.ClauseItems?.Select(item => new BsonDocument
                    {
                        { "id", item.Id },
                        { "text", item.Text },
                        { "points", new BsonArray(item.Points?.Select(p => new BsonDocument
                            {
                                { "id", p.Id },
                                { "text", p.Text }
                            }) ?? new List<BsonDocument>()) 
                        }
                    }) ?? new List<BsonDocument>()) 
                }
            };

            var update = Builders<BsonDocument>.Update.Push("articles", newClauseBson);

            var result = await _bsonCollection.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0;
        }


    }
}
