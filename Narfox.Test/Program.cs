using Narfox.Network;
using Narfox.Test.Logging;

var writer = new ConsoleWriter();

writer.WriteTitle("Narfox Test Console");
writer.WriteParagraph("This Program is to do integration and other non-unit testing of the tools in this repo.");

DoMenu();







void DoMenu()
{
    writer.WriteMenu(
    "Testing options",
    new Dictionary<char, string>()
    {
        { 's', "Start Narfox.Network.Server" },
        { 'c', "Start Narfox.Network.Client" },
        { 'x', "Exit" }
    });

    var choice = writer.PromptForCharacter("Choose an option...");

    if(choice == 's')
    {
        RunServer();
    }
    else if(choice == 'c')
    {
        RunClient();
    }
    else
    {
        Exit();
    }
}

void RunServer()
{
    writer.ClearScreen();
    writer.WriteTitle("Running Server");
    writer.WriteParagraph("Starting the Narfox.Server, press CTRL+C to stop.");
    var server = new NetGameStateService(writer);
    server.StartServer(7777);

    while(writer.IsAwaitingCancel == false)
    {
        server.Update();
        Thread.Sleep(15);
    }

    server.Stop();
    writer.NotifyCancelHandled();
    var c = writer.PromptForCharacter("Press any key to return to the menu...");
    writer.ClearScreen();
    DoMenu();
}

void RunClient()
{
    writer.ClearScreen();
    writer.WriteTitle("Running Client");
    writer.WriteParagraph("Starting the Narfox.Client and connecting via loopback. Press CTRL+C to stop.");
    var client = new NetGameStateService(writer);
    client.StartClient();
    client.Connect("127.0.0.1", 7777);

    while(writer.IsAwaitingCancel == false)
    {
        client.Update();
        Thread.Sleep(15);
    }

    client.Stop();
    writer.NotifyCancelHandled();
    var c = writer.PromptForCharacter("Press any key to return to the menu...");
    writer.ClearScreen();
    DoMenu();
}

void Exit()
{
    Environment.Exit(0);
}
