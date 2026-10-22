using System;
using VAA.Facade.EnrolmentFacade.Domain;
using VAA.Facade.EnrolmentFacade.Domain.Events;
using VAA.Facade.EnrolmentFacade.Helper;

namespace VAA.Facade.EnrolmentFacade.Mapper
{
    public class RequestMapper
    {
        public static EventGridTopicEvent RequestDataMapper(EnrolmentRequest enrolmentRequest)
        {
            return
             new EventGridTopicEvent
             {
                 Id = Convert.ToString(Guid.NewGuid()),
                 Subject = ConfigReader.GetConfigurationSettings(Constants.Constants.CreateFacadeEventSubject),
                 Type = ConfigReader.GetConfigurationSettings(Constants.Constants.EventType),
                 Data = enrolmentRequest,
                 Source = Constants.Constants.EventSource,
                 Specversion = Constants.Constants.DataVersion
             };

        }
    }
}
