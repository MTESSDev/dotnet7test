using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FRW.PR.Extra
{
    public class ClaimExpiration
    {
        public string Val { get; set; }
        public DateTimeOffset Exp { get; set; }
    }
}
