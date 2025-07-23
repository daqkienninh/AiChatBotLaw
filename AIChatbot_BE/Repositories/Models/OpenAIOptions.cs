using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Models
{
    public class OpenAIOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string EmbeddingModel { get; set; } = "text-embedding-ada-002";
        public string ChatModel { get; set; } = "ft:gpt-4o-mini:your-finetuned-model-id";
    }
}
