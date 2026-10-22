namespace VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Infrastructure.Domain.Entities
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
