using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Errors
{
    public class CustomErrorResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }
}
