import { GasStationConfigurationV1 } from './types/GasStationConfigurationV1'

export const createGasStationConfigurationV1 = (assets: bigint[], apps: bigint[], addresses: string[]) => {
  const config: GasStationConfigurationV1 = {
    assets: assets,
    apps: apps,
    addresses: addresses,
    version: 1n,
  }
  return JSON.stringify(
    config,
    (key, value) =>
      typeof value === 'bigint' ? (value > Number.MAX_SAFE_INTEGER ? value.toString() : Number(value)) : value, // return everything else unchanged
  )
}
