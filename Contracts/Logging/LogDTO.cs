using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Logging
{
    public class LogDTO
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string MessageTemplate { get; set; }

        public string Level { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Exception { get; set; }
        public string Properties { get; set; }
    }

    public class DeleteLogDTO
    {
        public int Id { get; set; }
    }

    public class LogsQueryResponse
    {
        public List<LogDTO> Logs{ get; set; }
        public int TotalCount { get; set; }
    }
}
