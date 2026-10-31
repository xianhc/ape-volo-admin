using Newtonsoft.Json;

namespace Ape.Volo.Application.Contracts
{
    /// <summary>
    /// 实体基类
    /// </summary>
    public class BaseEntityDto<T> where T : IEquatable<T>
    {
        /// <summary>
        /// 主键
        /// </summary>
        public T Id { get; set; } = default!;

        /// <summary>
        /// 创建者主键；后台任务或匿名写入时为空
        /// </summary>
        public long? CreatorId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// 最后修改者主键
        /// </summary>
        public long? LastModifierId { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime? LastModificationTime { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        [JsonIgnore]
        public bool IsDeleted { get; set; }
    }
}
