using DataBase.Models;
using Enums.User;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;


namespace DataBase.Repositories
{
    public interface IUserRepository
    {
        UserData? GetById(int id);
        IEnumerable<UserData> GetAll();
        void Delete(UserData user);
        void BlockUsers(int userId);
        void UnblockUsers(int userId);
        void Register(string name, string password, string email, Role role = Role.User);
        UserData? FindByCredentials(string email, string password);
        IQueryable<UserData> GetQueryable();
        void MakeAdmin(int userId);
        void RemoveAdmin(int userId);
        public Task<UserData> GetByIdAsync(int id);
    }

    public class UserRepository : IUserRepository
    {
        private readonly WebDbContext _webDbContext;
        protected DbSet<UserData> _dbSet;

        public UserRepository(WebDbContext webDbContext)
        {
            _webDbContext = webDbContext;
            _dbSet = _webDbContext.Set<UserData>();
        }

        public async Task<UserData> GetByIdAsync(int id)
        {
            return await _webDbContext.Users.FindAsync(id);
        }

        public IQueryable<UserData> GetQueryable()
        {
            return _webDbContext.Users.AsQueryable();
        }

        public UserData? GetById(int id)
        {
            return _dbSet.FirstOrDefault(x => x.Id == id);
        }

        public IEnumerable<UserData> GetAll()
        {
            return _dbSet.ToList();
        }

        public UserData? FindByCredentials(string email, string password)
        {
            return _dbSet.FirstOrDefault(x => x.Email == email && x.Password == password);
        }

        public void Register(string name, string password, string email, Role role = Role.User)
        {

            var user = new UserData
            {
                Name = name,
                Password = password,
                Email = email,
                IsBlocked = false,
                Role = role,
            };

            _dbSet.Add(user);
            _webDbContext.SaveChanges();
        }

        public void Delete(UserData user)
        {
            _dbSet.Remove(user);
            _webDbContext.SaveChanges();
        }

        public void BlockUsers(int userId)
        {
            var users = _dbSet.FirstOrDefault(x => x.Id == userId).IsBlocked = true;
            _webDbContext.SaveChanges();
        }

        public void UnblockUsers(int userId)
        {
            var users = _dbSet.FirstOrDefault(x => x.Id == userId).IsBlocked = false;
            _webDbContext.SaveChanges();
        }

        public void MakeAdmin(int userId)
        {
            var users = _dbSet.FirstOrDefault(x => x.Id == userId).Role = Role.Admin;
            _webDbContext.SaveChanges();
        }

        public void RemoveAdmin(int userId)
        {
            var users = _dbSet.FirstOrDefault(x => x.Id == userId).Role = Role.User;
            _webDbContext.SaveChanges();
        }
       
    }
}
