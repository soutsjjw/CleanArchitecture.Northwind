namespace CleanArchitecture.Northwind.Application.Common.DTOs;

public class OptionItemDto
{
    // 父層的值
    public string ParentValue { get; set; }

    // 目前項目的值
    public string Value { get; set; }

    // 顯示文字
    public string Text { get; set; }

    // 建構子（可選）
    public OptionItemDto(string parentValue, string value, string text)
    {
        ParentValue = parentValue;
        Value = value;
        Text = text;
    }

    // 無參數建構子（如需支援序列化）
    public OptionItemDto() { }
}
