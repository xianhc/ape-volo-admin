using System;
using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.SharedModel.Dto.Jwt;

/// <summary>
/// 
/// </summary>
public class RefreshToken
{
    /// <summary>
    /// 组
    /// </summary>
    [Required(ErrorMessage = "{0}required")]
    public string Token { get; set; }

}
