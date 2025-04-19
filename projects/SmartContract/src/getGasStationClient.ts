import { AlgorandClient } from '@algorandfoundation/algokit-utils'
import { GasStationClient } from '../smart_contracts/artifacts/gas_station/GasStationClient'

export const getGasStationClient = (
  env: 'mainnet-v1.0' | 'aramidmain-v1.0' | 'testnet-v1.0' | 'betanet-v1.0' | 'voimain-v1.0' | 'fnet-v1',
  algorandClient: AlgorandClient,
): GasStationClient => {
  let appId = 0n
  switch (env) {
    case 'aramidmain-v1.0':
      appId = 176794n
      break
    case 'mainnet-v1.0':
      appId = 2939506666n
      break
    case 'testnet-v1.0':
      appId = 737918254n
      break
    case 'voimain-v1.0':
      appId = 39960441n
      break
    case 'betanet-v1.0':
    case 'fnet-v1':
      throw Error(`App was not deployed yet to ${env}`)
  }

  return new GasStationClient({ algorand: algorandClient, appId: appId })
}
