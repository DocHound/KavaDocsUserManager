using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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


        /// <summary>
        /// The domain Prefix and display key for this repository
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Prefix { get; set; }

        /// <summary>
        /// The display name of this repository
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        /// <summary>
        /// An optional longer description of the repository
        /// </summary>
        [StringLength(200)]
        public string Description { get; set; }


        /// <summary>
        /// Comma delimited list of Tag keywords for this repo
        /// </summary>
        public string Tags { get; set; }


        /// <summary>
        /// JSON Settings required to get the site bootstrapped
        /// </summary>
        public string Settings { get; set; }


        /// <summary>
        /// Optional table of contents to be used in lieu of the TOC
        /// provided by the help target site
        /// </summary>
        public string TableOfContents { get; set; }

        
        public bool IsActive { get; set; }

        public bool IncludeInSearchResults { get; set; }

        /// <summary>
        /// Owners and Contributors of this repository
        /// </summary>
        public List<RepositoryUser> Users { get; set; }
                

        /// <summary>
        /// Optional organization that this repository belongs to
        /// </summary>
        public Organization Organization { get; set; }
    }
}
