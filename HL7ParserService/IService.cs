using HL7ParserService.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using NHapi.Base.Model;
using NHapi.Base.Parser;
using NHapi.Base.Util;
using NHapi.Base;
using NHapi.Model.V25.Message;

namespace HL7ParserService
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        string ParseHL7(string message);
    }

    public class Service : IService
    {
        private readonly IMongoCollection<HL7Message> _collection;

        public Service(IOptions<MongoDBSettings> settings)
        {
            _collection = new MongoClient(settings.Value.ConnectionString)
                .GetDatabase(settings.Value.DatabaseName)
                .GetCollection<HL7Message>(settings.Value.CollectionName);
        }

        IMessage CreateACK(string fieldSeparator, string encodingChar, string sendingApp, string sendingFacility, string receivingApp, string receivingFacility, string controlId, string processingId, string versionId, string ackCode, string txtMsg)
        {
            const string DATE_FORMAT = "yyyyMMddHHmmss";
            const string MESSAGE_TYPE = "ACK";

            IMessage ack = new ACK();
            Terser ackTerser = new Terser(ack);
            ackTerser.Set("/MSH-1", fieldSeparator);
            ackTerser.Set("/MSH-2", encodingChar);
            ackTerser.Set("/MSH-3", sendingApp);
            ackTerser.Set("/MSH-4", sendingFacility);
            ackTerser.Set("/MSH-5", receivingApp);
            ackTerser.Set("/MSH-6", receivingFacility);
            ackTerser.Set("/MSH-7", DateTime.Now.ToString(DATE_FORMAT));
            ackTerser.Set("/MSH-9", MESSAGE_TYPE);
            ackTerser.Set("/MSH-10", controlId);
            ackTerser.Set("/MSH-11", processingId);
            ackTerser.Set("/MSH-12", versionId);
            ackTerser.Set("/MSA-1", ackCode);
            ackTerser.Set("/MSA-2", controlId);
            ackTerser.Set("/MSA-3", txtMsg);
            return ack;
        }
        private void StoreHL7(string message)
        {
            HL7Message hl7 = new HL7Message
            {
                Message = message,
                CreatedDate = DateTime.Now,
            };
            _collection.InsertOne(hl7);
        }

        public string ParseHL7(string message)
        {
            const string FIELD_SEPARATOR = "|";
            const string DEFAULT_ENCODING_CHAR = "^~\\&";
            const string DEFAULT_SENDING_APP = "HelloService";
            const string DEFAULT_SENDING_FACILITY = "HelloFacility";
            const string DEFAULT_PROCESSING_ID = "T";
            const string DEFAULT_VERSION_ID = "2.5";
            const string SUCCESS_ACK_CODE = "AA";
            const string ERROR_ACK_CODE = "AE";
            const string SUCCESS_TXT_MSG = "Message is received and stored successfully";
            const string ERROR_TXT_MSG = "Message is invalid";

            message = message.Trim();
            var parser = new PipeParser();
            var random = new Random();
            IMessage ack = null;
            try
            {
                IMessage hl7 = parser.Parse(message);

                var msh = hl7.GetStructure("MSH");
                Terser terser = new Terser(hl7);

                string receivingApp = terser.Get("/MSH-5");
                string receivingFacility = terser.Get("/MSH-6");
                string controlId = terser.Get("/MSH-10");
                if (controlId.IsNullOrEmpty())
                {
                    controlId = random.Next(10000000, 100000000).ToString();
                }
                string processingId = terser.Get("/MSH-11");
                string versionId = terser.Get("/MSH-12");

                string fileName = String.Format("Messages/{0}.txt", controlId);
                StoreHL7(message);

                ack = CreateACK(
                    terser.Get("/MSH-1"),
                    terser.Get("/MSH-2"),
                    receivingApp.IsNullOrEmpty() ? DEFAULT_SENDING_APP : receivingApp,
                    receivingFacility.IsNullOrEmpty() ? DEFAULT_SENDING_FACILITY : receivingFacility,
                    terser.Get("/MSH-3"),
                    terser.Get("/MSH-4"),
                    controlId,
                    processingId.IsNullOrEmpty() ? DEFAULT_PROCESSING_ID : processingId,
                    versionId.IsNullOrEmpty() ? DEFAULT_VERSION_ID : versionId,
                    SUCCESS_ACK_CODE,
                    SUCCESS_TXT_MSG);
            }
            catch (HL7Exception)
            {
                ack = CreateACK(
                    FIELD_SEPARATOR,
                    DEFAULT_ENCODING_CHAR,
                    DEFAULT_SENDING_APP,
                    DEFAULT_SENDING_FACILITY,
                    String.Empty,
                    String.Empty,
                    random.Next(10000000, 100000000).ToString(),
                    DEFAULT_PROCESSING_ID,
                    DEFAULT_VERSION_ID,
                    ERROR_ACK_CODE,
                    ERROR_TXT_MSG);
            }

            return parser.Encode(ack).Replace("\r", Environment.NewLine);
        }
    }
}
