using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace KavaDocsUserManager.Business.Models
{
    public class User
    {
        public User()
        {
            Id = Guid.NewGuid();

            Repositories = new List<Repository>();
            Organizations = new List<Organization>();
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

        public List<Repository> Repositories { get; set; }

        public List<Organization> Organizations { get; set; }

    }


    
}
