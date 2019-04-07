using System.Collections.Generic;

namespace ProductShop.DTOs.Export
{
    public class UsersWithProductsDto
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public ICollection<SoldProductsDto> SoldProducts { get; set; }
    }
}