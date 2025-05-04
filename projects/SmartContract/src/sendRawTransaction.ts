export interface IPostTransactionsResponse {
  txId: string
}
export function concatArrays(...arrs: ArrayLike<number>[]) {
  const size = arrs.reduce((sum, arr) => sum + arr.length, 0)
  const c = new Uint8Array(size)

  let offset = 0
  for (let i = 0; i < arrs.length; i++) {
    c.set(arrs[i], offset)
    offset += arrs[i].length
  }

  return c
}
function isByteArray(array: any): array is Uint8Array {
  return array && array.byteLength !== undefined
}
export const sendRawTransaction = async (
  stxOrStxs: Uint8Array | Uint8Array[],
  funderToken: string,
  gasStationUrl: string = 'https://gas-station-api.biatec.io',
): Promise<IPostTransactionsResponse> => {
  try {
    let forPosting = stxOrStxs
    if (Array.isArray(stxOrStxs)) {
      if (!stxOrStxs.every(isByteArray)) {
        throw new TypeError('Array elements must be byte arrays')
      }
      // Flatten into a single Uint8Array
      forPosting = concatArrays(...stxOrStxs)
    } else if (!isByteArray(forPosting)) {
      throw new TypeError('Argument must be byte array')
    }

    console.log('forPosting', forPosting)
    const res = await fetch(`${gasStationUrl}/v1/submit-transaction`, {
      method: 'POST',
      headers: {
        Authorization: funderToken,
        'Content-Type': 'application/x-binary',
      },
      body: forPosting,
    })
    const response = await res.json()

    if (!res.ok) {
      if (response.detail) {
        throw new Error(response.detail)
      } else {
        throw new Error('Network response was not ok')
      }
    }
    if (!response.txId) {
      throw new Error(response)
    }
    return {
      txId: response.txId,
    }
  } catch (error) {
    console.error('POST request failed:', error)
    throw error
  }
}
