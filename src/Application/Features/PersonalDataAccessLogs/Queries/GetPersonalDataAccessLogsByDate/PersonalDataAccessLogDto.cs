namespace CleanArchitecture.Northwind.Application.Features.PersonalDataAccessLogs.Queries.GetPersonalDataAccessLogsByDate;

public class PersonalDataAccessLogDto
{
    public int Id { get; set; }
    public string ViewerUserName { get; set; }
    public string TargetUserName { get; set; }
    public string Action { get; set; }
    public DateTime Accessed { get; set; }
    public string Description { get; set; }
}
