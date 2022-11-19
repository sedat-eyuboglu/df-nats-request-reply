using Microsoft.AspNetCore.Mvc;
using NATS.Client;

[ApiController]
[Route("[controller]")]
public class PrintReceiptController : ControllerBase
{
    private readonly NatsService nats;
    private readonly IConfiguration configuration;
    public PrintReceiptController(NatsService nats, IConfiguration configuration)
    {
        this.nats = nats;
        this.configuration = configuration;
    }

    [HttpPost("Print")]
    public async Task<ActionResult<PrintResponse>> PrintAsync(PrintRequest request)
    {
        if(nats.IsConnected == false)
        {
            throw new Exception("Can not continue while connection to the broker is not established. Check logs to inspect the case more detailed.");
        }

        request.receiptId = DateTimeOffset.UtcNow.ToString();

        var waitTimeOut = configuration.GetValue<int>("ReplyTimeout", 5000);
        
        var requestJson = System.Text.Json.JsonSerializer.Serialize(request);
        var requestData = System.Text.Encoding.UTF8.GetBytes(requestJson);

        Msg reply;
        try
        {
            reply = await nats.Connection.RequestAsync($"store.receipt.s{request.storeId}.c{request.clientId}", requestData, waitTimeOut);
        }
        catch(NATSNoRespondersException)
        {
            throw new Exception("No printer available. Check print service. I need some printer to listen my requests!");
        }
        
        var replyJson = System.Text.Encoding.UTF8.GetString(reply.Data);
        var printResponse = System.Text.Json.JsonSerializer.Deserialize<PrintResponse>(replyJson);

        return printResponse!;
    }
}