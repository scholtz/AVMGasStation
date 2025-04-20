import {
  arc4,
  assert,
  BoxMap,
  Bytes,
  bytes,
  Contract,
  Global,
  GlobalState,
  gtxn,
  itxn,
  op,
  Txn,
  uint64,
} from '@algorandfoundation/algorand-typescript'
import { Address, UintN64 } from '@algorandfoundation/algorand-typescript/arc4'

class UserStruct extends arc4.Struct<{
  balance: UintN64
  configuration: arc4.Str
}> {}
export const version = 'BIATEC-GAS-01-01-02'

export class GasStation extends Contract {
  public configuration = BoxMap<Address, UserStruct>({ keyPrefix: 'c' })
  public allDeposits = GlobalState<uint64>() // difference between this value and real value is the protocol fee accumulation

  /**
   * Top secret multisig account with which it is possible update user contracts or biatec contracts.
   */
  addressUdpater = GlobalState<Address>({ key: 'u' })
  /**
   * Address which can execute the gas distribution. In possession of the Biatec.
   */
  addressExecutive = GlobalState<Address>({ key: 'e' })
  /**
   * Version of the smart contract
   */
  version = GlobalState<string>({ key: 'scver' })

  /**
   * Kill switch. In the extreme case all services (deposit, trading, withdrawal, identity modifications and more) can be suspended.
   * Only addressUdpater multisig can modify this setting.
   */
  suspended = GlobalState<boolean>({ key: 's' })
  /**
   * Initial setup
   */
  public constructor() {
    super()
    this.addressUdpater.value = new Address(Txn.sender)
    this.version.value = version
    this.addressExecutive.value = new Address(Txn.sender)
    this.suspended.value = false
    this.allDeposits.value = 0
  }

  /**
   * addressUdpater from global biatec configuration is allowed to update application
   */
  @arc4.abimethod({ allowActions: 'UpdateApplication' })
  updateApplication(newVersion: string): boolean {
    assert(
      this.addressUdpater.value === new Address(Txn.sender),
      'Only addressUdpater setup in the config can update application',
    )
    this.version.value = newVersion
    return true
  }

  /**
   * Execution address with which it is possible to fund other addresses
   *
   * @param a Address
   */
  @arc4.abimethod()
  setAddressExecutive(a: Address) {
    assert(this.addressUdpater.value === new Address(Txn.sender), 'Only updater can change addressExecutive')
    this.addressExecutive.value = a
  }

  /**
   * Execution address with which it is possible to fund other addresses
   *
   * @param a Address
   */
  @arc4.abimethod()
  setSuspended(isSuspended: boolean) {
    assert(this.addressUdpater.value === new Address(Txn.sender), 'Only updater can change addressExecutive')
    this.suspended.value = isSuspended
  }
  /**
   * Readonly method to fetch easily funder's balance
   *
   * @param funder Funder's address
   */
  @arc4.abimethod({ readonly: true })
  getFunderBalance(funder: Address): UintN64 {
    return this.configuration(funder).value.balance
  }
  /**
   * Readonly method to fetch easily funder's configuration
   *
   * @param funder Funder's address
   */
  @arc4.abimethod({ readonly: true })
  getFunderConfiguration(funder: Address): string {
    return this.configuration(funder).value.configuration.native
  }

  /**
   * Readonly method to fetch easily funder's box
   *
   * @param funder Funder's address
   */
  @arc4.abimethod({ readonly: true })
  getFunderBox(funder: Address): UserStruct {
    return this.configuration(funder).value
  }

  /**
   * Gas Funder can set configuration with the deposit tx
   *
   * Service fee is 5% and is deducted on deposit, on deposit of 100 Algo, user receives 95 Algo credit for his users to use for gas
   * @param txnDeposit Deposit transaction
   * @param configuration Configration to be stored into the box
   */
  @arc4.abimethod()
  public depositWithConfiguration(txnDeposit: gtxn.PaymentTxn, configuration: arc4.Str): void {
    assert(!this.suspended.value, 'The smart contract is suspended at the moment')
    assert(op.len(Bytes(configuration.native)) > 0, 'Configuration must be defined')
    assert(op.substring(Bytes(configuration.native), 0, 1).toString() === '{', 'Invalid configuration provided')
    var sender = new arc4.Address(txnDeposit.sender)
    const fee: uint64 = txnDeposit.amount / 20 //5%
    const deposit: uint64 = txnDeposit.amount - fee

    this.allDeposits.value += deposit

    assert(txnDeposit.receiver === Global.currentApplicationAddress, 'Receiver must be the gas station app')

    if (this.configuration(sender).exists) {
      this.configuration(sender).value.balance = new UintN64(deposit + this.configuration(sender).value.balance.native)
      this.configuration(sender).value.configuration = configuration
    } else {
      const newValue = new UserStruct({
        balance: new UintN64(deposit),
        configuration: configuration,
      })
      this.configuration(sender).value = newValue.copy()
    }
  }

