using ServiceReference;

var client = new ServiceClient(ServiceClient.EndpointConfiguration.BasicHttpBinding_IService, "https://localhost:5001/HL7Parser");

string fileName = "message.txt";
string filePath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, fileName);
if (File.Exists(filePath))
{
    string message = File.ReadAllText(filePath);
    var response = await client.ParseHL7Async(message);
    Console.WriteLine("Response from endpoint:\n{0}\nPress any key to close...", response);
    Console.ReadKey();
}

client.Close();
