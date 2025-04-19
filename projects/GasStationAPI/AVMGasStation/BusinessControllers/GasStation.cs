using Algorand.Algod;
using Algorand;
using AlgorandAuthenticationV2;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using Algorand.Algod.Model.Transactions;
using Algorand.Algod.Model;
using AVMGasStation.Controllers;
using System.Text.RegularExpressions;
using AVMGasStation.Model;
using AlgorandARC76AccountDotNet.Utils;
using AlgorandARC76AccountDotNet;
using System.Text;
using Microsoft.AspNetCore.Mvc.Razor;
using AVMGasStation.GeneratedClients;
using Newtonsoft.Json;
using AVMGasStation.Model.GasConfiguration;
using System.Linq;
using static AVMGasStation.GeneratedClients.GasStationProxy;

namespace AVMGasStation.BusinessControllers
{
  public class GasStation
  {
    private readonly ILogger<GasStation> _logger;
    private readonly IOptions<AlgorandAuthenticationOptionsV2> _algodOptions;
    private readonly IOptions<GasStationConfiguration> _gasStationOptions;
    private readonly Dictionary<string, DefaultApi> algod = new();

    public GasStation(
        ILogger<GasStation> logger,
        IOptions<AlgorandAuthenticationOptionsV2> algodOptions,
        IOptions<GasStationConfiguration> gasStationOptions
        )
    {
      var account = AlgorandARC76AccountDotNet.ARC76.GetAccount(gasStationOptions.Value.FundingAccount);
      logger.LogInformation($"Funder account: {account.Address}");
      _logger = logger;
      _algodOptions = algodOptions;
      _gasStationOptions = gasStationOptions;
      foreach (var network in algodOptions.Value.AllowedNetworks)
      {
        var httpClient = HttpClientConfigurator.ConfigureHttpClient(network.Value.Server, network.Value.Token, network.Value.Header);
        algod[network.Key] = new DefaultApi(httpClient);
      }
    }

    public async Task FundAccount(Address funder, ulong amount, Address receiver, string originalTx, string genesis, ulong? appId, ulong? assetId)
    {

      var client = new GasStationProxy(algod[genesis], _gasStationOptions.Value.ChainGenesis2AppId[genesis]);
      var account = AlgorandARC76AccountDotNet.ARC76.GetAccount(_gasStationOptions.Value.FundingAccount);
      var boxes = new List<BoxRef>()
                {
                    new BoxRef(){App = 0,Name = new byte[] { (byte)'c' }.Concat(funder.Bytes).ToArray()}
                };


      var box = await algod[genesis].GetApplicationBoxByNameAsync(client.appId, $"b64:{Convert.ToBase64String(boxes.First().Name)}");
      var boxData = UserStruct.Parse(box.Value);
      var config = boxData.Configuration;
      //

      //var boxData = await client.GetFunderBox(funder, account, 1000, _tx_boxes: boxes);
      //var config = boxData.Configuration;
      //var config = await client.GetFunderConfiguration(funder, account, 1000, _tx_boxes: boxes);
      var configObjTop = JsonConvert.DeserializeObject<GasConfiguration>(config);

      if (configObjTop?.Version == 1)
      {
        // check rules
        var check = false;
        var configObj = JsonConvert.DeserializeObject<GasConfigurationV1>(config);
        if (configObj?.Addresses.Contains(receiver.EncodeAsString()) == true)
        {
          check = true;
        }
        if (assetId.HasValue && configObj?.Assets.Contains(assetId.Value) == true)
        {
          check = true;
        }
        if (appId.HasValue && configObj?.Apps.Contains(appId.Value) == true)
        {
          check = true;
        }
        if (!check)
        {
          throw new Exception("Funder did not chose this call to be funded");
        }
      }
      else
      {
        throw new Exception("Invalid funder's configuration");
      }

      //var funderBalance = await client.GetFunderBalance(funder, account, 1000, _tx_boxes: boxes);

      await client.FundAccount(
          receiver: receiver,
          funder: funder,
          amount: amount,
          note: "biatec-gas-station/v1:j{\"tx\":\"" + originalTx + "\"}",
          _tx_sender: account,
          _tx_fee: 2000,
          _tx_boxes: boxes
      );
    }

    public async Task<PostTransactionsResponse> SubmitTransaction(Address funder, List<SignedTransaction> signedAVMTransactions, int depth = 0)
    {
      if (depth > 5) throw new Exception("Depth of funding is too high");
      var genesis = signedAVMTransactions.First().Tx.GenesisId;
      if (!algod.ContainsKey(genesis)) throw new Exception($"Chain for genesis id {genesis} not configured");
      try
      {
        var submitOk = await algod[genesis].TransactionsAsync(signedAVMTransactions);
        return submitOk;
      }
      catch (ApiException<PostTransactionsResponse> exc)
      {
        _logger.LogError(exc, "Failed to submit tx");
        throw;
      }
      catch (ApiException<ErrorResponse> exc)
      {
        string pattern = @"transaction (\w+):.*overspend.*?account (\w+),.*?MicroAlgos:\{Raw:(\d+)\}.*?tried to spend \{(\d+)\}";
        Match match = Regex.Match(exc.Result.Message, pattern);

        if (match.Success)
        {
          string transactionId = match.Groups[1].Value;
          string account = match.Groups[2].Value;
          string balance = match.Groups[3].Value;
          string spending = match.Groups[4].Value;

          Console.WriteLine($"Transaction ID: {transactionId}");
          Console.WriteLine($"Account: {account}");
          Console.WriteLine($"Balance: {balance}");
          Console.WriteLine($"Spending: {spending}");
          long toFund = 0;
          var info = await algod[genesis].AccountInformationAsync(account);
          if (info.Amount == 0)
          {
            // not funded yet
            toFund += 100000 + long.Parse(spending); // 100000 is min balance to hold the algo token
          }
          else
          {
            toFund = (long)info.Amount - (long)info.MinBalance + long.Parse(spending);
          }

          // if the tx is asset transfer, check if it is already opted in
          var failedTx = signedAVMTransactions.Where(tx => tx.Tx.TxID() == transactionId).FirstOrDefault();
          ulong? assetIdCause = null;
          ulong? appIdCause = null;

          if (failedTx != null)
          {
            if (failedTx.Tx is AssetTransferTransaction assetTransfer)
            {
              if (info.Assets?.Any(a => a.AssetId == assetTransfer.XferAsset) != true)
              {
                if (assetTransfer.AssetAmount == 0 && assetIdCause == null) // only opt in is funded with 100k micro algos, check if this asset has been funded before
                {
                  toFund += 100000;// need to optin to the asset
                  assetIdCause = assetTransfer.XferAsset;
                }
              }
            }
            if (failedTx.Tx is Algorand.Algod.Model.Transactions.ApplicationNoopTransaction appCall)
            {
              appIdCause = appCall.ApplicationId;
            }

          }

          _logger.LogInformation($"Need to send {toFund} to {account} because of {transactionId}.");
          if (toFund < 0)
          {
            throw new Exception("The calculation of amount to fund seems to be incorrect for this tx.");
          }
          await FundAccount(funder, (ulong)toFund, new Address(account), transactionId, genesis, appIdCause, assetIdCause);
          return await SubmitTransaction(funder, signedAVMTransactions, depth + 1);
        }

        _logger.LogError(exc, "Failed to submit tx");
        throw;
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, "Failed to submit tx");
        throw;
      }
    }
  }
}
