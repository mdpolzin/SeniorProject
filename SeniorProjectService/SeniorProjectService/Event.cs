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
        Option option1;
        Option option2;

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

        [Serializable]
        internal struct Option
        {
            public string description;
            public string value;

            public bool exists;
        };

        /// <summary>
        /// Mark true if option 1 exists
        /// </summary>
        public bool Option1
        {
            get { return option1.exists; }
            set { option1.exists = value; }
        }

        /// <summary>
        /// Mark true if option 2 exists
        /// </summary>
        public bool Option2
        {
            get { return option2.exists; }
            set { option2.exists = value; }
        }

        /// <summary>
        /// Set the parameters for the first option. Only possible if option 1 exists.
        /// </summary>
        /// <param name="desc">Description of the option</param>
        /// <param name="val">Current value of the option</param>
        public void SetOption1(string desc)
        {
            if (option1.exists)
            {
                option1.description = desc;
                option1.exists = true;
            }
        }

        /// <summary>
        /// Set the parameters for the second option. Only possible if both option 1 and 2 both exist.
        /// </summary>
        /// <param name="desc">Description of the option</param>
        /// <param name="val">Current value of the option</param>
        public void SetOption2(string desc)
        {
            if (option1.exists && option2.exists)
            {
                option2.description = desc;
                option2.exists = true;
            }
        }
    }
}