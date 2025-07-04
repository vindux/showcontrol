using System.IO;
using Newtonsoft.Json;
using ShowControl.Models;

namespace ShowControl.Services
{
    /// <summary>
    /// Service for JSON file operations and data serialization
    /// </summary>
    public class JsonService
    {
        /// <summary>
        /// Loads and deserializes show data from a JSON file
        /// </summary>
        /// <param name="jsonPath">The full path to the JSON file</param>
        /// <returns>Deserialized ShowData object</returns>
        /// <exception cref="ArgumentException">Thrown when jsonPath is null or empty</exception>
        /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist</exception>
        /// <exception cref="JsonException">Thrown when the JSON format is invalid</exception>
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

        /// <summary>
        /// Validates whether a JSON string can be properly deserialized to ShowData
        /// </summary>
        /// <param name="jsonContent">The JSON string to validate</param>
        /// <returns>True if the JSON is valid and can be deserialized, false otherwise</returns>
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

        /// <summary>
        /// Serializes template data object to a compact JSON string
        /// </summary>
        /// <param name="templateData">The template data object to serialize</param>
        /// <returns>Compact JSON string representation of the template data</returns>
        public string SerializeTemplateData(object templateData)
        {
            return JsonConvert.SerializeObject(templateData, Formatting.None);
        }
    }
}