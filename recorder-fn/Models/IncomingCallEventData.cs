// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class From
    {
        public string kind { get; set; }
        public string rawId { get; set; }
        public PhoneNumber phoneNumber { get; set; }
    }

    public class PhoneNumber
    {
        public string value { get; set; }
    }

    public class IncomingCallEventData
    {
        public To to { get; set; }
        public From from { get; set; }
        public string serverCallId { get; set; }
        public string callerDisplayName { get; set; }
        public string incomingCallContext { get; set; }
        public string correlationId { get; set; }
    }

    public class To
    {
        public string kind { get; set; }
        public string rawId { get; set; }
        public PhoneNumber phoneNumber { get; set; }
    }

