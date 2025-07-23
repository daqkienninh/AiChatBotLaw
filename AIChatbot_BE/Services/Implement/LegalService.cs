using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Repositories;
using Repositories.DBContext;
using Repositories.Models;
using Services.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using static Repositories.LawRepository;
using static UglyToad.PdfPig.Core.PdfSubpath;


namespace Services.Implement
{
    public class LegalService : ILegalService
    {
        private readonly IMongoClient _mongoClient;
        private readonly MongoDbSettings _settings;
        private readonly LawRepository _lawRepository;
        private readonly IMongoCollection<LegalChapter> _chapterCollection;
        private readonly IEmbeddingService _embeddingService;

        public LegalService(IMongoClient mongoClient, IOptions<MongoDbSettings> settings, LawRepository lawRepository, IEmbeddingService embeddingService)
        {
            _mongoClient = mongoClient;
            _settings = settings.Value;
            _lawRepository = lawRepository;
            var database = _mongoClient.GetDatabase(_settings.DatabaseName);
            _chapterCollection = database.GetCollection<LegalChapter>("localLegalChapter");
            _embeddingService = embeddingService;
        }

        public Task<List<LegalChapter>> GetAllLegalChapter()
        {
            return _lawRepository.GetAllLegalChapter();
        }

        public async Task<SyncResult> ProcessPdfAsync(IFormFile file)
        {
            // 1. Đọc text
            var rawText = await ExtractTextAsync(file);

            // 2. Parse sang các dòng .raw
            var rawLines = SplitTextToRawFile(rawText);

            // 3. Chuyển sang cây JSON
            var tree = ParseRawLinesToTree(rawLines);

            if (Directory.Exists("outputs"))
            {
                Directory.Delete("outputs", true); // true = xóa đệ quy (bao gồm cả file và folder con)
            }
            // 4. Gửi vào MongoDB
            return await SyncTreeAsync(tree);
        }

        public static async Task<string> ExtractTextAsync(IFormFile file)
        {
            // Tạo thư mục outputs nếu chưa có
            var outputFolder = Path.Combine(Directory.GetCurrentDirectory(), "outputs");
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            // Tạo file PDF tạm trong outputs
            var tempPdfPath = Path.Combine(outputFolder, Path.GetRandomFileName() + ".pdf");

            await using (var stream = new FileStream(tempPdfPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Đường dẫn tới file Python
            var pythonScript = Path.Combine(Directory.GetCurrentDirectory(), "PythonScripts", "parse_law.py");

            // Chuẩn bị process để gọi Python
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "python", // hoặc "python3" tùy máy
                Arguments = $"\"{pythonScript}\" \"{tempPdfPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // Chạy process
            string output, error;
            using (var process = new System.Diagnostics.Process())
            {
                process.StartInfo = psi;
                process.Start();

                output = await process.StandardOutput.ReadToEndAsync();
                error = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();
            }

            if (!string.IsNullOrEmpty(error))
            {
                throw new Exception($"Python error: {error}");
            }

            // output sẽ in ra đường dẫn file txt từ script Python
            var txtFilePath = output.Trim();
            return txtFilePath;
        }
        public static string SplitTextToRawFile(string filePath)
        {
            var text = File.ReadAllText(filePath);

            var lines = new List<string>();
            var chuongPattern = new Regex(@"Chương\s+([IVXLC]+)[\.:]?\s*(.*)", RegexOptions.IgnoreCase);
            var dieuPattern = new Regex(@"Điều\s+(\d+)[\.:]?\s*(.*)");
            var khoanPattern = new Regex(@"^\s*(\d+)[\.:]\s*(.*)");
            var diemPattern = new Regex(@"^\s*([a-z])[\)\.]\s*(.*)");

            string? currentChuong = null;
            string? currentDieu = null;
            string? currentKhoan = null;
            string? currentDiem = null;

            foreach (var line in text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmed))
                {
                    if (lines.Count > 0 && !string.IsNullOrEmpty(lines[^1]))
                    {
                        lines[^1] += " ";
                    }
                    continue;
                }

                var m = chuongPattern.Match(trimmed);
                if (m.Success)
                {
                    currentChuong = m.Groups[1].Value;
                    lines.Add($"CHUONG|{currentChuong}|{m.Groups[2].Value.Trim()}");
                    continue;
                }

                m = dieuPattern.Match(trimmed);
                if (m.Success)
                {
                    currentDieu = m.Groups[1].Value;
                    lines.Add($"DIEU|{currentDieu}|{m.Groups[2].Value.Trim()}");
                    continue;
                }

                m = khoanPattern.Match(trimmed);
                if (m.Success && currentDieu != null)
                {
                    currentKhoan = m.Groups[1].Value;
                    lines.Add($"KHOAN|{currentDieu}.{currentKhoan}|{m.Groups[2].Value.Trim()}");
                    continue;
                }

                m = diemPattern.Match(trimmed);
                if (m.Success && currentDieu != null && currentKhoan != null)
                {
                    currentDiem = m.Groups[1].Value;
                    lines.Add($"DIEM|{currentDieu}.{currentKhoan}.{currentDiem}|{m.Groups[2].Value.Trim()}");
                    continue;
                }

                if (lines.Count > 0)
                {
                    lines[^1] += " " + trimmed;
                }
            }

            var outputPath = Path.ChangeExtension(filePath, ".raw");
            File.WriteAllLines(outputPath, lines);
            return outputPath;
        }



        public static List<LegalChapter> ParseRawLinesToTree(string filePath)
        {
            var rawLines = File.ReadAllLines(filePath).ToList();
            var tree = new List<LegalChapter>();
            LegalChapter? currentChuong = null;
            LegalClause? currentDieu = null;
            LegalClauseItem? currentKhoan = null;

            foreach (var line in rawLines)
            {
                var parts = line.Split('|');
                if (parts.Length < 3) continue;

                var type = parts[0];
                var id = parts[1];
                var content = parts[2];

                switch (type)
                {
                    case "CHUONG":
                        currentChuong = new LegalChapter { Type = "CHUONG", Id = id, Title = content, Clauses = new() };
                        tree.Add(currentChuong);
                        break;

                    case "DIEU":
                        currentDieu = new LegalClause { Type = "DIEU", Id = id, Title = content, ClauseItems = new() };
                        currentChuong?.Clauses.Add(currentDieu);
                        break;

                    case "KHOAN":
                        currentKhoan = new LegalClauseItem { Id = id.Split('.')[1], Text = content, Points = new() };
                        currentDieu?.ClauseItems.Add(currentKhoan);
                        break;

                    case "DIEM":
                        var point = new LegalPoint { Id = id.Split('.').Last(), Text = content };
                        currentKhoan?.Points.Add(point);
                        break;
                }
            }

            return tree;
        }

        public Task<bool> UpdateClauseAsync(string chapterId, string clauseId, string? newClauseText = null, List<LegalClauseItem>? newClauseItems = null, List<LegalPoint>? newPoints = null)
        {
            return _lawRepository.UpdateClauseAsync(chapterId, clauseId, newClauseText, newClauseItems, newPoints); 
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
                    newChapter.Embedding = await _embeddingService.GenerateEmbeddingAsync(newChapter.Title);

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
    }
}
