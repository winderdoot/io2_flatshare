using flatshare_server.Infrastructure.Model.Requests;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Model.Users;
using flatshare_server.Infrastructure.Repositories;
using flatshare_server.Infrastructure.Utils;

namespace flatshare_server.Infrastructure.Services;

public class UserService
{
    private IUserRepository _userRepo;
    private IResetCodesRepository _resetCodesRepo;
    private ISessionRepository _sessionRepo;

    public UserService(IUserRepository userRepo, IResetCodesRepository resetRepo, ISessionRepository sessionRepo)
    {
        _userRepo = userRepo;
        _resetCodesRepo = resetRepo;
        _sessionRepo = sessionRepo;
    }
    public async Task<User> Create(CreateUserRequest request)
    {
        User user = User.TryCreate(request);
        await _userRepo.SaveNew(user);   
        return user;
    }
    public async Task<User> GetByIdAsync(Guid id)
    {
        return await _userRepo.GetById(id);
    }

    public async Task CreatePasswordResetEntry(Guid userId, string resetCode)
        => await _resetCodesRepo.SaveNew(userId, resetCode);

    public async Task<bool> ResetPassword(ConfirmPasswordResetRequest request)
    {
        var user = await _userRepo.GetByEmail(request.Email);
        if (user is null)
        {
            return false;
        }

        // check db requests with email and code
        var valid = await _resetCodesRepo.CheckValidity(user.Id, request.ResetToken);

        // if code matches and request is valid update password, invalidate active sessions (they use old password) and return true
        if (valid)
        {
            await _userRepo.UpdatePassword(user.Id, request.NewPassword);
            await _sessionRepo.InvalidateByUserId(user.Id);
            await _resetCodesRepo.InvalidateCodes(user.Id);
        }
        // else invalidate request code and return false
        else
        {
            await _resetCodesRepo.InvalidateCodes(user.Id);
            return false;
        }

        return true;
    }

    public async Task<UserDTO> GetById(Guid id)
    {
        User user = await _userRepo.GetById(id);

        return new UserDTO(user.Id, user.FirstName, user.LastName, user.Email, user.Role.ToString());
    }

    public async Task<UserDTO?> GetByEmail(string email)
    {
        User? user = await _userRepo.GetByEmail(email);
        if (user == null)
            return null;

        return new UserDTO(user.Id, user.FirstName, user.LastName, user.Email, user.Role.ToString());
    }
    public async Task<TenantPreferencesDTO> GetPreferencesAsync(Guid userId)
    {
        var user = await _repo.GetById(userId);

        if (user.Role is not TenantRole tenantRole)
        {
            throw ErrorResponse.Generate(
                "Only tenants can have preferences.",
                StatusCodes.Status403Forbidden
            );
        }

        return tenantRole.TenantPreferences.IntoDTO();
    }

    public async Task<TenantPreferencesDTO> UpdatePreferencesAsync(Guid userId, TenantPreferencesDTO dto)
    {
        var user = await _repo.GetById(userId);

        if (user.Role is not TenantRole tenantRole)
        {
            throw ErrorResponse.Generate(
                "Only tenants can update preferences.",
                StatusCodes.Status403Forbidden
            );
        }

        tenantRole.TenantPreferences.UpdatePreferences(dto);
        await _repo.Update(user);

        return tenantRole.TenantPreferences.IntoDTO();
    }
}
