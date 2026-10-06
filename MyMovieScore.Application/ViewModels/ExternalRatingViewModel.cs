namespace MyMovieScore.Application.ViewModels
{
    // A rating from another site, as OMDb returns it (e.g. "Rotten Tomatoes", "89%")
    public class ExternalRatingViewModel
    {
        public ExternalRatingViewModel(string source, string value)
        {
            Source = source;
            Value = value;
        }

        public string Source { get; private set; }
        public string Value { get; private set; }
    }
}
