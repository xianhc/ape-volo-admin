using System.ComponentModel.DataAnnotations;
using Ape.Volo.Shared.Attributes;

namespace Ape.Volo.Application.Contracts.Permission.OnlineUsers.Models;

/// <summary>
/// 强制退出的会话标识集合
/// </summary>
public class DropOutSessionsRequest
{
    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "ID")]
    [Required(ErrorMessage = "{0}required")]
    [AtLeastOneItem]
    public HashSet<string> IdArray { get; set; } = default!;
}
