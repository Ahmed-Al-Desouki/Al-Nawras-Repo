using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Domain.Entities
{
    public class Client
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }
        public string Phone { get; private set; }
        public string Country { get; private set; }
        public string CompanyName { get; private set; }
        public int? AssignedSalesUserId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public User AssignedSalesUser { get; private set; }
        public ICollection<Deal> Deals { get; private set; } = new List<Deal>();

        private Client() { }

        public Client(string name, string email, string phone, string country,
            string companyName, int? assignedSalesUserId = null)
        {
            Id = Guid.NewGuid();
            Name = name;
            Email = email;
            Phone = phone;
            Country = country;
            CompanyName = companyName;
            AssignedSalesUserId = assignedSalesUserId;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string name, string email, string phone, string country, string companyName)
        {
            Name = name;
            Email = email;
            Phone = phone;
            Country = country;
            CompanyName = companyName;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
