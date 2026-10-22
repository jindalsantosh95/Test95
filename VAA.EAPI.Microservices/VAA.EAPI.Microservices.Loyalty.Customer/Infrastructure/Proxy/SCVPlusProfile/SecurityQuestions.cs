using System.IO;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Proxy.SCVPlusProfile
{
    public partial class SecurityQuestions
    {
        public SecurityQuestions(int? question = default, string answer = default)
        {
            // to ensure "question" is required (not null)
            if (question == null)
            {
                throw new InvalidDataException("question is a required property for SecurityQuestions and cannot be null");
            }
            else
            {
                Question = question;
            }
            // to ensure "answer" is required (not null)
            if (answer == null)
            {
                throw new InvalidDataException("answer is a required property for SecurityQuestions and cannot be null");
            }
            else
            {
                Answer = answer;
            }
        }

        public int? Question { get; set; }
        public string Answer { get; set; }


    }
}
