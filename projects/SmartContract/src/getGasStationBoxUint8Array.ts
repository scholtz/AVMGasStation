import algosdk from 'algosdk'

export const getGasStationBoxUint8Array = (address: algosdk.Address): Uint8Array => {
  var box: Uint8Array = new Uint8Array(Buffer.concat([Buffer.from('c', 'ascii'), address.publicKey]))
  return box
}
