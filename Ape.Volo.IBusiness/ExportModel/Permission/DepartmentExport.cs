﻿using System.ComponentModel.DataAnnotations;
using Ape.Volo.Common.Global;
using Ape.Volo.Common.Model;

namespace Ape.Volo.IBusiness.ExportModel.Permission;

public class DepartmentExport : ExportBase
{
    [Display(Name = "部门名称")]
    public string Name { get; set; }

    [Display(Name = "部门父ID")]
    public long ParentId { get; set; }

    [Display(Name = "排序")]
    public int Sort { get; set; }

    [Display(Name = "是否启用")]
    public EnabledState EnabledState { get; set; }

    [Display(Name = "子部门个数")]
    public int SubCount { get; set; }
}