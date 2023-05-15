using ServiceReference;

var client = new IS_PortTypeClient(IS_PortTypeClient.EndpointConfiguration.BasicHttpBinding_IIS_PortType, "https://localhost:8283/wsdl-demo");

string fileName = "message.txt";
string filePath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, fileName);
if (File.Exists(filePath))
{
    string username = "johndoe";
    string password = "johnDoe!";
    string facilityID = "12345678";
    string message = File.ReadAllText(filePath);
    submitSingleMessageResponse response = await client.submitSingleMessageAsync(username, password, facilityID, message);
    Console.WriteLine("Response from endpoint:\n{0}\nPress any key to close...", response.@return);
    Console.ReadKey();
}

client.Close();
