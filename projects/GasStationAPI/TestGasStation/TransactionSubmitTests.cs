#define ATOKEN

using Algorand.Algod;
using Algorand;
using Algorand.Algod.Model;
using Algorand.Algod.Model.Transactions;
using Algorand.KMD;
using System.Security.Principal;
using Algorand.Utils;
using System.Reflection;
using AVMGasStation.BusinessControllers;
using Moq;
using Microsoft.Extensions.Logging;
using AlgorandAuthenticationV2;
using Microsoft.Extensions.Options;
using AVMGasStation.Model;
using AlgorandARC76AccountDotNet;
using System.Text;
using Algorand.AVM.ClientGenerator.ABI.ARC56;
using AVMGasStation.GeneratedClients;
using Newtonsoft.Json;
using AVMGasStation.Model.GasConfiguration;

namespace TestGasStation
{
  public class TransactionSubmitTests
  {
    const string FundingAccount = "TestAccount";
    DefaultApi algodApiInstance;
    [SetUp]
    public async Task Setup()
    {
      var ALGOD_API_ADDR = "http://localhost:4001/";
      var ALGOD_API_TOKEN = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
      var httpClient = HttpClientConfigurator.ConfigureHttpClient(ALGOD_API_ADDR, ALGOD_API_TOKEN);
      algodApiInstance = new DefaultApi(httpClient);


      var executorAccount = ARC76.GetAccount(FundingAccount);

      var accountInfo = await algodApiInstance.AccountInformationAsync(executorAccount.Address.EncodeAsString());
      if (accountInfo.Amount == 0)
      {

        // need to fund the account
        var accountWithAlgos = await GetAccount(0);
        var fundingTx = new PaymentTransaction()
        {
          Sender = accountWithAlgos.Address,
          Receiver = executorAccount.Address,
          Amount = 1_000_000_000,
          Note = Encoding.UTF8.GetBytes("fund the funding account"),
        };
        await fundingTx.FillInParams(algodApiInstance);
        var signed = fundingTx.Sign(accountWithAlgos);
        var sent = await algodApiInstance.TransactionsAsync(new List<SignedTransaction>() { signed });

      }
    }

