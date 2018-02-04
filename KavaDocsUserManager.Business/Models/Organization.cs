using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace KavaDocsUserManager.Business.Models
{
    public class Organization
    {
        public Organization()
        {
            Id = Guid.NewGuid();
            Repositories = new List<OrganizationRepository>();
        }

        public Guid Id { get; set; }

        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(250)]
        public string Description { get; set; }
        
        public bool IsActive { get; set; }

        public List<OrganizationRepository> Repositories { get; set; }
    }
}
