using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KavaDocsUserManager.Business.Models
{
    [DebuggerDisplay("{Name}")]
    public class Role
    {
        public Role()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }

        public string Name { get; set; }

        public int Level { get; set; } = 1;

        public bool IsGlobalAdmin { get; set; }

        public bool IsRepoAdmin { get; set; }
    }
}
