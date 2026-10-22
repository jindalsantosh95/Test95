using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace VAA.Facade.AzureB2C.Domain.Model
{
    public class EnrolmentRequest 
    {
        public CustomerDetails CustomerDetails { get; set; } 
    }
}
