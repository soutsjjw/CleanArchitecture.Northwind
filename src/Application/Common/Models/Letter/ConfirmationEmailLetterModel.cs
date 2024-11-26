namespace CleanArchitecture.Northwind.Application.Common.Models.Letter;

public class ConfirmationEmailLetterModel : BaseLetterModel
{
    public string UserName { get; set; }

    public string ConfirmationLink { get; set; }
}
