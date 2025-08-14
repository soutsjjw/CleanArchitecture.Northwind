using System.Data;
using CleanArchitecture.Northwind.Application.Common.DTOs;
using CleanArchitecture.Northwind.Application.Common.Interfaces.Repository;
using CleanArchitecture.Northwind.Domain.Entities.Identity;
using Dapper;

namespace CleanArchitecture.Northwind.Infrastructure.Repository;

public class UserProfileRepository : IUserProfileRepository
{
    private readonly IDbConnection _dbConnection;

    public UserProfileRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<IEnumerable<ApplicationUserProfile>> GetUserProfilesAsync(AccountConditionDto condition)
    {
        var sql = @"
SELECT[Id]
,[UserId]
,[FullName]
,[IDNo]
,[Gender]
,[Title]
,[DepartmentId]
,[OfficeId]
,[IsTotpEnabled]
,[TotpSecretKey]
,[TotpRecoveryCodes]
,[Status]
,[Created]
,[CreatedBy]
,[LastModified]
,[LastModifiedBy]
,[IsDelete]
FROM[dbo].[AspNetUserProfiles] A
WHERE 1 = 1 ";
        var parameters = new DynamicParameters();

        if (condition.DepartmentId.HasValue)
        {
            sql += " AND A.DepartmentId = @DepartmentId";
            parameters.Add("DepartmentId", condition.DepartmentId);
        }

        if (condition.OfficeId.HasValue)
        {
            sql += " AND A.OfficeId = @OfficeId";
            parameters.Add("OfficeId", condition.OfficeId);
        }

        return await _dbConnection.QueryAsync<ApplicationUserProfile>(sql, parameters);
    }
}
