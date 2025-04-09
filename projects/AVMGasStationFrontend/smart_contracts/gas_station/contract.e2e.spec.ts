import { Config } from '@algorandfoundation/algokit-utils'
import { registerDebugEventHandlers } from '@algorandfoundation/algokit-utils-debug'
import { algorandFixture } from '@algorandfoundation/algokit-utils/testing'
import { AlgoAmount } from '@algorandfoundation/algokit-utils/types/amount'
import { BoxIdentifier } from '@algorandfoundation/algokit-utils/types/app-manager'
import algosdk, { Address } from 'algosdk'
import { beforeAll, beforeEach, describe, expect, test } from 'vitest'
import { GasStationFactory } from '../artifacts/gas_station/GasStationClient'

describe('GasStation contract', () => {
  const localnet = algorandFixture()
  beforeAll(() => {
    Config.configure({
      debug: true,
      // traceAll: true,
    })
    registerDebugEventHandlers()
  })
  beforeEach(localnet.newScope)

  const deploy = async (account: Address) => {
    const factory = localnet.algorand.client.getTypedAppFactory(GasStationFactory, {
      defaultSender: account,
    })

    const { appClient } = await factory.deploy({
      onUpdate: 'append',
      onSchemaBreak: 'append',
    })
    return { client: appClient }
  }

  test('can deploy and update', async () => {
    const { testAccount } = localnet.context
    const { client } = await deploy(testAccount)
    const executorAfterDeploy = await client.state.global.addressExecutive()
    const updaterAfterDeploy = await client.state.global.addressUdpater()
    const versionAfterDeploy = await client.state.global.version()
    expect(executorAfterDeploy).toBe(algosdk.encodeAddress(testAccount.addr.publicKey))
    expect(updaterAfterDeploy).toBe(algosdk.encodeAddress(testAccount.addr.publicKey))
    expect(versionAfterDeploy).toBe('BIATEC-GAS-01-01-01')
    const result = await client.send.update.updateApplication({ args: { newVersion: '2' } })
    const versionAfterUpdate = await client.state.global.version()
    expect(versionAfterUpdate).toBe('2')

    expect(result.return).toBe(true)
  })
  test('can deposit', async () => {
    const { testAccount, algod } = localnet.context
    const { client } = await deploy(testAccount)
    const executorAfterDeploy = await client.state.global.addressExecutive()
    const updaterAfterDeploy = await client.state.global.addressUdpater()
    const versionAfterDeploy = await client.state.global.version()
    const suspendedAfterDeploy = await client.state.global.suspended()
    expect(executorAfterDeploy).toBe(algosdk.encodeAddress(testAccount.addr.publicKey))
    expect(updaterAfterDeploy).toBe(algosdk.encodeAddress(testAccount.addr.publicKey))
    expect(versionAfterDeploy).toBe('BIATEC-GAS-01-01-01')
    expect(suspendedAfterDeploy).toBe(0n)
    const params = await algod.getTransactionParams().do()

    var box: BoxIdentifier = new Uint8Array(Buffer.concat([Buffer.from('c', 'ascii'), testAccount.publicKey]))

    await client.send.deposit({
      args: {
        txnDeposit: algosdk.makePaymentTxnWithSuggestedParamsFromObject({
          amount: 1_000_000,
          receiver: client.appAddress,
          sender: testAccount,
          suggestedParams: params,
        }),
      },
      maxFee: AlgoAmount.MicroAlgo(2000),
      staticFee: AlgoAmount.MicroAlgo(2000),
      boxReferences: [box],
    })
  })
})
