using System;
using System.Collections.Generic;
using System.Text;

namespace Cryptogram
{
    class CryptogramDBDTO
    {
        public Guid id { get; set; } = Guid.Empty;
        public int accountId { get; set; }
        public DateTime expirationDateTime { get; set; }
        public int retrivalCount { get; set; }
        public bool encrypted { get; set; }
        public string o { get; set; }
    }
}
