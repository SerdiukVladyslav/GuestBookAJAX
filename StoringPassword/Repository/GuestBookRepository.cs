using StoringPassword.Models;
using Microsoft.EntityFrameworkCore;

namespace StoringPassword.Repository
{
    public class GuestBookRepository : IRepository
    {
        private readonly UserContext _context;

        public GuestBookRepository(UserContext context)
        {
            _context = context;
        }

        public async Task<User> GetUserAsync(int userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User> GetUserByLoginAsync(string login)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Login == login);
        }

        public async Task<User> GetUserForMessageAsync(string firstName, string lastName)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.FirstName == firstName && u.LastName == lastName);
        }

        public async Task<List<Message>> GetMessagesAsync()
        {
            return await _context.Messages.Include(m => m.User).ToListAsync();
        }

        public async Task AddMessageAsync(Message message)
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
        }

        public async Task AddUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
    }
}
