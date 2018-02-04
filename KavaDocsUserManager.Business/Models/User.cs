﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace KavaDocsUserManager.Business.Models
{
    public class User
    {
        public User()
        {
            Id = Guid.NewGuid();
            Repositories = new List<UserRepository>();
        }

        public Guid Id { get; set; }

        public string UserDisplayName { get; set; }

        [Required]
        public string Email { get; set; }
        
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Company { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;

        public bool IsAdmin { get; set; }

        [JsonIgnore]
        [Required]
        public string Password
        {
            get { return _password; }
            set => _password = UserBusiness.HashPassword(value, Id.ToString());
        }
        [XmlIgnore]
        private string _password;

        public List<UserRepository> Repositories { get; set; }

        //public List<UserOrganization> Organizations { get; set; }

    }

 


}
