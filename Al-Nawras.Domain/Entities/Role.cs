using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Domain.Entities
{
    public class Role
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string? Description { get; private set; }

        private Role() { }

        public Role(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}
