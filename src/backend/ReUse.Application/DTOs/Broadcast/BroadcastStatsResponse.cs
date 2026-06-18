namespace ReUse.Application.DTOs.Broadcast;

public class BroadcastSummaryStats
{
    public int TotalSent { get; init; }
    public int TotalScheduled { get; init; }
    public int TotalRecipients { get; init; }
    public int TotalDelivered { get; init; }
}