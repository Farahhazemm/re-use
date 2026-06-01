namespace ReUse.Application.Options;

public class RecommendationWeights
{
    public double CategoryAffinity { get; set; } = 0.35;
    public double Freshness { get; set; } = 0.20;
    public double Popularity { get; set; } = 0.20;
    public double SellerAffinity { get; set; } = 0.15;
    public double Location { get; set; } = 0.10;
    public double PremiumMultiplierMax { get; set; } = 1.40;
}