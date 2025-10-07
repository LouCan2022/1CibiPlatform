
namespace Auth.Data.Repository;

public class AuthRepository : IAuthRepository
{
	private readonly AuthApplicationDbContext _dbcontext;

	public AuthRepository(AuthApplicationDbContext dbcontext)
	{
		this._dbcontext = dbcontext;
	}

	public async Task<LoginDTO> LoginAsync(LoginCred cred)
	{
		var userData = await (from user in _dbcontext.AuthUsers
							  join userRole in _dbcontext.AuthUserAppRoles
														 on user.Id equals userRole.UserId into userRolesGroup
							  where user.Username == cred.Username && user.IsActive == true
							  select new LoginDTO(
							   user.Id,
							   user.Username,
							   user.PasswordHash,
							   user.Email!,
							   user.FirstName!,
							   user.LastName!,
							   user.MiddleName,
							   userRolesGroup.Select(r => r.AppId.ToString()).ToList(),
							   userRolesGroup.GroupBy(r => r.AppId)
											 .Select(g => g.Select(r => r.Submenu).ToList())
											 .ToList(),
							   userRolesGroup.Select(r => r.RoleId.ToString()).ToList()
							  )
			).AsNoTracking()
			 .FirstOrDefaultAsync();


		return userData!;
	}

	public Task<bool> SaveRefreshToken(Guid userId, string refreshToken, DateTime expiryDate)
	{
		throw new NotImplementedException();
	}
}
