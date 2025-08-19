using InventoryManagement.Api.AI.Models;

namespace InventoryManagement.Api.AI.Services.Skills
{
    /// <summary>
    /// Base interface for AI skills
    /// </summary>
    public interface ISkill
    {
        SkillType SkillType { get; }
        Task<SkillResponse> ExecuteAsync(SkillRequest request);
    }

    /// <summary>
    /// Service to route queries to appropriate skills
    /// </summary>
    public interface ISkillRouter
    {
        Task<SkillResponse> RouteQueryAsync(string userQuery);
    }
}
