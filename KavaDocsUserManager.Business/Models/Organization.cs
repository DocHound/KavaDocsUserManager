using System;
using System.Collections.Generic;
using System.Text;

namespace KavaDocsUserManager.Business.Models
{
    public class Organization
    {
        public Organization()
        {
            Id = Guid.NewGuid();
            Repositories = new List<Repository>();
        }

        public Guid Id { get; set; }
        
        public List<Repository> Repositories { get; set; }
    }
}
