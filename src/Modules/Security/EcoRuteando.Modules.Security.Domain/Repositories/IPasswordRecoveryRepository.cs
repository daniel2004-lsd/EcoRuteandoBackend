using EcoRuteando.Modules.Security.Domain.Entities;

namespace EcoRuteando.Modules.Security.Domain.Repositories;

public interface IPasswordRecoveryRepository
{
    Task AddAsync(PasswordRecovery passwordRecovery);

    Task<PasswordRecovery?> GetByIdAsync(Guid id);

    Task<PasswordRecovery?> GetByTokenHashAsync(string tokenHash);

    Task<IEnumerable<PasswordRecovery>> GetByUserIdAsync(Guid userId);

    void Update(PasswordRecovery passwordRecovery);

    void Delete(PasswordRecovery passwordRecovery);

}