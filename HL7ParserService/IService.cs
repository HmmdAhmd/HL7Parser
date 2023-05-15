using HL7ParserService.Models;
using HL7ParserService.Utility;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using NHapi.Base.Model;
using NHapi.Base.Parser;
using NHapi.Base.Util;
using NHapi.Base;
using NHapi.Model.V25.Message;
using System.Xml.Serialization;
using System.ComponentModel;

namespace HL7ParserService
{
    [ServiceContract]
    public interface IIS_PortType
    {
        [OperationContract]
        [FaultContract(typeof(soapFaultType), Name = "fault")]
        [FaultContract(typeof(UnsupportedOperationFaultType), Name = "UnsupportedOperationFault")]
        [XmlSerializerFormat(SupportFaults = true)]
        connectivityTestResponse connectivityTest(connectivityTestRequest request);

        [OperationContract]
        [FaultContract(typeof(soapFaultType), Name = "fault")]
        [FaultContract(typeof(SecurityFaultType), Name = "SecurityFault")]
        [FaultContract(typeof(MessageTooLargeFaultType), Name = "MessageTooLargeFault")]
        [XmlSerializerFormat(SupportFaults = true)]
        submitSingleMessageResponse submitSingleMessage(submitSingleMessageRequest request);
    }

    public class IS_PortType : IIS_PortType
    {
        private readonly IMongoCollection<HL7Message> _collection;

        public IS_PortType(IOptions<MongoDBSettings> settings)
        {
            _collection = new MongoClient(settings.Value.ConnectionString)
                .GetDatabase(settings.Value.DatabaseName)
                .GetCollection<HL7Message>(settings.Value.CollectionName);
        }

        private IMessage CreateACK(string fieldSeparator, string encodingChar, string sendingApp, string sendingFacility, string receivingApp, string receivingFacility, string controlId, string processingId, string versionId, string ackCode, string txtMsg)
        {
            IMessage ack = new ACK();
            Terser ackTerser = new Terser(ack);
            ackTerser.Set(Constants.MSH1_SEGMENT, fieldSeparator);
            ackTerser.Set(Constants.MSH2_SEGMENT, encodingChar);
            ackTerser.Set(Constants.MSH3_SEGMENT, sendingApp);
            ackTerser.Set(Constants.MSH4_SEGMENT, sendingFacility);
            ackTerser.Set(Constants.MSH5_SEGMENT, receivingApp);
            ackTerser.Set(Constants.MSH6_SEGMENT, receivingFacility);
            ackTerser.Set(Constants.MSH7_SEGMENT, DateTime.Now.ToString(Constants.DATE_FORMAT));
            ackTerser.Set(Constants.MSH9_SEGMENT, Constants.MESSAGE_TYPE);
            ackTerser.Set(Constants.MSH10_SEGMENT, controlId);
            ackTerser.Set(Constants.MSH11_SEGMENT, processingId);
            ackTerser.Set(Constants.MSH12_SEGMENT, versionId);
            ackTerser.Set(Constants.MSA1_SEGMENT, ackCode);
            ackTerser.Set(Constants.MSA2_SEGMENT, controlId);
            ackTerser.Set(Constants.MSA3_SEGMENT, txtMsg);
            return ack;
        }

        private string GenerateRandomId()
        {
            var random = new Random();
            //To generate random id of length 8
            return random.Next(10000000, 100000000).ToString();
        }

        private void StoreHL7(submitSingleMessageRequest request)
        {
            HL7Message hl7 = new HL7Message
            {
                Username = request.username,
                Password = request.password,
                FacilityId = request.facilityID,
                Message = request.hl7Message,
                CreatedDate = DateTime.Now,
            };
            _collection.InsertOne(hl7);
        }

        public submitSingleMessageResponse submitSingleMessage(submitSingleMessageRequest request)
        {
            string message = request.hl7Message.Trim();
            var parser = new PipeParser();
            IMessage ack = null;
            try
            {
                IMessage hl7 = parser.Parse(message);

                var msh = hl7.GetStructure(Constants.MSH_STRUCTURE);
                Terser terser = new Terser(hl7);

                string receivingApp = terser.Get(Constants.MSH5_SEGMENT);
                string receivingFacility = terser.Get(Constants.MSH6_SEGMENT);
                string controlId = terser.Get(Constants.MSH10_SEGMENT);
                if (controlId.IsNullOrEmpty())
                {
                    controlId = GenerateRandomId();
                }
                string processingId = terser.Get(Constants.MSH11_SEGMENT);
                string versionId = terser.Get(Constants.MSH12_SEGMENT);

                StoreHL7(request);

                ack = CreateACK(
                    terser.Get(Constants.MSH1_SEGMENT),
                    terser.Get(Constants.MSH2_SEGMENT),
                    receivingApp.IsNullOrEmpty() ? Constants.DEFAULT_SENDING_APP : receivingApp,
                    receivingFacility.IsNullOrEmpty() ? Constants.DEFAULT_SENDING_FACILITY : receivingFacility,
                    terser.Get(Constants.MSH3_SEGMENT),
                    terser.Get(Constants.MSH4_SEGMENT),
                    controlId,
                    processingId.IsNullOrEmpty() ? Constants.DEFAULT_PROCESSING_ID : processingId,
                    versionId.IsNullOrEmpty() ? Constants.DEFAULT_VERSION_ID : versionId,
                    Constants.SUCCESS_ACK_CODE,
                    Constants.SUCCESS_TXT_MSG);
            }
            catch (HL7Exception)
            {
                ack = CreateACK(
                    Constants.FIELD_SEPARATOR,
                    Constants.DEFAULT_ENCODING_CHAR,
                    Constants.DEFAULT_SENDING_APP,
                    Constants.DEFAULT_SENDING_FACILITY,
                    String.Empty,
                    String.Empty,
                    GenerateRandomId(),
                    Constants.DEFAULT_PROCESSING_ID,
                    Constants.DEFAULT_VERSION_ID,
                    Constants.ERROR_ACK_CODE,
                    Constants.ERROR_TXT_MSG);
            }

            return new submitSingleMessageResponse { @return = parser.Encode(ack).Replace(Constants.CARRIAGE_RETURN, Environment.NewLine) };
        }

