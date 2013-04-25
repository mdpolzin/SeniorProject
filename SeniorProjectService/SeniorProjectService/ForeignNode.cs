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
        string addressHexRepresentation;
        List<Event> events = new List<Event>();

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

        public void SetAlias(string _alias)
        {
            alias = _alias;
        }

        public void SetBrand(string _brand)
        {
            brand = _brand;
        }

        public void SetName(string _name)
        {
            name = _name;
        }
    }
}
