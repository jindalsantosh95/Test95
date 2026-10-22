using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Runtime.InteropServices;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS;

namespace VAA.EAPI.NDC.Order.Validations.Common
{
    public class PaymentResponseValidation
    {
        public static Response<T> ValidatePaymentResponse<T>(PaymentRS paymentRS, IHostEnvironment hostEnvironment, [Optional] bool isCardPayment)
        {
            
            var errors = paymentRS.Errors.Select(error =>
            {
                if (error?.Code == "G066")
                {
                    var code = isCardPayment ? error.Message.Split(';').FirstOrDefault(part => part.Contains("CPDCode"))?.Split(':').Last().Trim() : error?.DeveloperMessage.Split(";")[0].Split(":")[1].Trim();
                    var subcode = isCardPayment ? error.Message.Split(';').FirstOrDefault(part => part.Contains("CPDSubCode"))?.Split(':').Last().Trim() : error?.DeveloperMessage.Split(";")[1].Split(":")[1].Trim();

                    if (code == "21")
                    {
                        return new ErrorDetail()
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "708_5"
                        };
                    }

                    else if (code == "23")
                    {
                        return new ErrorDetail()
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "708_6"
                        };
                    }

                    else if (code == "24")
                    {
                        return new ErrorDetail()
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "708_7"
                        };
                    }
                    
                    else if (code == "400")
                    {
                        return new ErrorDetail()
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "913_7"
                        };
                    }

                    else if (code  == "2011")
                    {
                        return new ErrorDetail()
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "708_19"
                        };
                    }

                    else if(code == "92")
                    {
                        return new ErrorDetail()
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "708_20"
                        };
                    }

                    else if (subcode == "2010102")
                    {
                        return new ErrorDetail()
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "708_5"
                        };
                    }

                    else if (subcode == "2010105")
                    {
                        return new ErrorDetail()
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "708_8"
                        };
                    }

                    else if (subcode == "2010109")
                    {
                        return new ErrorDetail()
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "708_9"
                        };
                    }

                    else if (subcode == "2010112")
                    {
                        return new ErrorDetail()
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "708_10"
                        };
                    }

                    else if (subcode == "2010113")
                    {
                        return new ErrorDetail()
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "708_11"
                        };
                    }

                    else if (subcode == "2010203")
                    {
                        return new ErrorDetail()
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "708_12"
                        };
                    }

                    else if (subcode == "2010204")
                    {
                        return new ErrorDetail()
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "708_17"
                        };
                    }

                    else if (subcode == "2010206")
                    {
                        return new ErrorDetail()
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "708_13"
                        };
                    }

                    else if (subcode == "2010207")
                    {
                        return new ErrorDetail()
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "708_14"
                        };
                    }

                    else if (subcode == "2010208")
                    {
                        return new ErrorDetail()
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "708_15"
                        };
                    }

                    else if (subcode == "2010304")
                    {
                        return new ErrorDetail()
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "708_18"
                        };
                    }

                    else if (subcode == "2010411")
                    {
                        return new ErrorDetail()
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "708_16"
                        };
                    }

                    else if (code == "3")
                    {
                        return new ErrorDetail()
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "913_5"
                        };
                    }

                    else if (code == "56")
                    {
                        return new ErrorDetail()
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "913_6"
                        };
                    }


                    return new ErrorDetail()
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "708_4"
                    };
                }
                else if (error?.Code == "G008")
                {
                    return new ErrorDetail()
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "911_22"
                    };
                }
                else if (error?.Code == "G009")
                {
                    return new ErrorDetail()
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "911_22"
                    };
                }
                else
                {
                    return new ErrorDetail()
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "708_4"
                    };
                }
            });

            return ErrorHelper<T>.GetErrors(errors, hostEnvironment);
        }

    }
}
