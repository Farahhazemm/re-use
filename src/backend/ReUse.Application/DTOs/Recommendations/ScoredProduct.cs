using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReUse.Application.DTOs.Recommendations;

public record ScoredProduct
{
    //The candidate that was scored.
    public CandidateProduct Candidate { get; init; } = default!;

    // The final composite score after applying all sub-score weights
    // and the PremiumMultiplier. Range: [0.0, ~1.4].
    // Results are sorted descending by this value before pagination.

    public double Score { get; init; }
}