        public connectivityTestResponse connectivityTest(connectivityTestRequest request)
        {
            throw new NotImplementedException();
        }
    }

    [XmlType]
    public class soapFaultType
    {
        string codeField;
        string reasonField;
        string detailField;

        [XmlElement(DataType = "integer", Order = 0)]
        public string Code
        {
            get { return codeField; }
            set { codeField = value; }
        }

        [XmlElement(Order = 1)]
        public string Reason
        {
            get { return reasonField; }
            set { reasonField = value; }
        }

        [XmlElement(Order = 2)]
        public string Detail
        {
            get { return detailField; }
            set { detailField = value; }
        }
    }

    [XmlType]
    public class UnsupportedOperationFaultType
    {
        private string codeField;
        private object reasonField;
        private string detailField;

        [XmlElement(DataType = "integer", Order = 0)]
        public string Code
        {
            get { return codeField; }
            set { codeField = value; }
        }

        [XmlElement(Order = 1)]
        public object Reason
        {
            get { return reasonField; }
            set { reasonField = value; }
        }

        [XmlElement(Order = 2)]
        public string Detail
        {
            get { return detailField; }
            set { detailField = value; }
        }
    }

    [XmlType]
    public class SecurityFaultType
    {
        private string codeField;
        private object reasonField;
        private string detailField;

        [XmlElement(DataType = "integer", Order = 0)]
        public string Code
        {
            get { return codeField; }
            set { codeField = value; }
        }

        [XmlElement(Order = 1)]
        public object Reason
        {
            get { return reasonField; }
            set { reasonField = value; }
        }

        [XmlElement(Order = 2)]
        public string Detail
        {
            get { return detailField; }
            set { detailField = value; }
        }
    }

    [XmlType]
    public class MessageTooLargeFaultType
    {
        private string codeField;
        private object reasonField;
        private string detailField;

        [XmlElement(DataType = "integer", Order = 0)]
        public string Code
        {
            get { return codeField; }
            set { codeField = value; }
        }

        [XmlElement(Order = 1)]
        public object Reason
        {
            get { return reasonField; }
            set { reasonField = value; }
        }

        [XmlElement(Order = 2)]
        public string Detail
        {
            get { return detailField; }
            set { detailField = value; }
        }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [MessageContract(WrapperName = "connectivityTest",IsWrapped =true)]
    public class connectivityTestRequest
    {

        [MessageBodyMember(Order = 0)]
        [XmlElement(IsNullable = true)]
        public string echoBack;

        public connectivityTestRequest() { }

        public connectivityTestRequest(string echoBack)
        {
            this.echoBack = echoBack;
        }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [MessageContract(WrapperName = "connectivityTestResponse", IsWrapped = true)]
    public class connectivityTestResponse
    {

        [MessageBodyMember(Order = 0)]
        [XmlElement(IsNullable = true)]
        public string @return;

        public connectivityTestResponse() { }

        public connectivityTestResponse(string @return)
        {
            this.@return = @return;
        }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [MessageContract(WrapperName = "submitSingleMessage", IsWrapped = true)]
    public class submitSingleMessageRequest
    {
        [MessageBodyMember(Order = 0)]
        [XmlElement(IsNullable = true)]
        public string username;

        [MessageBodyMember(Order = 1)]
        [XmlElement(IsNullable = true)]
        public string password;

        [MessageBodyMember(Order = 2)]
        [XmlElement(IsNullable = true)]
        public string facilityID;

        [MessageBodyMember(Order = 3)]
        [XmlElement(IsNullable = true)]
        public string hl7Message;

        public submitSingleMessageRequest() { }

        public submitSingleMessageRequest(string username, string password, string facilityID, string hl7Message)
        {
            this.username = username;
            this.password = password;
            this.facilityID = facilityID;
            this.hl7Message = hl7Message;
        }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [MessageContract(WrapperName = "submitSingleMessageResponse", IsWrapped = true)]
    public class submitSingleMessageResponse
    {

        [MessageBodyMember(Order = 0)]
        [XmlElement(IsNullable = true)]
        public string @return;

        public submitSingleMessageResponse() { }

        public submitSingleMessageResponse(string @return)
        {
            this.@return = @return;
        }
    }
}
