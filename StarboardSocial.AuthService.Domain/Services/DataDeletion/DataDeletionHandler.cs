using FluentResults;

namespace StarboardSocial.UserService.Domain.Services;

public class DataDeletionHandler(IDataDeletionService dataDeletionService)
{
    private readonly IDataDeletionService _dataDeletionService = dataDeletionService;

    public async Task Handle(string userId)
    {
        await _dataDeletionService.DeleteUserData(userId);
    }
}