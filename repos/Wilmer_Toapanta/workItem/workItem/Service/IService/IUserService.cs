using workItem.DTO;

namespace workItem.Service.IService
{
    public interface IUserService
    {
        Task<List<UserExternalDTO>> GetActiveUsersAsync();
    }
}
