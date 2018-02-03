using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace KavaDocsUserManager.Business.Models
{
    public class Contributors
    {
        public Guid Id { get; set; }

        [ForeignKey("User")] public Guid UserId { get; set; }

        [ForeignKey("Repository")] public Guid RepositoryId { get; set; }

        public User User { get; set; }

        public Repository Repository { get; set; }
    }
}
