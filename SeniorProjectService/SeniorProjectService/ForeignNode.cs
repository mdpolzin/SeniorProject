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
        string alias;
        string brand;
        ulong address;
        byte version = 255;
        string addressHexRepresentation;
        List<Event> events = new List<Event>();
        bool registered = false;

        int nodeID;

        public ForeignNode(ulong _address)
        {
            address = _address;
            addressHexRepresentation = "0x" + String.Format("{0:X}", address);

            name = "Unknown Node. " + addressHexRepresentation;
            alias = name;
        }

        public ForeignNode(ulong _address, string _name)
        {
            address = _address;
            addressHexRepresentation = "0x" + String.Format("{0:X}", address);

            name = _name;
            alias = name;
        }

        /// <summary>
        /// Adds an event to this Foreign Node's Event list. Only adds it to the list if the event ID does not already exist in the Event list.
        /// </summary>
        /// <param name="e">Event to add</param>
        public void AddEvent(Event e)
        {
            if (!events.Any<Event>(exist => exist.ID == e.ID))
                events.Add(e);
        }

        public ulong GetAddress()
        {
            return address;
        }

        public string GetAlias()
        {
            return alias;
        }

        public string GetBrand()
        {
            return brand;
        }

        public string GetHexAddress()
        {
            return addressHexRepresentation;
        }

        public List<Event> GetEvents()
        {
            return events;
        }

        public string GetName()
        {
            return name;
        }

        public int GetNodeID()
        {
            return nodeID;
        }

        public bool GetRegistered()
        {
            return registered;
        }

        public byte GetVersion()
        {
            return version;
        }

        public void ResetNode()
        {
            name = "";
            brand = "";
            events = new List<Event>();
            registered = false;
        }

        public void SetAddress(ulong addr)
        {
            address = addr;
        }

        public void SetAlias(string _alias)
        {
            alias = _alias;
        }

        public void SetBrand(string _brand)
        {
            brand = _brand;
        }

        public void SetHexAddress(string hexAddr)
        {
            addressHexRepresentation = hexAddr;
        }

        public void SetName(string _name)
        {
            name = _name;
        }

        public void SetNodeID(int node)
        {
            nodeID = node;
        }

        public void SetRegistered(bool reg)
        {
            registered = reg;
        }

        public void SetVersion(byte val)
        {
            version = val;
        }

        public void ThrowEvent(int eventID, string op1, string op2)
        {
            Event e = events.Single<Event>(temp => temp.ID == eventID);
            e.Throw();

            e.SetOption1Value(op1);
            e.SetOption2Value(op2);
        }
    }
}
