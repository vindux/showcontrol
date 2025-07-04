using Newtonsoft.Json;

namespace ShowControl.Models
{
    public class ShowData
    {
        [JsonProperty("controlSettings")] 
        public required ControlSettings ControlSettings { get; set; }

        [JsonProperty("event")] 
        public required string Event { get; set; }

        [JsonProperty("custom")] 
        public required List<CustomButton> Custom { get; set; }

        [JsonProperty("content")] 
        public required List<Chapter> Content { get; set; }
    }

    public class ControlSettings
    {
        [JsonProperty("buttonsPerRow")] 
        public int? ButtonsPerRow { get; set; }
    }

    public class CustomButton
    {
        [JsonProperty("thumbnail")] 
        public required string Thumbnail { get; set; }

        [JsonProperty("title")] 
        public required string Title { get; set; }

        [JsonProperty("templateData")] 
        public required object TemplateData { get; set; }
    }

    public class Chapter
    {
        [JsonProperty("chapter")] 
        public required string ChapterName { get; set; }

        [JsonProperty("slides")] 
        public required List<Slide> Slides { get; set; }
    }

    public class Slide
    {
        [JsonProperty("thumbnail")] 
        public required string Thumbnail { get; set; }
        
        [JsonProperty("title")] 
        public required string Title { get; set; }

        [JsonProperty("templateData")] 
        public required object TemplateData { get; set; }
    }
}