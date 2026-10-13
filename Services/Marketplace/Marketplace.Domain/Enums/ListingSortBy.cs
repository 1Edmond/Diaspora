using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Domain.Enums;

public enum ListingSortBy
{
    Relevance = 0,   // Note moyenne puis date (comportement historique)
    Newest = 1,
    RatingDesc = 2,
    PriceAsc = 3,
    PriceDesc = 4,
    Distance = 5     // Nécessite UserLatitude/UserLongitude dans la requête
}