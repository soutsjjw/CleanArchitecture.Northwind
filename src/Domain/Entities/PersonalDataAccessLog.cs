namespace CleanArchitecture.Northwind.Domain.Entities;

public class PersonalDataAccessLog : BaseEntity<int>
{
    /// <summary>
    /// 瀏覽者
    /// </summary>
    public string ViewerUserId { get; set; }

    /// <summary>
    /// 被瀏覽者
    /// </summary>
    public string TargetUserId { get; set; }

    /// <summary>
    /// 動作
    /// </summary>
    public string Action { get; set; }

    /// <summary>
    /// 瀏覽時間
    /// </summary>
    public DateTime Accessed { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string? Description { get; set; }
}
