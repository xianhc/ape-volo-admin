using Ape.Volo.Entity.Base;
using SqlSugar;

namespace Ape.Volo.Entity.Core.System
{
    /// <summary>
    /// 文件记录
    /// </summary>
    [SugarTable("sys_file_record")]
    public class FileRecord : BaseEntity
    {
        /// <summary>
        /// 文件描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 文件类型
        /// </summary>
        public string? ContentType { get; set; }

        /// <summary>
        /// 文件类别
        /// </summary>
        public string? ContentTypeName { get; set; }

        /// <summary>
        /// 文件类别英文名称
        /// </summary>
        public string? ContentTypeNameEn { get; set; }

        /// <summary>
        /// 文件原名称
        /// </summary>
        public string? OriginalName { get; set; }

        /// <summary>
        /// 文件新名称
        /// </summary>
        public string? NewName { get; set; }

        /// <summary>
        /// 文件存储路径
        /// </summary>
        public string? FilePath { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public string? Size { get; set; }
    }
}
