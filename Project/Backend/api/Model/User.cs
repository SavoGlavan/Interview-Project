using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Model
{
    public class User
    {
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; } 

    public string? Email { get; set; }
    public required string Role { get; set; } // e.g. "admin" or "user"
    public double? Consumption { get; set; } 
    public int? TaxGroupId { get; set; }
    public TaxGroup? TaxGroup { get; set; }
    public int? PlanId { get; set; }
    public Plan? Plan { get; set; }
    }
}