import { GasStationConfigurationV1 } from './types/GasStationConfigurationV1'

export const createGasStationConfigurationV1 = (assets: number[], apps: number[], addresses: string[]) => {
  const config: GasStationConfigurationV1 = {
    assets: assets,
    apps: apps,
    addresses: addresses,
    version: 1,
  }
  return JSON.stringify(config)
}