    private async Task<Account> GetAccount(int accountIndex = 0)
    {

      //A standard sandbox connection
      var client = new HttpClient();
      client.DefaultRequestHeaders.Add("X-KMD-API-Token", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

      var kmdApi = new Api(client);
      kmdApi.BaseUrl = @"http://localhost:4002";
      var handle = await getWalletHandleToken(kmdApi);
      var accs = await kmdApi.ListKeysInWalletAsync(new ListKeysRequest() { Wallet_handle_token = handle });
      var a = accs.Addresses.Skip(accountIndex).First();
      var resp = await kmdApi.ExportKeyAsync(new ExportKeyRequest() { Address = a, Wallet_handle_token = handle, Wallet_password = "" });
      return new Account(resp.Private_key);
    }
    private static async Task<string> getWalletHandleToken(Api kmdApi)
    {
      var wallets = await kmdApi.ListWalletsAsync(null);
      var wallet = wallets.Wallets.FirstOrDefault();
      var handle = await kmdApi.InitWalletHandleTokenAsync(new InitWalletHandleTokenRequest() { Wallet_id = wallet.Id, Wallet_password = "" });
      return handle.Wallet_handle_token;
    }


    [Test]
    public async Task GenerateArc200Client()
    {
      using var client = new HttpClient();
      //var response = await client.GetAsync("https://raw.githubusercontent.com/scholtz/arc200/refs/heads/main/contracts/artifacts/Arc200.arc56.json");
      var response = await client.GetAsync("https://raw.githubusercontent.com/Open-Submarine/opensubmarine-starter/refs/heads/example/arc200/artifacts/AToken.arc56.json");

      Assert.That((int)response.StatusCode, Is.EqualTo(200), "Failed to download file");
      var content = await response.Content.ReadAsStringAsync();
      //var content = File.ReadAllText("..\\..\\..\\..\\..\\AVMGasStation\\projects\\SmartContracts\\smart_contracts\\artifacts\\gas_station\\GasStation.arc56.json");
      Assert.That(content.Trim().StartsWith("{"), Is.True, "File content is not valid JSON");

      var ALGOD_API_ADDR = "http://localhost:4001/";
      var ALGOD_API_TOKEN = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
      var httpClient = HttpClientConfigurator.ConfigureHttpClient(ALGOD_API_ADDR, ALGOD_API_TOKEN);
      DefaultApi algodApiInstance = new DefaultApi(httpClient);

      var generator = new ClientGeneratorARC56();
      generator.LoadFromByteArray(Encoding.UTF8.GetBytes(content));
      var appProxy = await generator.ToProxy("AVMGasStation.GeneratedClients");
      Assert.That(appProxy.Length, Is.GreaterThan(1));
      File.WriteAllText("..\\..\\..\\..\\AVMGasStation\\GeneratedClients\\ARC200Proxy.cs", appProxy);
    }

    [Test]
    public async Task GenerateGasStationClient()
    {
      using var client = new HttpClient();
      var response = await client.GetAsync("https://raw.githubusercontent.com/scholtz/AVMGasStation/refs/heads/main/projects/SmartContract/smart_contracts/artifacts/gas_station/GasStation.arc56.json");

      Assert.That((int)response.StatusCode, Is.EqualTo(200), "Failed to download file");
      var content = await response.Content.ReadAsStringAsync();
      //var content = File.ReadAllText("..\\..\\..\\..\\..\\AVMGasStation\\projects\\SmartContracts\\smart_contracts\\artifacts\\gas_station\\GasStation.arc56.json");
      Assert.That(content.Trim().StartsWith("{"), Is.True, "File content is not valid JSON");

      var ALGOD_API_ADDR = "http://localhost:4001/";
      var ALGOD_API_TOKEN = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
      var httpClient = HttpClientConfigurator.ConfigureHttpClient(ALGOD_API_ADDR, ALGOD_API_TOKEN);
      DefaultApi algodApiInstance = new DefaultApi(httpClient);

      var generator = new ClientGeneratorARC56();
      generator.LoadFromByteArray(Encoding.UTF8.GetBytes(content));
      var appProxy = await generator.ToProxy("AVMGasStation.GeneratedClients");
      Assert.That(appProxy.Length, Is.GreaterThan(1));
      File.WriteAllText("..\\..\\..\\..\\AVMGasStation\\GeneratedClients\\GasStationProxy.cs", appProxy);
    }

    [Test]
    public async Task OptInFunding()
    {
      // create asset with account 0
      var funderAccount = await GetAccount(0);
      var executorAccount = ARC76.GetAccount(FundingAccount);

      AssetCreateTransaction txn = new AssetCreateTransaction()
      {
        AssetParams = new AssetParams() { Creator = funderAccount.Address, Decimals = 1, Total = 1, Name = "TestToken" }
      };
      await txn.FillInParams(algodApiInstance);
      txn.Sender = funderAccount.Address;
      var signed = txn.Sign(funderAccount);
      var send = await algodApiInstance.TransactionsAsync(new List<SignedTransaction>() { signed });
      var txInfo = await Utils.WaitTransactionToComplete(algodApiInstance, send.Txid); // wait for newly created asset id
      ulong? assetId = null;
      if (txInfo is AssetCreateTransaction assetTxInfo)
      {
        assetId = assetTxInfo.AssetIndex;
      }
      Assert.That(assetId, Is.Not.Null);


      // optin to asset with zero balance account
      var emptyAccount = new Account();


      var transParams = await algodApiInstance.TransactionParamsAsync();
      // 1. Mock logger
      var loggerMock = new Mock<ILogger<GasStation>>();

      // 2. deploy gas contract

      var client = new GasStationProxy(algodApiInstance, 0);
      await client.CreateApplication(executorAccount, 1000);
      Assert.That(client.appId, Is.GreaterThan(0));

      // 3. Create IOptions mock or real object

      var algodOptions = Options.Create(new AlgorandAuthenticationOptionsV2
      {
        Realm = "TestRealm",
        CheckExpiration = true,
        EmptySuccessOnFailure = false,
        AllowedNetworks = new AllowedNetworks()
        {
          [transParams.GenesisId] = new AlgodConfig()
          {
            Token = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
            Server = "http://localhost:4001/"
          }
        },
        Debug = true
      });
      var gasStationOptions = Options.Create(new GasStationConfiguration
      {
        FundingAccount = FundingAccount,
        ChainGenesis2AppId = new Dictionary<string, ulong>()
        {
          [transParams.GenesisId] = client.appId
        }
      });


      // make configuration

      var payment = new PaymentTransaction()
      {
        Amount = 100_000_000,
        Sender = funderAccount.Address,
        Fee = 0,
        Note = Encoding.UTF8.GetBytes("Fund gas by funder"),
        Receiver = client.AppAddress,
      };

      var funder = funderAccount;
      byte[] funderBoxName = new byte[] { (byte)'c' }.Concat(funder.Address.Bytes).ToArray();

      var boxContent = JsonConvert.SerializeObject(new GasConfigurationV1()
      {
        Addresses = new List<string>() { emptyAccount.Address.EncodeAsString() }
      });
      await client.DepositWithConfiguration(payment, boxContent, funder, 2000, _tx_boxes: new List<BoxRef>()
            {
                new BoxRef()
                {
                    App = 0,
                    Name = funderBoxName
            } });

      var gasStation = new GasStation(loggerMock.Object, algodOptions, gasStationOptions);

      // The magic is here.. opt in with empty account


      AssetTransferTransaction optInTx = new AssetTransferTransaction()
      {
        AssetAmount = 0,
        Sender = emptyAccount.Address,
        AssetReceiver = emptyAccount.Address,
        XferAsset = assetId.Value,
      };
      await optInTx.FillInParams(algodApiInstance);
      optInTx.Sender = emptyAccount.Address;
      var signedOptInTx = optInTx.Sign(emptyAccount);

      var optInResult = await gasStation.SubmitTransaction(funder.Address, new List<SignedTransaction>() { signedOptInTx });
      Assert.That(optInResult.Txid, Is.Not.Empty);
    }

    [Test]
    public async Task ARC200Funding()
    {
      // create asset with account 0
      var funderAccount = await GetAccount(0);
      var executorAccount = ARC76.GetAccount(FundingAccount);

#if ATOKEN
      var arc200 = new ATokenProxy(algodApiInstance, 0);
#else
      var arc200 = new Arc200Proxy(algodApiInstance, 0);
#endif
      await arc200.CreateApplication(funderAccount, 1000);
      Assert.That(arc200.appId, Is.GreaterThan(0));


      var fundMinBalance = new PaymentTransaction()
      {
        Sender = funderAccount.Address,
        Receiver = arc200.AppAddress,
        Amount = 128500,
        Note = Encoding.UTF8.GetBytes("fund mbr"),
      };
      await fundMinBalance.FillInParams(algodApiInstance);
      await algodApiInstance.TransactionsAsync(new List<SignedTransaction>() { fundMinBalance.Sign(funderAccount) });

      /// Bootstrap
#if ATOKEN
      var boxRefArc200Funder = Encoding.ASCII.GetBytes("balances").Concat(funderAccount.Address.Bytes).ToArray();
      try
      {
        await arc200.Mint(funderAccount.Address, Encoding.UTF8.GetBytes("TestARC200Token"), Encoding.UTF8.GetBytes("test"), 6, new AVM.ClientGenerator.ABI.ARC4.Types.UInt256() { Value = new System.Numerics.BigInteger(1_000_000_000) }, funderAccount, 1000, _tx_transactions: new List<Transaction>()
        {
          new PaymentTransaction()
          {
            Sender = funderAccount.Address,
            Receiver = arc200.AppAddress,
            Amount = 28500,
          }
        }, _tx_boxes: new List<BoxRef>()
        {
          new BoxRef()
          {
            App = 0,
            Name = boxRefArc200Funder
          }
        });
      }
      catch (Algorand.ApiException<Algorand.Algod.Model.ErrorResponse> error)
      {
        throw new Exception(error.Result.Message);
      }
#else
      var boxRefArc200Funder = new byte[] { (byte)'b' }.Concat(funderAccount.Address.Bytes).ToArray();

      var result = await arc200.Bootstrap(Encoding.UTF8.GetBytes("TestARC200Token"), Encoding.UTF8.GetBytes("test"), 6, new AVM.ClientGenerator.ABI.ARC4.Types.UInt256() { Value = new System.Numerics.BigInteger(1_000_000_000) },funderAccount, 1000, _tx_boxes: new List<BoxRef>()
      {
        new BoxRef()
        {
          App = 0,
          Name = boxRefArc200Funder
        }
      });
      Assert.That(result, Is.True);
#endif

      AssetCreateTransaction txn = new AssetCreateTransaction()
      {
        AssetParams = new AssetParams() { Creator = funderAccount.Address, Decimals = 1, Total = 1, Name = "TestToken" }
      };
      await txn.FillInParams(algodApiInstance);
      txn.Sender = funderAccount.Address;
      var signed = txn.Sign(funderAccount);
      var send = await algodApiInstance.TransactionsAsync(new List<SignedTransaction>() { signed });
      var txInfo = await Utils.WaitTransactionToComplete(algodApiInstance, send.Txid); // wait for newly created asset id
      ulong? assetId = null;
      if (txInfo is AssetCreateTransaction assetTxInfo)
      {
        assetId = assetTxInfo.AssetIndex;
      }
      Assert.That(assetId, Is.Not.Null);

      var transParams = await algodApiInstance.TransactionParamsAsync();
      // 1. Mock logger
      var loggerMock = new Mock<ILogger<GasStation>>();

      // 2. deploy gas contract

      var client = new GasStationProxy(algodApiInstance, 0);
      await client.CreateApplication(executorAccount, 1000);
      Assert.That(client.appId, Is.GreaterThan(0));

      // 3. Create IOptions mock or real object

      var algodOptions = Options.Create(new AlgorandAuthenticationOptionsV2
      {
        Realm = "TestRealm",
        CheckExpiration = true,
        EmptySuccessOnFailure = false,
        AllowedNetworks = new AllowedNetworks()
        {
          [transParams.GenesisId] = new AlgodConfig()
          {
            Token = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
            Server = "http://localhost:4001/"
          }
        },
        Debug = true
      });
      var gasStationOptions = Options.Create(new GasStationConfiguration
      {
        FundingAccount = FundingAccount,
        ChainGenesis2AppId = new Dictionary<string, ulong>()
        {
          [transParams.GenesisId] = client.appId
        }
      });


      // make configuration

      var payment = new PaymentTransaction()
      {
        Amount = 100_000_000,
        Sender = funderAccount.Address,
        Fee = 0,
        Note = Encoding.UTF8.GetBytes("Fund gas by funder"),
        Receiver = client.AppAddress,
      };

      var funder = funderAccount;



      byte[] funderBoxName = new byte[] { (byte)'c' }.Concat(funder.Address.Bytes).ToArray();

      var boxContent = JsonConvert.SerializeObject(new GasConfigurationV1()
      {
        Apps = new List<ulong>() { arc200.appId }
      });
      await client.DepositWithConfiguration(payment, boxContent, funder, 2000, _tx_boxes: new List<BoxRef>()
      {
        new BoxRef()
        {
            App = 0,
            Name = funderBoxName
        }
      });

      var gasStation = new GasStation(loggerMock.Object, algodOptions, gasStationOptions);


      // the magic is here.. send to empty account some arc200 asset
      var emptyAccount = new Account();
#if ATOKEN
      var boxRefArc200Empty1 = Encoding.ASCII.GetBytes("balances").Concat(emptyAccount.Address.Bytes).ToArray();
#else
      var boxRefArc200Empty1 = new byte[] { (byte)'b' }.Concat(emptyAccount.Address.Bytes).ToArray();
#endif
      var txs = await arc200.Arc200Transfer_Transactions(emptyAccount.Address, new AVM.ClientGenerator.ABI.ARC4.Types.UInt256() { Value = new System.Numerics.BigInteger(1000) }, funderAccount, 1000 /* fee */, _tx_boxes: new List<BoxRef>()
      {
        new BoxRef()
        {
          App = 0,
          Name = boxRefArc200Empty1
        },
        new BoxRef()
        {
          App = 0,
          Name = boxRefArc200Funder
        },
      });

      Assert.That(txs.Count, Is.EqualTo(1));
      var signedTx = txs[0].Sign(funderAccount);

      var receiveResult = await gasStation.SubmitTransaction(funder.Address, new List<SignedTransaction>() { signedTx });
      Assert.That(receiveResult.Txid, Is.Not.Empty);



      // the magic is here.. now move from the empty account to some second empty account
      var emptyAccount2 = new Account();
#if ATOKEN
      var boxRefArc200Empty2 = Encoding.ASCII.GetBytes("balances").Concat(emptyAccount2.Address.Bytes).ToArray();
#else
      var boxRefArc200Empty2 = new byte[] { (byte)'b' }.Concat(emptyAccount2.Address.Bytes).ToArray();
#endif

      var txs2 = await arc200.Arc200Transfer_Transactions(emptyAccount2.Address, new AVM.ClientGenerator.ABI.ARC4.Types.UInt256() { Value = new System.Numerics.BigInteger(1000) }, emptyAccount, 1000 /* fee */, _tx_boxes: new List<BoxRef>()
      {
        new BoxRef()
        {
          App = 0,
          Name = boxRefArc200Empty1
        },
        new BoxRef()
        {
          App = 0,
          Name = boxRefArc200Empty2
        },
      });

      Assert.That(txs2.Count, Is.EqualTo(1));
      var signedTx2 = txs2[0].Sign(emptyAccount);

      var receiveResult2 = await gasStation.SubmitTransaction(funder.Address, new List<SignedTransaction>() { signedTx2 });
      Assert.That(receiveResult2.Txid, Is.Not.Empty);
    }
  }
}

