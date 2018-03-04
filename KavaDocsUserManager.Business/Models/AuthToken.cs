using System;
using System.Collections.Generic;
using System.Text;

namespace KavaDocsUserManager.Business.Models
{

    /// <summary>
    /// Maps a token ID to a User Id
    /// </summary>
    public class AuthToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public DateTime Entered { get; set; }
    }
}
