namespace MyMovieScore.Infrastructure.ExternalServices
{
    // OMDb API settings
    public class ExternalServiceOptions
    {
        public const string SectionName = "ExternalService";

        public string BaseUrl { get; set; } = "https://www.omdbapi.com";
        public string Key { get; set; } = string.Empty;
        // "short" or "full"
        public string Plot { get; set; } = "full";
    }
}
