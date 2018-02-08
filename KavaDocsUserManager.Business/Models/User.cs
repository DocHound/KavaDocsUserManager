using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace KavaDocsUserManager.Business.Models
{

    [DebuggerDisplay("{UserDisplayName}")]
    public class User
    {
        public User()
        {
            Id = Guid.NewGuid();
            Repositories = new List<RepositoryUser>();
            IsActive = true;
        }

        public Guid Id { get; set; }

        [Required]
        public string UserDisplayName { get; set; }

        [Required]
        public string Email { get; set; }
        
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Initials { get; set; }

        public string Company { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;

        

        public bool IsAdmin
        {
            get
            {
                if (!IsActive)
                    return false;

                return _isAdmin;
            }
            set { _isAdmin = value; }
        }
        private bool _isAdmin;

        public bool IsActive
        {
            get
            {
                // account has to be validated!
                if (string.IsNullOrEmpty(ValidationKey))
                    return false;

                return _isActive;
            }
            set { _isActive = value; }
        }
        private bool _isActive;


        public string ValidationKey { get; set; }

        [JsonIgnore]
        [Required]
        public string Password
        {
            get { return _password; }
            set => _password = UserBusiness.HashPassword(value, Id.ToString());
        }
        [XmlIgnore]
        private string _password;

        public List<RepositoryUser> Repositories { get; set; }

        //public List<UserOrganization> Organizations { get; set; }

    }

 


}
