using System;

namespace Emk.Domain.Entities
{
    public class License
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public string Number { get; set; }

        public bool IsValid => Status == "OK" && Message == "License Data found" && Number == "1";
    }
}