  /**
   * Gas Funder can set configuration
   *
   * Service fee is 5% and is deducted on deposit, on deposit of 100 Algo, user receives 95 Algo credit for his users to use for gas
   * @param txnDeposit Deposit transaction
   * @param configuration Configration to be stored into the box
   */
  @arc4.abimethod()
  public changeConfiguration(configuration: arc4.Str): void {
    assert(!this.suspended.value, 'The smart contract is suspended at the moment')
    assert(op.len(Bytes(configuration.native)) > 0, 'Configuration must be defined')
    assert(op.substring(Bytes(configuration.native), 0, 1).toString() === '{', 'Invalid configuration provided')
    var sender = new arc4.Address(Txn.sender)
    assert(this.configuration(sender).exists, 'Change of the configuration can be executed only on existing boxes')
    this.configuration(sender).value.configuration = configuration
  }

  /**
   * Gas Funder can deposit more algos to his funder account deposit
   *
   * Service fee is 5% and is deducted on deposit, on deposit of 100 Algo, user receives 95 Algo credit for his users to use for gas
   * @param txnDeposit Deposit transaction
   */
  @arc4.abimethod()
  public deposit(txnDeposit: gtxn.PaymentTxn): void {
    assert(!this.suspended.value, 'The smart contract is suspended at the moment')
    var sender = new arc4.Address(txnDeposit.sender)
    const fee: uint64 = txnDeposit.amount / 20 //5%
    const deposit: uint64 = txnDeposit.amount - fee

    this.allDeposits.value += deposit

    assert(txnDeposit.receiver === Global.currentApplicationAddress, 'Receiver must be the gas station app')

    assert(this.configuration(sender).exists, 'Funder must set configuration first')

    this.configuration(sender).value.balance = new UintN64(deposit + this.configuration(sender).value.balance.native)
  }

  /**
   * Executor can fund the account which needs gas to execute the transaction
   *
   * @param amount Amout to send
   * @param receiver Receiver
   * @param note Note
   * @returns
   */
  @arc4.abimethod()
  public fundAccount(amount: uint64, receiver: Address, note: string, funder: Address): bytes {
    assert(!this.suspended.value, 'The smart contract is suspended at the moment')
    assert(this.addressExecutive.value === new Address(Txn.sender), 'Only executor can use this method')
    assert(this.configuration(funder).exists, 'Funder box does not exists')
    const balance = this.configuration(funder).value.balance.native
    // assert(balance > 100000, 'Funder balance must be above 100000')
    // assert(amount < 100000, 'Amount must be below 100000')
    assert(balance > amount + 2000, 'Funder is out of the deposit')

    // 2000 is constant the network fees

    const itxnResult = itxn
      .payment({
        amount: amount,
        receiver: receiver.native,
        note: note,
      })
      .submit()

    // in case of edge case if current fees are > 2000 the tx fails here
    this.configuration(funder).value.balance = new UintN64(
      this.configuration(funder).value.balance.native - amount - itxnResult.fee * 2,
    )

    return itxnResult.txnId
  }
  /**
   * Biatec can withdraw service fees. The current balance
   *
   * @param amount Amout to send
   * @param receiver Receiver
   * @param note Note
   * @returns
   */
  @arc4.abimethod()
  public withdraw(receiver: Address, amount: UintN64): bytes {
    assert(!this.suspended.value, 'The smart contract is suspended at the moment')

    if (this.configuration(receiver).exists) {
      // withdrawal of the funder funds
      var excessBalance: uint64 = this.configuration(receiver).value.balance.native
      assert(amount.native <= excessBalance, 'The withdrawal amount cannot be maximum your deposit')
      // reduce the deposit in the box
      this.configuration(receiver).value.balance = new UintN64(excessBalance - amount.native)

      const itxnResult = itxn
        .payment({
          amount: amount.native,
          receiver: receiver.native,
          note: 'user withdrawal',
        })
        .submit()

      return itxnResult.txnId
    } else {
      // withdrawal of the protocol funds
      assert(this.addressUdpater.value === new Address(Txn.sender), 'Only updater can use this method')
      var excessBalance: uint64 = Global.currentApplicationAddress.balance - this.allDeposits.value
      assert(amount.native <= excessBalance, 'Withdrawal amount cannot be higher then collected fees')
      const itxnResult = itxn
        .payment({
          amount: amount.native,
          receiver: receiver.native,
          note: 'service fee withdrawal',
        })
        .submit()
      return itxnResult.txnId
    }
  }

  /**
   * Updater can perfom key registration for this LP pool
   */
  @arc4.abimethod()
  public sendOnlineKeyRegistration(
    voteKey: bytes,
    selectionKey: bytes,
    stateProofKey: bytes,
    voteFirst: uint64,
    voteLast: uint64,
    voteKeyDilution: uint64,
    fee: uint64,
  ): bytes {
    assert(!this.suspended.value, 'The smart contract is suspended at the moment')
    assert(this.addressUdpater.value === new Address(Txn.sender), 'Only updater can use this method')
    const itxnResult = itxn
      .keyRegistration({
        selectionKey: selectionKey,
        stateProofKey: stateProofKey,
        voteFirst: voteFirst,
        voteKeyDilution: voteKeyDilution,
        voteLast: voteLast,
        voteKey: voteKey,
        fee: fee,
      })
      .submit()
    return itxnResult.txnId
  }
}
