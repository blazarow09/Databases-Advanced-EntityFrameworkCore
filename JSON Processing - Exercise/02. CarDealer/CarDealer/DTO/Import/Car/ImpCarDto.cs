using System;
using System.Collections.Generic;
using System.Text;

namespace CarDealer.DTO.Import.Car
{
    public class ImpCarDto
    {
         public string Make { get; set; }
        
         public string Model { get; set; }
        
         public long TravelledDistance { get; set; }

         public List<int> PartsId = new List<int>();
    }
}
