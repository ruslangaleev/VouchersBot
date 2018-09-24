using TravelerBot.MVC.Data.Models;

namespace TravelerBot.MVC.Data.Repositories.Logic
{
    public class UserStateRepository
    {
        DatabaseContext _context = new DatabaseContext();

        public void AddUserState(UserState userState)
        {
            _context.UserStates.Add(userState);
        }

        public void Update(UserState userState)
        {
            _context.Entry(userState).State = System.Data.Entity.EntityState.Modified;
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }
    }
}