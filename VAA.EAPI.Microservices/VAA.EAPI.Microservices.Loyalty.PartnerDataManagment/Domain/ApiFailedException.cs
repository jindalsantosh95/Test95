using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain
{
    public class ApiFailedException : Exception
    {
        /// <summary>
        /// ApiFailedException
        /// </summary>
        public ApiFailedException()
        {
        }

        /// <summary>
        /// ApiFailedException 
        /// </summary>
        /// <param name="message"></param>
        public ApiFailedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// ApiFailedException
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public ApiFailedException(string message, Exception ex)
            : base(message, ex)
        {
        }
    }
}
