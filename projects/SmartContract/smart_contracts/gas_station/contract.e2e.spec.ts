import { checkIfGasClientIsInitiated } from '@/checkIfGasClientIsInitiated'
import { createGasStationConfigurationV1 } from '@/createGasStationConfigurationV1'
import { getGasStationBoxUint8Array } from '@/getGasStationBoxUint8Array'
import { parseGasStationConfiguration } from '@/parseGasStationConfiguration'
import { Config } from '@algorandfoundation/algokit-utils'
import { registerDebugEventHandlers } from '@algorandfoundation/algokit-utils-debug'
import { algorandFixture } from '@algorandfoundation/algokit-utils/testing'
import { AlgoAmount } from '@algorandfoundation/algokit-utils/types/amount'
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
    expect(versionAfterDeploy).toBe('BIATEC-GAS-01-01-02')
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
    expect(versionAfterDeploy).toBe('BIATEC-GAS-01-01-02')
    expect(suspendedAfterDeploy).toBe(0n)
    const params = await algod.getTransactionParams().do()

    var box = getGasStationBoxUint8Array(testAccount) //: BoxIdentifier = new Uint8Array(Buffer.concat([Buffer.from('c', 'ascii'), testAccount.publicKey]))
    var data =
      '{"assets":[],"apps":[],"addresses":["RIOZSBBY6B7ODWUHT2PA5RNO723MC4UWBQ7OIF56CECON63CLNAFANTWFY"],"version":1}'
    var data2 =
      '{"assets":[],"apps":[1],"addresses":["RIOZSBBY6B7ODWUHT2PA5RNO723MC4UWBQ7OIF56CECON63CLNAFANTWFY"],"version":1}'

    const dataFromCreate = createGasStationConfigurationV1(
      [],
      [],
      ['RIOZSBBY6B7ODWUHT2PA5RNO723MC4UWBQ7OIF56CECON63CLNAFANTWFY'],
    )
    expect(dataFromCreate).toBe(data)
    await client.send.depositWithConfiguration({
      args: {
        txnDeposit: algosdk.makePaymentTxnWithSuggestedParamsFromObject({
          amount: 1_000_000,
          receiver: client.appAddress,
          sender: testAccount,
          suggestedParams: params,
        }),
        configuration: data,
      },
      maxFee: AlgoAmount.MicroAlgo(2000),
      staticFee: AlgoAmount.MicroAlgo(2000),
      boxReferences: [box],
    })
    var funderBalance = await client.getFunderBalance({
      args: { funder: algosdk.encodeAddress(testAccount.addr.publicKey) },
    })
    expect(funderBalance).toBe(950_000n)
    var funderConfig = await client.getFunderConfiguration({
      args: { funder: algosdk.encodeAddress(testAccount.addr.publicKey) },
    })
    expect(funderConfig).toBe(data)
    var funderBox = await client.getFunderBox({
      args: { funder: algosdk.encodeAddress(testAccount.addr.publicKey) },
    })
    expect(funderBox.balance).toBe(950_000n)
    expect(funderBox.configuration).toBe(data)
    const configurationParsed = parseGasStationConfiguration(funderBox.configuration)
    expect(configurationParsed.addresses.length).toBe(1)
    expect(configurationParsed.addresses[0]).toBe('RIOZSBBY6B7ODWUHT2PA5RNO723MC4UWBQ7OIF56CECON63CLNAFANTWFY')

    var boxData = await algod.getApplicationBoxByName(client.appId, box).do()
    expect(Buffer.from(boxData.value).toString('hex')).toBe(
      '00000000000e7ef0000a006e7b22617373657473223a5b5d2c2261707073223a5b5d2c22616464726573736573223a5b2252494f5a534242593642374f445755485432504135524e4f3732334d433455574251374f494635364345434f4e3633434c4e4146414e54574659225d2c2276657273696f6e223a317d',
    )

    await client.send.changeConfiguration({
      args: {
        configuration: data2,
      },
    })

    var config = await checkIfGasClientIsInitiated(client, testAccount.addr)
    expect(config?.configuration).toBe(data2)
  })
})
