import algosdk from 'algosdk'
import { GasStationClient, UserStruct } from '../smart_contracts/artifacts/gas_station/GasStationClient'

export const checkIfGasClientIsInitiated = async (
  client: GasStationClient,
  address: algosdk.Address,
): Promise<UserStruct | null> => {
  try {
    var ret = await client.getFunderBox({
      args: {
        funder: algosdk.encodeAddress(address.publicKey),
      },
    })
    return ret
  } catch (e) {
    return null
  }
}
