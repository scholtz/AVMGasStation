import { GasStationConfigurationV1 } from './types/GasStationConfigurationV1'

export const parseGasStationConfiguration = (configuration: string) => {
  if (!configuration) throw Error('Configuration is empty')
  var parsed = JSON.parse(configuration)
  if (!parsed.version) {
    throw Error('Version of the configuration not detected')
  }
  if (parsed.version == 1) {
    return parsed as GasStationConfigurationV1
  }
  throw Error('Version is newer then one in this library. Please upgrade the library')
}
