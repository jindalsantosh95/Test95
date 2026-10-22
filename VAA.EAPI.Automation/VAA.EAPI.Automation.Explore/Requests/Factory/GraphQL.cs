using Newtonsoft.Json;
using VAA.EAPI.Automation.Explore.Requests.Models.RequestModels;

namespace VAA.EAPI.Automation.Explore.Requests.Factory
{
    public static class GraphQL
    {
        #region AppointmentBooker
        public static string RetrieveAllNearByStores(string searchParameter)
        {
            string query = ReadRequestBody("RetrieveAllNearByStores.txt");

            AppointmentBooker graphRequest = new()
            {
                query = query,
                variables = new RetrieveAllNearByStoresVariables()
                {
                    searchParameter = searchParameter
                }
            };
            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string RetrieveStoreDetailsByID(string storeId, string origin)
        {
            string query = ReadRequestBody("RetrieveStoreDetailsByID.txt");

            AppointmentBooker graphRequest = new()
            {
                query = query,
                variables = new RetrieveStoreDetailsByIDVariables()
                {
                    storeId = storeId,
                    origin = origin
                }
            };
            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string RetrieveStoreSlotsByDate(string storeId, string date)
        {
            string query = ReadRequestBody("RetrieveStoreSlotsByDate.txt");

            AppointmentBooker graphRequest = new()
            {
                query = query,
                variables = new RetrieveStoreSlotsByDateVariables()
                {
                    storeId = storeId,
                    date = date
                }
            };
            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string RetrieveStoreCalendarAvailability(string storeId, int year, int month)
        {
            string query = ReadRequestBody("RetrieveStoreCalendarAvailability.txt");

            AppointmentBooker graphRequest = new()
            {
                query = query,
                variables = new RetrieveStoreCalendarAvailabilityVariables()
                {
                    storeId = storeId,
                    year = year,
                    month = month
                }
            };
            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string SaveAppointmentBooking(Bookingdetails bookingDetails)
        {
            string query = ReadRequestBody("SaveAppointmentBooking.txt");

            AppointmentBooker graphRequest = new()
            {
                query = query,
                variables = new SaveAppointmentBookingVariables()
                {
                    bookingDetails = bookingDetails
                }
            };
            return JsonConvert.SerializeObject(graphRequest);
        }
        #endregion

        #region StoreManager
        public static string StoreManagerAdminStoreById(string id)
        {
            string query = ReadRequestBody("StoreManagerAdminStoreById.txt");

            StoreManager graphRequest = new()
            {
                query = query,
                variables = new StoreManagerAdminStoreByIdVariables()
                {
                    id = id
                }
            };
            return JsonConvert.SerializeObject(graphRequest);
        }

        // This API doesn't take any input parameters, so we pass an empty object for variables.
        // Unlike the StoreManagerAdminStoreById API which fetches details for a single store, this one retrieves all store records accessible under Store Manager with Admin access.
        public static string StoreManagerAdminStores()
        {
            string query = ReadRequestBody("StoreManagerAdminStores.txt");

            StoreManager graphRequest = new()
            {
                query = query,
                variables = new { } // empty object since no inputs
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string StoreManagerOpeningHours(string storeId)
        {
            string query = ReadRequestBody("StoreManagerOpeningHours.txt");

            StoreManager graphRequest = new()
            {
                query = query,
                variables = new StoreManagerOpeningHoursVariables()
                {
                    storeId = storeId
                }
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string StoreManagerStoreById(string id)
        {
            string query = ReadRequestBody("StoreManagerStoreById.txt");

            StoreManager graphRequest = new()
            {
                query = query,
                variables = new StoreManagerStoreByIdVariables()
                {
                    id = id
                }
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string StoreManagerReplaceOpeningHours(string storeId, StoreManagerOpeningHoursReplaceRequestInput request)
        {
            string query = ReadRequestBody("StoreManagerReplaceOpeningHours.txt");

            StoreManager graphRequest = new()
            {
                query = query,
                variables = new StoreManagerReplaceOpeningHoursVariables()
                {
                    storeId = storeId,
                    request = request
                }
            };
            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string StoreManagerSlotRules(string storeId)
        {
            string query = ReadRequestBody("StoreManagerSlotRules.txt");

            StoreManager graphRequest = new()
            {
                query = query,
                variables = new StoreManagerSlotRulesVariables()
                {
                    storeId = storeId
                }
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string StoreManagerReplaceSlotRules(string storeId, SlotRuleReplaceRequestInput request)
        {
            string query = ReadRequestBody("StoreManagerReplaceSlotRules.txt");

            StoreManager graphRequest = new()
            {
                query = query,
                variables = new StoreManagerReplaceSlotRulesVariables()
                {
                    storeId = storeId,
                    request = request
                }
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string StoreManagerClosures(string storeId, string from, string to)
        {
            string query = ReadRequestBody("StoreManagerClosures.txt");

            StoreManager graphRequest = new()
            {
                query = query,
                variables = new StoreManagerClosuresVariables()
                {
                    storeId = storeId,
                    from = from,
                    to = to
                }
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string StoreManagerCreateClosure(string storeId, StoreClosureRequestInput request)
        {
            string query = ReadRequestBody("StoreManagerCreateClosure.txt");

            StoreManager graphRequest = new()
            {
                query = query,
                variables = new
                {
                    storeId,
                    request
                }
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string StoreManagerDeleteClosure(string storeId, string closureId)
        {
            string query = ReadRequestBody("StoreManagerDeleteClosure.txt");

            StoreManager graphRequest = new()
            {
                query = query,
                variables = new
                {
                    storeId,
                    closureId
                }
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string StoreManagerCancelBooking(string storeId, string bookingId)
        {
            string query = ReadRequestBody("StoreManagerCancelBooking.txt");

            StoreManager graphRequest = new()
            {
                query = query,
                variables = new StoreManagerCancelBookingVariables
                {
                    storeId = storeId,
                    bookingId = bookingId,
                    request = new BookingCancellationRequestInput
                    {
                        reason = "STORE_CLOSURE",
                        customerContactConfirmed = true
                    }
                }
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string StoreManagerSlots(string storeId, string from, string to)
        {
            string query = ReadRequestBody("StoreManagerSlots.txt");

            StoreManager graphRequest = new()
            {
                query = query,
                variables = new StoreManagerSlotsVariables
                {
                    storeId = storeId,
                    from = from,
                    to = to
                }
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string StoreManagerUpdateAdminStore(string id, StoreRequestInput request)
        {
            string query = ReadRequestBody("StoreManagerUpdateAdminStore.txt");

            StoreManager graphRequest = new()
            {
                query = query,
                variables = new StoreManagerUpdateAdminStoreVariables
                {
                    id = id,
                    request = request
                }
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string StoreManagerSlotAllocations(string storeId, string slotId, string startAt)
        {
            string query = ReadRequestBody("StoreManagerSlotAllocations.txt");

            StoreManager graphRequest = new StoreManager
            {
                query = query,
                variables = new StoreManagerSlotAllocationsVariables
                {
                    storeId = storeId,
                    slotId = slotId,
                    startAt = startAt
                }
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string StoreManagerAvailabilityRange(string storeId, string from, string to)
        {
            string query = ReadRequestBody("StoreManagerAvailabilityRange.txt");

            StoreManager graphRequest = new StoreManager
            {
                query = query,
                variables = new StoreManagerAvailabilityRangeVariables
                {
                    storeId = storeId,
                    from = from,
                    to = to
                }
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string StoreManagerUpdateSlotAllocationStatus(string storeId, string slotId, string allocationId, StoreManagerSlotAllocationStatusUpdateRequestInput request)
        {
            string query = ReadRequestBody("StoreManagerUpdateSlotAllocationStatus.txt");

            StoreManager graphRequest = new StoreManager
            {
                query = query,
                variables = new StoreManagerUpdateSlotAllocationStatusVariables
                {
                    storeId = storeId,
                    slotId = slotId,
                    allocationId = allocationId,
                    request = request
                }
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string CancelledBookings(string storeId, string date, string status)
        {
            string query = ReadRequestBody("CancelledBookings.txt");

            StoreManager graphRequest = new StoreManager
            {
                query = query,
                variables = new CancelledBookingsVariables
                {
                    storeId = storeId,
                    date = date,
                    status = status
                }
            };

            return JsonConvert.SerializeObject(graphRequest);
        }
        #endregion
        private static string ReadRequestBody(string fileName)
        {
            return File.ReadAllText(Path.Combine("Requests", "Body", fileName));
        }
    }
}
