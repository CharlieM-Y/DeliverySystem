using System.Xml;
using System;

namespace DeliverySystem.Models
{
    public class Account
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string? Name { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
    }
}
