using StoringPassword.Models;

namespace StoringPassword.Repository
{
    public interface IRepository
    {
        Task<User> GetUserAsync(int userId);
        Task<User> GetUserByLoginAsync(string login);
        Task<User> GetUserForMessageAsync(string firstName, string lastName);
        Task<List<Message>> GetMessagesAsync();
        Task AddMessageAsync(Message message);
        Task AddUserAsync(User user);
    }
}
