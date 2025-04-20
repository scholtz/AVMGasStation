export const getAppIdFromChainGenesisId = (
  env: 'mainnet-v1.0' | 'aramidmain-v1.0' | 'testnet-v1.0' | 'betanet-v1.0' | 'voimain-v1.0' | 'fnet-v1',
) => {
  switch (env) {
    case 'aramidmain-v1.0':
      return 176794n
    case 'mainnet-v1.0':
      return 2939506666n
    case 'testnet-v1.0':
      return 737918254n
    case 'voimain-v1.0':
      return 39960441n
    case 'betanet-v1.0':
    case 'fnet-v1':
      throw Error(`App was not deployed yet to ${env}`)
  }
}
