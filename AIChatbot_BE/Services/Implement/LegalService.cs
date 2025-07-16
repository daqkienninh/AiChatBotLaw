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
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Services.Implement
{
    public class LegalService : ILegalService
    {
        private readonly IMongoClient _mongoClient;
        private readonly MongoDbSettings _settings;
        private readonly LawRepository _lawRepository;

        public LegalService(IMongoClient mongoClient, IOptions<MongoDbSettings> settings, LawRepository lawRepository)
        {
            _mongoClient = mongoClient;
            _settings = settings.Value;
            _lawRepository = lawRepository;
        }
        public async Task<bool> ProcessPdfAsync(string filePath)
        {
            var scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "PythonScripts", "parse_law.py");

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"\"{scriptPath}\" \"{filePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // ✅ Thêm biến môi trường an toàn
            processStartInfo.EnvironmentVariables["MONGO_URI"] = _settings.ConnectionString;

            using var process = new Process { StartInfo = processStartInfo };

            try
            {
                process.Start();
                // ✅ Đọc output và error song song (không chặn)
                Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
                Task<string> errorTask = process.StandardError.ReadToEndAsync();

                // ✅ Đợi process kết thúc
                await process.WaitForExitAsync(); // .NET 6+

                string output = await outputTask;
                string error = await errorTask;

                if (process.ExitCode == 0)
                {
                    Console.WriteLine("✅ Python xử lý thành công");
                    Console.WriteLine("Output:\n" + output);
                    return true;
                }
                else
                {
                    Console.WriteLine($"❌ Python xử lý thất bại (ExitCode = {process.ExitCode})");
                    Console.WriteLine("Output:\n" + output);
                    Console.WriteLine("Error:\n" + error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception khi chạy script: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateClauseTitleAsync(string chapterId, string clauseId, string newTitle)
        {
            return await _lawRepository.UpdateClauseTitleAsync(chapterId, clauseId, newTitle);
        }

        public async Task<List<LegalChapter>> GetAllLegalChapter()
        {
            try
            {
                return await _lawRepository.GetAllLegalChapter();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy danh sách chương luật: {ex.Message}");
                return new List<LegalChapter>();
            }
        }
    }
}
