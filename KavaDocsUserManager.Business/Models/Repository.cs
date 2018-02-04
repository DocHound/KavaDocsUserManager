using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace KavaDocsUserManager.Business.Models
{
    public class Repository
    {
        public Repository()
        {
            Id = Guid.NewGuid();
            Users = new List<UserRepository>();
            //Contributors = new List<Contributor>();
            //Organizations = new List<Organization>();
            IsActive = true;
        }

        public Guid Id { get; set; }        


        [StringLength(50)]
        public string Prefix { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        [StringLength(100)]
        public string Title { get; set; }

        public string Settings { get; set; }

        public string TableOfContents { get; set; }

        public bool IsActive { get; set; }

        public bool IncludeInSearchResults { get; set; }

        public List<UserRepository> Users { get; set; }

        //public List<Organization> Organizations { get; set; }

        //public List<Contributor> Contributors { get; set; }
    }
}
