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

    [HttpPost(Name = "submit-transaction")]
    public Task<PostTransactionsResponse> SubmitTransaction(string signedAVMTransaction)
    {
        var funder = new Address(User?.Identity?.Name ?? throw new Exception("Invalid authenticated user")); 

        var txs = Algorand.Utils.Encoder.DecodeFromMsgPack<List<SignedTransaction>>(Convert.FromBase64String(signedAVMTransaction));
        if (txs?.Any() != true) throw new Exception("Unable to parse transactions");
        var genesis = txs.First().Tx?.GenesisId ?? throw new Exception("Unable to parse genesis from the first transaction");
        var authHeader = Request.Headers.Authorization.ToString();
        if (authHeader.Substring(5, 1) == " ")
        {
            authHeader = authHeader.Substring(6);
        }
        var sigTx = Algorand.Utils.Encoder.DecodeFromMsgPack<SignedTransaction>(Convert.FromBase64String(authHeader));
        var group = txs.First().Tx.Group.Bytes;
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

        return _gasStation.SubmitTransaction(funder, txs);
    }
}
