using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using TravelerBot.MVC.Data.Models;

namespace TravelerBot.MVC.Data.Repositories
{
    public class UserRepository
    {
        DatabaseContext _context = new DatabaseContext();

        public void Add(User user)
        {
            _context.Users.Add(user);
        }

        public void Update(User user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }

        public void Delete(User user)
        {
            _context.Users.Remove(user);
        }

        public User Get(Guid userId)
        {
            return _context.Users.Find(userId);
        }

        public async Task<User> Get(int accountId)
        {
            return await  _context.Users
                .Include(t => t.Role)
                .FirstOrDefaultAsync(t => t.AccountId == accountId);
        }

        public async Task<IEnumerable<User>> Get()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<Role> GetRoleAsync(string role)
        {
            return await _context.Roles.FirstOrDefaultAsync(t => t.Name == role);
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }
    }
}