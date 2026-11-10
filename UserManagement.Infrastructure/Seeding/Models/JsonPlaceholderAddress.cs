using System;
using System.Collections.Generic;
using System.Text;

namespace UserManagement.Infrastructure.Seeding.Models
{
    public class JsonPlaceholderAddress
    {
        public string Street { get; init; } = string.Empty;
        public string Suite { get; init; } = string.Empty;
        public string City { get; init; } = string.Empty;
        public string Zipcode { get; init; } = string.Empty;
        public JsonPlaceholderGeo? Geo { get; init; }
    }
}
