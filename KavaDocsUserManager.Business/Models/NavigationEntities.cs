using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace KavaDocsUserManager.Business.Models
{
    //public class UserOrganization
    //{
    //    public Guid UserId { get; set; }
    //    public User User { get; set; }

    //    public Guid OrganizationId { get; set; }
    //    //public Organization Organization { get; set; }
    //}

    public class UserRepository
    {
        public UserRepository()
        {
            Id = Guid.NewGuid();
        }


        public Guid Id { get; set; }

        [ForeignKey("User")]

        public Guid UserId { get; set; }
        public User User { get; set; }

        [ForeignKey("Repository")]
        public Guid RepositoryId { get; set; }

        public Repository Respository { get; set; }
    }

    public class RepositoryContributor
    {
        public RepositoryContributor()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }

        public Guid RepositoryId { get; set; }
        public Guid Respository { get; set; }


        public Guid ContributorId { get; set; }

        public Guid Contributor { get; set; }

    }

}
