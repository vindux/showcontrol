using Newtonsoft.Json;

namespace ShowControl.Models
{
    /// <summary>
    /// Root data model representing the complete show configuration
    /// </summary>
    public class ShowData
    {
        /// <summary>
        /// Control settings that affect the application's behavior and appearance
        /// </summary>
        [JsonProperty("controlSettings")] 
        public required ControlSettings ControlSettings { get; set; }

        /// <summary>
        /// The name or title of the event/show
        /// </summary>
        [JsonProperty("event")] 
        public required string Event { get; set; }

        /// <summary>
        /// List of custom buttons displayed in the footer area
        /// </summary>
        [JsonProperty("custom")] 
        public required List<CustomButton> Custom { get; set; }

        /// <summary>
        /// List of chapters containing the main show content and slides
        /// </summary>
        [JsonProperty("content")] 
        public required List<Chapter> Content { get; set; }
    }

    /// <summary>
    /// Configuration settings that control the application's UI behavior
    /// </summary>
    public class ControlSettings
    {
        /// <summary>
        /// Number of buttons to display per row in the main content area (optional)
        /// </summary>
        [JsonProperty("buttonsPerRow")] 
        public int? ButtonsPerRow { get; set; }
    }

    /// <summary>
    /// Represents a custom button displayed in the footer area
    /// </summary>
    public class CustomButton
    {
        /// <summary>
        /// Path to the thumbnail image for this custom button
        /// </summary>
        [JsonProperty("thumbnail")] 
        public required string Thumbnail { get; set; }

        /// <summary>
        /// Display title text for this custom button
        /// </summary>
        [JsonProperty("title")] 
        public required string Title { get; set; }

        /// <summary>
        /// Template data object sent via OSC when this button is clicked
        /// </summary>
        [JsonProperty("templateData")] 
        public required object TemplateData { get; set; }
    }

    /// <summary>
    /// Represents a chapter or section containing related slides
    /// </summary>
    public class Chapter
    {
        /// <summary>
        /// The display name of this chapter
        /// </summary>
        [JsonProperty("chapter")] 
        public required string ChapterName { get; set; }

        /// <summary>
        /// List of slides contained within this chapter
        /// </summary>
        [JsonProperty("slides")] 
        public required List<Slide> Slides { get; set; }
    }

    /// <summary>
    /// Represents an individual slide with its associated data
    /// </summary>
    public class Slide
    {
        /// <summary>
        /// Path to the thumbnail image for this slide
        /// </summary>
        [JsonProperty("thumbnail")] 
        public required string Thumbnail { get; set; }
        
        /// <summary>
        /// Display title text for this slide
        /// </summary>
        [JsonProperty("title")] 
        public required string Title { get; set; }

        /// <summary>
        /// Template data object sent via OSC when this slide is clicked
        /// </summary>
        [JsonProperty("templateData")] 
        public required object TemplateData { get; set; }
    }
}