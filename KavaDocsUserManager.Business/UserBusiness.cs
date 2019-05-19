using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using KavaDocsUserManager.Business.Configuration;
using KavaDocsUserManager.Business.Models;
using Microsoft.EntityFrameworkCore;
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

        protected override void OnAfterCreated(User user)
        {
            base.OnAfterCreated(user);
            user.ValidationKey = DataUtils.GenerateUniqueId(12).ToLower();
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

                if (!user.IsActive)
                {
                    SetError("This account is not activated yet. Please activate the account, or reset your password");
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
            return Context.Users
                    .Include(u=> u.Repositories)
                    .Include("Repositories.Repository")
                    .FirstOrDefault(usr => usr.Id == id);
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
        /// Returns a user by its validation Id
        /// </summary>
        /// <param name="validationId"></param>
        /// <returns></returns>
        public User GetUserByValidationId(string validationId)
        {
            if (string.IsNullOrEmpty(validationId))
            {
                SetError("Email cannot be blank.");
                return null;
            }
            return Context.Users.FirstOrDefault(usr => usr.ValidationKey == validationId);
        }


        /// <summary>
        /// Writes the actual user to the database using 
        /// EF model.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public User SaveUser(User user)
        {
            //bool isNewUser = false;

            var curUser = Context.Users.FirstOrDefault(usr => usr.Email == user.Email);

            if (curUser == null)
            {
                curUser = new User();
                var id = curUser.Id;
                DataUtils.CopyObjectData(user, curUser);
                curUser.Id = id;
                Context.Users.Add(curUser);
                curUser.ValidationKey = DataUtils.GenerateUniqueId(12).ToLower();
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
        /// Creates a new repository for a given user
        /// </summary>
        /// <param name="id"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        public bool CreateRepositoryForUser(Guid userId, Repository repository)
        {
            var user = Context.Users.FirstOrDefault(u=> u.Id == userId);
            if (user == null)
                return false;

            // already attached - just return
            if (Context.UserRepositories.Any(ur => ur.UserId == userId && ur.RepositoryId == repository.Id))
                return true;

            var map = new RepositoryUser()
            {
                UserId = user.Id,
                Repository = repository,
                UserTypes = RepositoryUserTypes.Owner
            };
            user.Repositories.Add(map);
            
            return Save();
        }


        /// <summary>
        /// Deletes a repository
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="repoId"></param>
        /// <returns></returns>
        public bool DeleteRepository(Guid repoId)
        {
            var repo = Context.Repositories.FirstOrDefault(r => r.Id == repoId);
            if (repo == null)
            {
                SetError("Invalid repository to remove.");
                return false;
            }

            var userRepo = Context.UserRepositories.FirstOrDefault(m=> m.RepositoryId == repoId);
            Context.UserRepositories.Remove(userRepo);
            
            Context.Remove(repo);

            AutoValidate = false;
            return Save();
        }


        /// <summary>
        /// Gets a list of roles for the given user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="repositoryId"></param>
        /// <returns></returns>
        public List<Role> GetRepositoryRoles(Guid userId, Guid repositoryId)
        {
            var list = Context.UserRoles
                .Include("Role")
                .Where(ur => ur.RepositoryId == repositoryId && ur.UserId == userId)
                .Select(ur => ur.Role)
                .ToList();

            return list;
        }

        /// <summary>
        /// Gets a list of roles for the user
        /// </summary>
        /// <param name="repositoryId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public List<Role> GetRepositoryRoles(Guid repositoryId, User user = null)
        {
            if (user == null)
                user = Entity;
            if (user == null)
                throw new ArgumentException("To retrieve roles you need to pass a user.");

            var userId = user.Id;

            var list = Context.UserRoles
                .Include("Role")
                .Where(ur => ur.RepositoryId == repositoryId && ur.UserId == userId)
                .Select(ur => ur.Role)
                .ToList();

            return list;
        }

        /// <summary>
        /// Returns repository roles as a string
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="repositoryId"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public string GetRepositoryRolesAsString(Guid userId, Guid repositoryId,string separator = ",")
        {
            var list = Context.UserRoles
                .Include("Role")
                .Where(ur => ur.RepositoryId == repositoryId && ur.UserId == userId)
                .Select(ur => ur.Role);

            StringBuilder sb = new StringBuilder();
            foreach (var role in list)
            {
                sb.Append(role.Name + separator);
            }

            if (sb.Length < 1)
                return null;

            // strip last separator
            sb.Length--;

            return sb.ToString();
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

        private static Regex _displayNameRegEx = new Regex("^([a-z]|[A-z]|[0-9]|[-])*$");
        
        public bool IsValidDisplayName(string displayName)
        {
            if (displayName.EndsWith("-") || displayName.StartsWith("-"))
                return false;

            return _displayNameRegEx.IsMatch(displayName);
        }

        public bool IsValidEmailAddress(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }


        protected override bool OnValidate(User user)
        {
            bool? isNewUser = IsNewEntity(user);
            if (isNewUser == null)
                isNewUser = true;
            
            ValidationErrors.Clear();


            if(!IsValidDisplayName(user.UserDisplayName))
            ValidationErrors.Add(
                "Invalid display name. Name should contain no spaces contain: only alpha numeric characters and dashes and can't start or end with a dash.","UserDisplayName");

            if (Context.Users.Any(usr => usr.Email == user.Email && usr.Id != user.Id))
                ValidationErrors.Add("Email address is already in use by another user.","Email");
            
             if (Context.Users.Any(usr => usr.UserDisplayName == user.UserDisplayName && usr.Id != user.Id))
                 ValidationErrors.Add("Display name is already in use by another user.","UserDisplayName");


            if (string.IsNullOrEmpty(user.Email))
                ValidationErrors.Add("Email address can't be empty.","Email");

            if (!IsValidEmailAddress(user.Email))
                ValidationErrors.Add("Invalid email format.","Email");
            
            if (string.IsNullOrEmpty(user.UserDisplayName))
                ValidationErrors.Add("User display name nan't be empty.","UserDisplayName");

            if (string.IsNullOrEmpty(user.Password) || user.Password.Length < 5)
                ValidationErrors.Add("Password should be at least 5 characters long.","Password");
            else
            {
                // always force password to be updated and hashed even if it was entered as plain text            
                // this method detects if the password is already encoded
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

        #region Validation and  Password Recovery

        /// <summary>
        /// Validates an email address and makes the account active
        /// </summary>
        /// <param name="email"></param>
        /// <param name="validationId"></param>
        /// <returns></returns>
        public bool ValidateEmail(string validationId)
        {
            var user = GetUserByValidationId(validationId);
            if (user == null || user.ValidationKey != validationId)
            {
                SetError("Invalid email or validation code provided");
                return false;
            }

            // clear the validation key
            user.ValidationKey = null;
            user.IsActive = true;

            return Save();
        }


        /// <summary>
        /// Creates a new Recovery Validation Id that can be used to recover
        /// a password
        /// </summary>
        /// <param name="email"></param>
        /// <returns>id or null on error</returns>
        public string CreateRecoveryValidationId(string email)
        {
            string validationLink = Guid.NewGuid().ToString("N");

            var user = GetUserByEmail(email);
            if (user == null)
                return null;
            user.ValidationKey = validationLink;            

            if (!Save())
                return null;

            return validationLink;
        }

        /// <summary>
        /// Resets a users password based on an email address and validation key
        /// </summary>
        /// <param name="validationId"></param>
        /// <param name="email"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        public bool RecoverPassword(string validationId, string newPassword)
        {
            var user = GetUserByValidationId(validationId);

            if (user == null || user.ValidationKey != validationId)
            {
                SetError("Invalid validation code provided");
                return false;
            }

            user.Password = newPassword;

            if (!Validate(user))
                return false;

            user.ValidationKey = null;
            user.IsActive = true;
            return Save();

        }
        #endregion
    }
}
