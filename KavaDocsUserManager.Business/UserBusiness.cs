using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using KavaDocsUserManager.Business.Configuration;
using KavaDocsUserManager.Business.Models;
using Westwind.Data.EfCore;
using Westwind.Utilities;

namespace KavaDocsUserManager.Business
{
    public class UserBusiness : EntityFrameworkBusinessObject<KavaDocsContext, User>
    {
        private KavaDocsConfiguration Configuration;

        public UserBusiness(KavaDocsContext context, KavaDocsConfiguration config) : base(context)
        {
            Context = context;
            Configuration = config;
        }

        /// <summary>
        /// Authenticates a user by username and password
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool AuthenticateUser(string username, string password)
        {
            var user = AuthenticateAndRetrieveUser(username, password);
            if (user == null)
                return false;

            return true;
        }


        /// <summary>
        /// Authenticates a user by username and password and returns the
        /// user instance
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public User AuthenticateAndRetrieveUser(string username, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    SetError("Invalid username or password.");
                    return null;
                }

                // assumes only no dupe email addresses
                var user = GetUserByEmail(username);
                if (user == null)
                {
                    SetError("Invalid username or password.");
                    return null;
                }

                string passwordHash = HashPassword(password, user.Id.ToString());
                if (user.Password != passwordHash && user.Password != password)
                {
                    SetError("Invalid username or password.");
                    return null;
                }

                return user;
            }
            catch (Exception ex)
            {
                SetError(ex);
                return null;
            }
        }

        #region CRUD Overrides
        /// <summary>
        /// Retrieves an individual User
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public User GetUser(Guid id)
        {
            return Context.Users.FirstOrDefault(usr => usr.Id == id);
        }

        /// <summary>
        /// Returns an individual user by user name
        /// </summary>        
        /// <param name="email"></param>
        /// <returns></returns>
        public User GetUserByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                SetError("Email cannot be blank.");
                return null;
            }
            return Context.Users.FirstOrDefault(usr => usr.Email == email);
        }

        /// <summary>
        /// Writes the actual user to the database using 
        /// EF model.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public User SaveUser(User user)
        {
            bool isNewUser = false;

            var curUser = Context.Users.FirstOrDefault(usr => usr.Email == user.Email);

            if (curUser == null)
            {
                curUser = new User();
                var id = curUser.Id;
                DataUtils.CopyObjectData(user, curUser);
                curUser.Id = id;
                Context.Users.Add(curUser);
                isNewUser = true;
            }
            else
            {
                curUser.Email = user.Email;
                curUser.FirstName = user.FirstName;
                curUser.LastName = user.LastName;
                curUser.Company = user.Company;
                curUser.UserDisplayName = user.UserDisplayName;
                curUser.Password = user.Password;
            }

            if (!Validate(curUser))
                return null;

            Context.SaveChanges();

            return curUser;
        }


        public bool DeleteUser(Guid userId)
        {
            var user = Context.Users
                .FirstOrDefault(usr => usr.Id == userId);

            if (user == null)
                return false;

            //foreach (var role in user.Roles)
            //    user.Roles.Remove(role);
            //Context.Users.Remove(user);

            Context.SaveChanges();

            return true;
        }

        #endregion

        #region Repositories


        /// <summary>
        /// Adds a repository to 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        public bool AddRepositoryToUser(Guid userId, Repository repository)
        {
            var user = Context.Users.FirstOrDefault(u=> u.Id == userId);
            if (user == null)
                return false;
                        
            var map = new UserRepository()
            {
                UserId = user.Id, Respository = repository
            };
            user.Repositories.Add(map);
            
            return Save();
        }

        public bool RemoveRepositoryFromUser(Guid userId, Guid repoId)
        {
            var user = Context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return false;

            var repo = Context.Repositories.FirstOrDefault(r => r.Id == repoId);
            if (repo == null)
            {
                SetError("Invalid repository to remove.");
                return false;
            }
            
            var map = Context.UsersRepositories.FirstOrDefault(m => m.RepositoryId == repoId && m.UserId == userId);
            if (map != null)
            {
                Context.UsersRepositories.Remove(map);
                Context.Repositories.Remove(repo);
            }

            AutoValidate = false;
            return Save();
        }
        
        #endregion


        #region Utilities and Helpers
        const string HashPostFix = "|~|";

        /// <summary>
        /// Returns an hashed and salted password.
        /// 
        /// Encoded Passwords end in || to indicate that they are 
        /// encoded so that bus objects can validate values.
        /// </summary>
        /// <param name="password">The password to convert</param>
        /// <param name="uniqueSalt">
        /// Unique per instance salt - use user id</param>
        /// <param name="appSalt">Salt to apply to the password</param>
        /// <returns>Hashed password. If password passed is already a hash
        /// the existing hash is returned
        /// </returns>
        public static string HashPassword(string password, string uniqueSalt,
            string appSalt = "#5518-21%5#36@")
        {
            // don't allow empty password
            if (string.IsNullOrEmpty(password))
                return string.Empty;

            // already encoded
            if (password.EndsWith(HashPostFix))
                return password;

            string saltedPassword = uniqueSalt + password + appSalt;

            // pre-hash
            var sha = new SHA1CryptoServiceProvider();
            byte[] hash = sha.ComputeHash(Encoding.ASCII.GetBytes(saltedPassword));

            // hash again
            var sha2 = new SHA256CryptoServiceProvider();
            hash = sha2.ComputeHash(hash);

            return StringUtils.BinaryToBinHex(hash) + HashPostFix;
        }

        #endregion


        #region Validations

        protected override bool OnValidate(User user)
        {
            bool? isNewUser = IsNewEntity(user);
            if (isNewUser == null)
                isNewUser = true;
            
            ValidationErrors.Clear();

            if (isNewUser.Value)
            {
                if (Context.Users.Any(usr => usr.Email == user.Email))
                    ValidationErrors.Add("Email address is already in use.");
            }

            if (string.IsNullOrEmpty(user.Email))
                ValidationErrors.Add("Email address can't be empty.");

            if (string.IsNullOrEmpty(user.UserDisplayName))
                ValidationErrors.Add("User display name nan't be empty.");

            if (string.IsNullOrEmpty(user.Password))
                ValidationErrors.Add("Password can't be empty.");
            else
            {
                // always force password to be updated and hashed even if it was entered as plain text            
                user.Password = HashPassword(user.Password, user.Id.ToString());
            }

            if (ValidationErrors.Count > 0)
            {
                ErrorMessage = ValidationErrors.ToString();
                return false;
            }
            return true;
        }

        #endregion
    }
}
