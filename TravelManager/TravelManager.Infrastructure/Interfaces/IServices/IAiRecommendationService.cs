

namespace TravelManager.Infrastructure.Interfaces.IServices
{
    public interface IAiRecommendationService
    {
        Task<AiRecommendationResult?> GetRecommendationsAsync(
        string cityName,
        string? country,
        string language = "uk");
    }


public class AiRecommendationResult
    {
        public string CityName { get; set; } = string.Empty;

        public List<AiPlaceCard> Attractions { get; set; } = new();

        public List<AiPlaceCard> Restaurants { get; set; } = new();

        public List<AiTip> PracticalTips { get; set; } = new();

        public string GeneratedAt { get; set; } =
            DateTime.UtcNow.ToString("o");
    }

    public class AiPlaceCard
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string PriceRange { get; set; } = string.Empty;

        public string Emoji { get; set; } = "📍";

        public string BestTime { get; set; } = string.Empty;
    }

    public class AiTip
    {
        public string Title { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public string Emoji { get; set; } = "💡";
    }


}
