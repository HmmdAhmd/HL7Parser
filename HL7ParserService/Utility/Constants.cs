namespace HL7ParserService.Utility
{
    public class Constants
    {
        public const string DATE_FORMAT = "yyyyMMddHHmmss";
        public const string MESSAGE_TYPE = "ACK";
        public const string FIELD_SEPARATOR = "|";
        public const string DEFAULT_ENCODING_CHAR = "^~\\&";
        public const string DEFAULT_SENDING_APP = "HelloService";
        public const string DEFAULT_SENDING_FACILITY = "HelloFacility";
        public const string DEFAULT_PROCESSING_ID = "T";
        public const string DEFAULT_VERSION_ID = "2.5";
        public const string SUCCESS_ACK_CODE = "AA";
        public const string ERROR_ACK_CODE = "AE";
        public const string SUCCESS_TXT_MSG = "Message is received and stored successfully";
        public const string ERROR_TXT_MSG = "Message is invalid";
        public const string MSH_STRUCTURE = "MSH";
        public const string MSH1_SEGMENT = "/MSH-1";
        public const string MSH2_SEGMENT = "/MSH-2";
        public const string MSH3_SEGMENT = "/MSH-3";
        public const string MSH4_SEGMENT = "/MSH-4";
        public const string MSH5_SEGMENT = "/MSH-5";
        public const string MSH6_SEGMENT = "/MSH-6";
        public const string MSH7_SEGMENT = "/MSH-7";
        public const string MSH9_SEGMENT = "/MSH-9";
        public const string MSH10_SEGMENT = "/MSH-10";
        public const string MSH11_SEGMENT = "/MSH-11";
        public const string MSH12_SEGMENT = "/MSH-12";
        public const string MSA1_SEGMENT = "/MSA-1";
        public const string MSA2_SEGMENT = "/MSA-2";
        public const string MSA3_SEGMENT = "/MSA-3";
        public const string CARRIAGE_RETURN = "\r";
    }
}
