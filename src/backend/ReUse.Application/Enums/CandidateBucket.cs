using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReUse.Application.Enums;

public enum CandidateBucket
{
    //Products in the user's followed categories || top-favourited categories
    Affinity,
    //Products listed by sellers the user follows
    SellerAffinity,
    //Products with the highest recent favourite activity
    Trending,
    //Products located in the user's city or country
    Local,
    //Recently created products =>last 7 days
    Fresh,
    //Coldstart fallback  top products by alltime favourite count
    // Used when the user has no follows or favourites
    PopularAllTime
}