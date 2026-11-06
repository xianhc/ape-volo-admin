using System.ComponentModel.DataAnnotations;
using Ape.Volo.Shared.Attributes;

namespace Ape.Volo.Application.Contracts.Identity.OnlineUsers.Models;

/// <summary>
/// 强制退出的会话标识集合
/// </summary>
public class DropOutSessionsRequest
{
    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "ID")]
    [Required(ErrorMessage = "{0}不能为空。")]
    [AtLeastOneItem]
    public HashSet<string> IdArray { get; set; } = default!;
}
