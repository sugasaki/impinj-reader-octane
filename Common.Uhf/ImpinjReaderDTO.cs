using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Uhf
{
    public class ImpinjReaderDTO
    {
        // リーダー名
        // ex, "Dock Door #1 Reader".
        public string? Name { get; set; } = null;

        // Contains the IP address or hostname of the reader.
        public string? Address { get; set; } = null;

        public ImpinjReaderDTO(string name, string address)
        {
            Name = name;
            Address = address;
        }
    }
}