import { AlgorandClient } from '@algorandfoundation/algokit-utils'
import { Address, TransactionSigner } from 'algosdk'
import { GasStationClient } from '../smart_contracts/artifacts/gas_station/GasStationClient'
import { getAppIdFromChainGenesisId } from './getAppIdFromChainGenesisId'

export const getGasStationClient = (
  env: 'mainnet-v1.0' | 'aramidmain-v1.0' | 'testnet-v1.0' | 'betanet-v1.0' | 'voimain-v1.0' | 'fnet-v1',
  algorandClient: AlgorandClient,
  defaultSender?: string | Address | undefined,
  defaultSigner?: TransactionSigner | undefined,
): GasStationClient => {
  const appId = getAppIdFromChainGenesisId(env)
  return new GasStationClient({
    algorand: algorandClient,
    appId: appId,
    defaultSender: defaultSender,
    defaultSigner: defaultSigner,
  })
}
