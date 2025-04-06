import { Contract } from '@algorandfoundation/algorand-typescript'

export class GasStation extends Contract {
  public hello(name: string): string {
    return `Hello, ${name}`
  }
}
