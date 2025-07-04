using System.IO;
using Newtonsoft.Json;
using ShowControl.Models;

namespace ShowControl.Services
{
    public class JsonService
    {
        public ShowData LoadShowData(string jsonPath)
        {
            if (string.IsNullOrEmpty(jsonPath))
            {
                throw new ArgumentException("JSON file path is required");
            }

            if (!File.Exists(jsonPath))
            {
                throw new FileNotFoundException($"JSON file not found: {jsonPath}");
            }

            string json = File.ReadAllText(jsonPath);
            
            if (!IsValidJson(json))
            {
                throw new JsonException("Invalid JSON file format");
            }

            ShowData? showData = JsonConvert.DeserializeObject<ShowData>(json);
            if (showData == null)
            {
                throw new JsonException("Invalid JSON file format");
            }

            return showData;
        }

        private bool IsValidJson(string jsonContent)
        {
            try
            {
                ShowData? testDeserialize = JsonConvert.DeserializeObject<ShowData>(jsonContent);
                return testDeserialize != null;
            }
            catch
            {
                return false;
            }
        }

        public string SerializeTemplateData(object templateData)
        {
            return JsonConvert.SerializeObject(templateData, Formatting.None);
        }
    }
}