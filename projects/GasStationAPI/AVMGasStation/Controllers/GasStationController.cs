using Algorand;
using Algorand.Algod.Model;
using Algorand.Algod.Model.Transactions;
using AlgorandAuthenticationV2;
using AVMGasStation.BusinessControllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MsgPack.Serialization;

namespace AVMGasStation.Controllers;
[Authorize]
[ApiController]
[Route("/v1/")]
public class GasStationController : ControllerBase
{
  private readonly ILogger<GasStationController> _logger;
  private readonly GasStation _gasStation;
  private readonly IOptions<AlgorandAuthenticationOptionsV2> _options;

  public GasStationController(
      ILogger<GasStationController> logger,
      GasStation gasStation,
      IOptions<AlgorandAuthenticationOptionsV2> options
      )
  {
    _logger = logger;
    _gasStation = gasStation;
    _options = options;
  }

  [HttpPost("submit-transaction")]
  public async Task<PostTransactionsResponse> SubmitTransaction()
  {
    var funder = new Address(User?.Identity?.Name ?? throw new Exception("Invalid authenticated user"));

    using var stream = new MemoryStream();
    await Request.Body.CopyToAsync(stream);
    stream.Position = 0;

    var data = stream.ToArray();
    var txs = new List<SignedTransaction>();
    try
    {
      var tx = Algorand.Utils.Encoder.DecodeFromMsgPack<SignedTransaction>(data);
      txs = [tx];
    }
    catch
    {
      txs = Algorand.Utils.Encoder.DecodeFromMsgPack<List<SignedTransaction>>(data);
    }
    
    if (txs?.Any() != true) throw new Exception("Unable to parse transactions");
    var genesis = txs.First().Tx?.GenesisId ?? throw new Exception("Unable to parse genesis from the first transaction");
    var authHeader = Request.Headers.Authorization.ToString();
    if (authHeader.Substring(5, 1) == " ")
    {
      authHeader = authHeader.Substring(6);
    }
    var sigTx = Algorand.Utils.Encoder.DecodeFromMsgPack<SignedTransaction>(Convert.FromBase64String(authHeader));
    var group = txs.First().Tx?.Group?.Bytes ?? [];
    foreach (var tx in txs)
    {
      if (tx.Tx.GenesisId != sigTx.Tx.GenesisId) throw new Exception("Genesis hash in one of the tx does not match the auth header");
      if (group?.Any() == true)
      {
        if (!tx.Tx.Group.Bytes.SequenceEqual(group))
        {
          throw new Exception("Tx group in each transaction must match. Only one transaction group can be submitted at once.");
        }
      }
    }
    var ret = await _gasStation.SubmitTransaction(funder, txs);
    return ret;
  }
}
