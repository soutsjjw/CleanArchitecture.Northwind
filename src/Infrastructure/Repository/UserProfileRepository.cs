using System.Data;
using CleanArchitecture.Northwind.Application.Common.DTOs;
using CleanArchitecture.Northwind.Application.Common.Interfaces.Repository;
using CleanArchitecture.Northwind.Application.Common.Settings;
using CleanArchitecture.Northwind.Domain.Entities.Identity;
using Dapper;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Northwind.Infrastructure.Repository;

public class UserProfileRepository : IUserProfileRepository
{
    private readonly IDbConnection _dbConnection;
    public DataProtectionSettings _dataProtectionSettings { get; }

    public UserProfileRepository(IDbConnection dbConnection,
        IOptions<DataProtectionSettings> dataProtectionSettings)
    {
        _dbConnection = dbConnection;
        _dataProtectionSettings = dataProtectionSettings.Value;
    }

    public async Task<ApplicationUserProfile?> GetByIdAsync(object id)
    {
        return (await GetUserProfilesAsync(new AccountConditionDto { UserId = id.ToString() })
            ).FirstOrDefault();
    }

    public async Task<IEnumerable<ApplicationUserProfile>> GetUserProfilesAsync(AccountConditionDto condition)
    {
        var sql = @"
SELECT[Id]
,[UserId]
,[FullName]
,CONVERT(NVARCHAR(MAX), DECRYPTBYPASSPHRASE(@SQLPassPhrase, [IDNo])) AS [IDNo]
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

        parameters.Add("SQLPassPhrase", _dataProtectionSettings.SQLPassPhrase);

        if (!string.IsNullOrEmpty(condition.UserId))
        {
            sql += " AND A.UserId = @UserId";
            parameters.Add("UserId", condition.UserId);
        }

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

    public async Task<int> AddUserProfileAsync(ApplicationUserProfile profile, string createdBy)
    {
        var sql = @"
INSERT INTO [dbo].[AspNetUserProfiles]
([UserId], [FullName], [IDNo], [Gender], [Title], [DepartmentId], [OfficeId], [IsTotpEnabled], [TotpSecretKey], [TotpRecoveryCodes], [Status], [Created], [CreatedBy])
VALUES
(@UserId, @FullName, ENCRYPTBYPASSPHRASE(@SQLPassPhrase, @IDNo), @Gender, @Title, @DepartmentId, @OfficeId, @IsTotpEnabled, @TotpSecretKey, @TotpRecoveryCodes, @Status, @Created, @CreatedBy);
";

        var parameters = new DynamicParameters();
        parameters.Add("SQLPassPhrase", _dataProtectionSettings.SQLPassPhrase);
        parameters.Add("UserId", profile.UserId);
        parameters.Add("FullName", profile.FullName);
        parameters.Add("IDNo", profile.IDNo);
        parameters.Add("Gender", profile.Gender);
        parameters.Add("Title", profile.Title);
        parameters.Add("DepartmentId", profile.DepartmentId);
        parameters.Add("OfficeId", profile.OfficeId);
        parameters.Add("IsTotpEnabled", profile.IsTotpEnabled);
        parameters.Add("TotpSecretKey", profile.TotpSecretKey);
        parameters.Add("TotpRecoveryCodes", profile.TotpRecoveryCodes);
        parameters.Add("Status", profile.Status);
        parameters.Add("Created", profile.Created ?? DateTime.Now);
        parameters.Add("CreatedBy", profile.CreatedBy);

        int affectedRows = await _dbConnection.ExecuteAsync(sql, parameters);

        return affectedRows;
    }

    public async Task<int> UpdateUserProfileAsync(ApplicationUserProfile profile, string lastModifiedBy)
    {
        var sql = @"
UPDATE [dbo].[AspNetUserProfiles]
SET
    [FullName] = @FullName,
    [IDNo] = ENCRYPTBYPASSPHRASE(@SQLPassPhrase, @IDNo),
    [Gender] = @Gender,
    [Title] = @Title,
    [DepartmentId] = @DepartmentId,
    [OfficeId] = @OfficeId,
    [Status] = @Status,
    [LastModified] = @LastModified,
    [LastModifiedBy] = @LastModifiedBy
WHERE [UserId] = @UserId;
";

        var parameters = new DynamicParameters();
        parameters.Add("SQLPassPhrase", _dataProtectionSettings.SQLPassPhrase);
        parameters.Add("UserId", profile.UserId);
        parameters.Add("FullName", profile.FullName);
        parameters.Add("IDNo", profile.IDNo);
        parameters.Add("Gender", profile.Gender);
        parameters.Add("Title", profile.Title);
        parameters.Add("DepartmentId", profile.DepartmentId);
        parameters.Add("OfficeId", profile.OfficeId);
        parameters.Add("IsTotpEnabled", profile.IsTotpEnabled);
        parameters.Add("TotpSecretKey", profile.TotpSecretKey);
        parameters.Add("TotpRecoveryCodes", profile.TotpRecoveryCodes);
        parameters.Add("Status", profile.Status);
        parameters.Add("LastModified", profile.LastModified ?? DateTime.Now);
        parameters.Add("LastModifiedBy", profile.LastModifiedBy);

        int affectedRows = await _dbConnection.ExecuteAsync(sql, parameters);

        return affectedRows;
    }
}
