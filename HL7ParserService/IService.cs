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
            IMessage ack = new ACK();
            Terser ackTerser = new Terser(ack);
            ackTerser.Set(Constants.MSH1_SEGMENT, fieldSeparator);
            ackTerser.Set(Constants.MSH2_SEGMENT, encodingChar);
            ackTerser.Set(Constants.MSH3_SEGMENT, sendingApp);
            ackTerser.Set(Constants.MSH4_SEGMENT, sendingFacility);
            ackTerser.Set(Constants.MSH5_SEGMENT, receivingApp);
            ackTerser.Set(Constants.MSH6_SEGMENT, receivingFacility);
            ackTerser.Set(Constants.MSH7_SEGMENT, DateTime.Now.ToString(DATE_FORMAT));
            ackTerser.Set(Constants.MSH9_SEGMENT, MESSAGE_TYPE);
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
            message = message.Trim();
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

                StoreHL7(message);

                ack = CreateACK(
                    terser.Get(Constants.MSH1_SEGMENT),
                    terser.Get(Constants.MSH2_SEGMENT),
                    receivingApp.IsNullOrEmpty() ? DEFAULT_SENDING_APP : receivingApp,
                    receivingFacility.IsNullOrEmpty() ? DEFAULT_SENDING_FACILITY : receivingFacility,
                    terser.Get(Constants.MSH3_SEGMENT),
                    terser.Get(Constants.MSH4_SEGMENT),
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
                    GenerateRandomId(),
                    DEFAULT_PROCESSING_ID,
                    DEFAULT_VERSION_ID,
                    ERROR_ACK_CODE,
                    ERROR_TXT_MSG);
            }

            return parser.Encode(ack).Replace(Constants.CARRIAGE_RETURN, Environment.NewLine);
        }
    }
}
