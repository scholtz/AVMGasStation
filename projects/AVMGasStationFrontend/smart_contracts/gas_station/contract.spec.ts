import { TestExecutionContext } from '@algorandfoundation/algorand-typescript-testing'
import { describe, expect, it } from 'vitest'
import { GasStation } from './contract.algo'

describe('GasStation contract', () => {
  const ctx = new TestExecutionContext()
  it('Logs the returned value when sayHello is called', () => {
    const contract = ctx.contract.create(GasStation)

    const result = contract.hello('Sally')

    expect(result).toBe('Hello, Sally')
  })
})
