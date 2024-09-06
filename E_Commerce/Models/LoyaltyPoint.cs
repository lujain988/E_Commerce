using System;
using System.Collections.Generic;

namespace E_Commerce.Models;

public partial class LoyaltyPoint
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int PointsEarned { get; set; }

    public int? PointsUsed { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
