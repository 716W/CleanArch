namespace GymManagement.Domain.Entities;

public class MembershipPlan
{
    public Guid Id { get; set; }
    public string Name { get; set; } = String.Empty;
    public int DurationInDays { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
    public int MaxFreezeDays { get; set; }
    public int DisplayOrder { get; set; }

}