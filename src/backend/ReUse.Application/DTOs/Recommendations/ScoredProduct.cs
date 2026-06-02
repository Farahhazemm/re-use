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
    public double Score { get; init; }
}