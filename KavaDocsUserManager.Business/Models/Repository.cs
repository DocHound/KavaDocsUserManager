using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text;

namespace KavaDocsUserManager.Business.Models
{
    [DebuggerDisplay("{Title}")]
    public class Repository
    {
        public Repository()
        {
            Id = Guid.NewGuid();
            Users = new List<RepositoryUser>();                    
            IsActive = true;
        }

        public Guid Id { get; set; }        

        [Required]
        [StringLength(50)]
        public string Prefix { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        public string Settings { get; set; }

        public string TableOfContents { get; set; }

        public bool IsActive { get; set; }

        public bool IncludeInSearchResults { get; set; }

        public List<RepositoryUser> Users { get; set; }
        
        //public List<RepositoryContributor> Contributors { get; set; }

        public Organization Organization { get; set; }
    }
}
