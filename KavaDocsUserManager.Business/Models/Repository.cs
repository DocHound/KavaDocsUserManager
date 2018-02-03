using System;
using System.Collections.Generic;
using System.Text;

namespace KavaDocsUserManager.Business.Models
{
    public class Repository
    {
        public Repository()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
    }
}
