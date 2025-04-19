import { AlgorandClient } from '@algorandfoundation/algokit-utils'
import algosdk, { Transaction, TransactionSigner } from 'algosdk'
import { GasStationClient, GasStationFactory } from '../smart_contracts/artifacts/gas_station/GasStationClient'
const deploy = async () => {
  /*
set -o allexport
source .env
set +o allexport  
  */
  // console.log('process', process)
  // console.log('process.env', process.env)
  // console.log('process.env.ALGOD_SERVER', process.env.ALGOD_SERVER)
  if (!process.env.ALGOD_SERVER) {
    throw Error('Algod Server is missing. Make sure you configure .env vars')
  }
  if (!process.env.INDEXER_SERVER) {
    throw Error('Algod Server is missing. Make sure you configure .env vars')
  }
  const signers: algosdk.Account[] = []
  if (process.env.DEPLOYER_MNEMONICS_1) {
    signers.push(algosdk.mnemonicToSecretKey(process.env.DEPLOYER_MNEMONICS_1))
  }
  if (process.env.DEPLOYER_MNEMONICS_2) {
    signers.push(algosdk.mnemonicToSecretKey(process.env.DEPLOYER_MNEMONICS_2))
  }
  if (process.env.DEPLOYER_MNEMONICS_3) {
    signers.push(algosdk.mnemonicToSecretKey(process.env.DEPLOYER_MNEMONICS_3))
  }
  if (process.env.DEPLOYER_MNEMONICS_4) {
    signers.push(algosdk.mnemonicToSecretKey(process.env.DEPLOYER_MNEMONICS_4))
  }
  if (process.env.DEPLOYER_MNEMONICS_5) {
    signers.push(algosdk.mnemonicToSecretKey(process.env.DEPLOYER_MNEMONICS_5))
  }
  const threshold = Number(process.env.DEPLOYER_MSIG_THRESHOLD)
  const deployerMsigParams: algosdk.MultisigMetadata = {
    addrs: signers.map((a) => a.addr),
    threshold: threshold > 0 ? threshold : 2,
    version: 1,
  }
  const msigAccount = algosdk.multisigAddress(deployerMsigParams)
  const msigAddress = algosdk.encodeAddress(msigAccount.publicKey)
  console.log('Deployer address:', msigAddress)

  const signer: TransactionSigner = async (txnGroup: Transaction[], indexesToSign: number[]) => {
    return txnGroup.map((tx) => {
      let msigObject = algosdk.createMultisigTransaction(tx, deployerMsigParams)
      for (const signer of signers) {
        console.log(`signing ${tx.txID()} from ${signer.addr}`)
        msigObject = algosdk.appendSignMultisigTransaction(msigObject, deployerMsigParams, signer.sk).blob
      }
      // console.log('decoded', algosdk.decodeSignedTransaction(msigObject).msig);
      return msigObject
    })
  }
  const appId = BigInt(process.env.APPID ?? '0')
  if (appId == 0n) {
    var factory = new GasStationFactory({
      algorand: AlgorandClient.fromConfig({
        algodConfig: {
          server: process.env.ALGOD_SERVER,
        },
        indexerConfig: {
          server: process.env.INDEXER_SERVER,
        },
      }),
      //updatable: true,
      appName: 'gas-station',
      defaultSender: msigAddress,
      defaultSigner: signer,
    })

    console.log(`Deploying app`)
    const { appClient } = await factory.deploy({
      onUpdate: 'append',
      onSchemaBreak: 'append',
    })
    if (process.env.EXECUTOR_ADDRESS) {
      console.log(`Setting executor address to ${process.env.EXECUTOR_ADDRESS}`)
      appClient.send.setAddressExecutive({
        args: {
          a: process.env.EXECUTOR_ADDRESS,
        },
      })
    }
  } else {
    console.log(`Updating application ${appId}`)
    // update application
    var client = new GasStationClient({
      algorand: AlgorandClient.fromConfig({
        algodConfig: {
          server: process.env.ALGOD_SERVER,
        },
        indexerConfig: {
          server: process.env.INDEXER_SERVER,
        },
      }),
      appId: appId,
      appName: 'gas-station',
      defaultSender: msigAddress,
      defaultSigner: signer,
    })

    const result = await client.send.update.updateApplication({ args: { newVersion: 'BIATEC-GAS-01-01-01' } })
  }
  console.log(`DONE`)
}

deploy()
