using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeniorProjectService
{
    [Serializable]
    public class ForeignNode
    {
        string name;
        ulong address;
        string addressHexRepresentation;

        public ForeignNode(ulong _address)
        {
            address = _address;
            addressHexRepresentation = "0x" + String.Format("{0:X}", address);

            name = "Unknown Node. " + addressHexRepresentation;
        }

        public ForeignNode(ulong _address, string _name)
        {
            address = _address;
            addressHexRepresentation = "0x" + String.Format("{0:X}", address);

            name = _name;
        }

        public string GetName()
        {
            return name;
        }

        public ulong GetAddress()
        {
            return address;
        }

        public string GetHexAddress()
        {
            return addressHexRepresentation;
        }
    }
}
