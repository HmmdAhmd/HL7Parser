using HL7ParserClient.Utility;
using ServiceReference;
using System.ServiceModel;
using System.ServiceModel.Channels;

var client = new IS_PortTypeClient(IS_PortTypeClient.EndpointConfiguration.BasicHttpBinding_IIS_PortType, Constants.SERVICE_ENDPOINT);

int operation = 0;

void ConsoleFault(string code, string reason, string detail)
{
    Console.WriteLine("A fault occurred:\nCode: {0}\nReason: {1}\nDetail: {2}", code, reason, detail);
}

async Task ConnectivityTest()
{
    string echo = string.Empty;
    
    while(echo == string.Empty)
    {
        Console.Write("\nEnter anything for connectivity test: ");
        echo = Console.ReadLine().Trim();
    }

    Console.WriteLine("----------------------------------------------------------------------\n");
    try
    {
        connectivityTestResponse response = await client.connectivityTestAsync(echo);

        Console.WriteLine("Service echoed back: " + response.@return);
    }
    catch (Exception ex)
    {
        Console.WriteLine("An exception occurred:\n{0}", ex.Message);
    }
}

async Task SubmitSingleMessageFromFile()
{
    string filePath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, Constants.DEFAULT_MESSAGE_FILE);
    if (File.Exists(filePath))
    {
        string username = string.Empty;
        string password = string.Empty;
        string facilityID = string.Empty;

        Console.Write("\nEnter Username: ");
        username = Console.ReadLine().Trim();

        Console.Write("Enter Password: ");
        password = Console.ReadLine().Trim();

        Console.Write("Enter Facility ID: ");
        facilityID = Console.ReadLine().Trim();

        Console.WriteLine("----------------------------------------------------------------------\n");

        string message = File.ReadAllText(filePath).Trim();
        try
        {
            submitSingleMessageResponse response = await client.submitSingleMessageAsync(username, password, facilityID, message);

            Console.WriteLine(response.@return);
        }
        catch (FaultException<soapFaultType> ex)
        {
            soapFaultType fault = ex.Detail;
            ConsoleFault(fault.Code, fault.Reason.ToString(), fault.Detail);
        }
        catch (FaultException<SecurityFaultType> ex)
        {
            SecurityFaultType fault = ex.Detail;
            ConsoleFault(fault.Code, fault.Reason.ToString(), fault.Detail);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An exception occurred:\n{0}", ex.Message);
        }
    } else
    {
        Console.WriteLine("----------------------------------------------------------------------\nError: message.txt file doesn't exist");
    }
}

int ShowMenu()
{
    Console.Clear();
    Console.Write("""
    ----------------------------------------------------------------------
    0 = Connectivity Test
    1 = Submit Single Message (Input message from console)
    2 = Submit Single Message (Read message from message.txt file)
    Any other option = Exit
    ----------------------------------------------------------------------

    Enter your option: 
    """);
    var key = Console.ReadKey();
    int value = key.KeyChar;

    return value - 48;
}

async Task PerformTask(int operation)
{
    switch (operation)
    {
        case 0:
            await ConnectivityTest();
            break;
        case 2:
            await SubmitSingleMessageFromFile();
            break;
        default:
            break;
    }
}

while (operation >= 0 && operation <= 2)
{
    operation = ShowMenu();
    if (operation >= 0 && operation <= 2)
    {
        await PerformTask(operation);
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }
}

client.Close();
