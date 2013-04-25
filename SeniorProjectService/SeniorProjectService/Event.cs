using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeniorProjectService
{
    [Serializable]
    public class Event
    {
        private string name;
        private string description;
        private bool triggerable;
        private int count;

        private int id;

        public Event(string _name, string _description, int _id, bool _triggerable)
        {
            name = _name;
            description = _description;
            id = _id;
            triggerable = _triggerable;
        }

        public void Throw()
        {
            count++;
        }

        public void ResetCount()
        {
            count = 0;
        }

        public int Count
        {
            get { return this.count; }
        }

        public string Description
        {
            get { return description; }
        }

        public int ID
        {
            set { this.id = value; }
            get { return this.id; }
        }

        public string Name
        {
            get { return name; }
        }
        
    }
}