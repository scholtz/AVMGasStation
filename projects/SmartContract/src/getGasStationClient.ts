import { AlgorandClient } from '@algorandfoundation/algokit-utils'
import { GasStationClient } from '../smart_contracts/artifacts/gas_station/GasStationClient'

export const getGasStationClient = (
  env: 'mainnet-v1.0' | 'aramidmain-v1.0' | 'testnet-v1.0' | 'betanet-v1.0' | 'voimain-v1.0' | 'fnet-v1',
  algorandClient: AlgorandClient,
): GasStationClient => {
  let appId = 0n
  switch (env) {
    case 'aramidmain-v1.0':
    case 'betanet-v1.0':
    case 'fnet-v1':
    case 'mainnet-v1.0':
    case 'testnet-v1.0':
    case 'voimain-v1.0':
      throw Error(`App was not deployed yet to ${env}`)
  }

  return new GasStationClient({ algorand: algorandClient, appId: appId })
}
