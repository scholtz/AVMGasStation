import { BoxIdentifier } from '@algorandfoundation/algokit-utils/types/app-manager'
import algosdk from 'algosdk'
import { getGasStationBoxUint8Array } from './getGasStationBoxUint8Array'

export const getGasStationBoxIdentifier = (address: algosdk.Address): BoxIdentifier => {
  var box: BoxIdentifier = getGasStationBoxUint8Array(address)
  return box
}
