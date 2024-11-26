namespace CleanArchitecture.Northwind.Application.Common.Models.Letter;

public class ForgotPasswordLetterModel : BaseLetterModel
{
    public string UserName { get; set; }

    public string ResetCodeLink { get; set; }
}
