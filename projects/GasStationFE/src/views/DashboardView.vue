<script setup lang="ts">
import H2 from '@/components/H2.vue'
import MainButton from '@/components/MainButton.vue'
import PageHeader from '@/components/PageHeader.vue'
import AuthScreen from '@/components/AuthScreen.vue'
import { useWallet } from '@txnlab/use-wallet-vue'
import { useRouter } from 'vue-router'
import MainPanel from '@/components/MainPanel.vue'
import { onMounted, watch } from 'vue'
import { useAppStore } from '@/stores/app'

import { AlgorandClient } from '@algorandfoundation/algokit-utils'

import {
  checkIfGasClientIsInitiated,
  createGasStationConfigurationV1,
  GasStationClient,
  getAppIdFromChainGenesisId,
  getGasStationBoxIdentifier,
  getGasStationBoxUint8Array,
  getGasStationClient,
  parseGasStationConfiguration,
} from 'biatec-avm-gas-station'
import algosdk, { Address, Algodv2, makePaymentTxnWithSuggestedParamsFromObject } from 'algosdk'
import { useToast } from 'primevue/usetoast'

const { activeWallet, activeAccount, transactionSigner } = useWallet()
const router = useRouter()
const clickLogout = async () => {
  store.state.boxData = null
  store.state.configuration = null
  activeWallet.value?.disconnect()
  router.push('/')
}

const store = useAppStore()

onMounted(async () => {
  await loadConfiguration()
})

const loadConfiguration = async () => {
  console.log('Loading configuration', activeAccount.value?.address)
  if (!activeAccount.value?.address) return
  try {
    var algorandClient = AlgorandClient.fromConfig({
      algodConfig: {
        server: store.state.algodHost,
        port: store.state.algodPort,
        token: store.state.algodToken,
      },
      indexerConfig: {
        server: store.state.indexerHost,
        port: store.state.indexerPort,
        token: store.state.indexerToken,
      },
    })
    algorandClient.account.setDefaultSigner(transactionSigner)

    const address = Address.fromString(activeAccount.value?.address)
    var algod = new Algodv2(store.state.algodToken, store.state.algodHost)
    // const appId = getAppIdFromChainGenesisId(store.state.env)
    // console.log('initiating ', appId)
    // var client = new GasStationClient({
    //   algorand: algorandClient,
    //   appId: appId,
    //   defaultSender: activeAccount.value.address,
    //   defaultSigner: transactionSigner,
    // })

    var client = getGasStationClient(
      store.state.env,
      algorandClient,
      activeAccount.value.address,
      transactionSigner,
    )
    const boxName = getGasStationBoxUint8Array(address)

    console.log(
      'fetching box',
      activeAccount.value.address,
      store.state.algodHost,
      client.appId,
      boxName,
    )
    var box = await algod.getApplicationBoxByName(client.appId, boxName).do()
    console.log('fetching box result', box)

    const funderBox = await client.getFunderBox({
      args: {
        funder: activeAccount.value.address,
      },
      //sender: address,
    })
    console.log('funderBox', funderBox)

    console.log(
      'getting box data for ',
      activeAccount.value?.address,
      algosdk.encodeAddress(address.publicKey),
    )
    store.state.boxData = await checkIfGasClientIsInitiated(client, address)
    console.log('result box data for ', activeAccount.value?.address, store.state.boxData)
    if (store.state.boxData?.configuration) {
      store.state.configuration = await parseGasStationConfiguration(
        store.state.boxData?.configuration,
      )
    }
    console.log('store.state.configuration', store.state.configuration)
  } catch (e: any) {
    console.error(e)
    const toast = useToast()
    toast.add({
      severity: 'error',
      detail: e.message ?? e,
      life: 10000,
    })
  }
  if (!store.state.boxData) {
    router.push({ name: 'initial-deposit' })
  }
}

watch(
  () => activeAccount.value,
  async () => {
    await loadConfiguration()
  },
)
</script>

<template>
  <AuthScreen>
    <div class="min-h-screen flex flex-col">
      <PageHeader />
      <main class="container mx-auto px-6 py-8 flex-grow-1">
        <div class="grid md:grid-cols-3 gap-8">
          <div class="bg-teal-900 bg-opacity-50 p-6 rounded-xl backdrop-blur-lg">
            <H2>Select funder action</H2>
            <RouterLink to="initial-deposit">
              <MainButton class="mb-4"> Initial deposit and configuration </MainButton>
            </RouterLink>

            <RouterLink to="configuration-change">
              <MainButton class="mb-4"> Configuration change </MainButton>
            </RouterLink>

            <RouterLink to="deposit">
              <MainButton class="mb-4"> Deposit more </MainButton>
            </RouterLink>

            <RouterLink to="funding-token">
              <MainButton class="mb-4"> Create funding token </MainButton>
            </RouterLink>

            <MainButton @click="clickLogout"> Logout </MainButton>
          </div>
          <div class="bg-teal-900 bg-opacity-50 p-6 rounded-xl backdrop-blur-lg">
            <H2>Current Configuration</H2>
            <div class="space-y-4">
              <div>
                <label class="block text-teal-100 mb-2">Application IDs</label>
                <input
                  disabled
                  type="text"
                  :value="store.state.configuration?.apps.map((m) => m.toString())"
                  class="w-full bg-white bg-opacity-20 text-white p-3 rounded-lg focus:outline-none focus:ring-2 focus:ring-teal-300"
                  placeholder="Comma-separated App IDs"
                />
              </div>
              <div>
                <label class="block text-teal-100 mb-2">Token IDs</label>
                <input
                  disabled
                  type="text"
                  :value="store.state.configuration?.assets.map((m) => m.toString())"
                  class="w-full bg-white bg-opacity-20 text-white p-3 rounded-lg focus:outline-none focus:ring-2 focus:ring-teal-300"
                  placeholder="Comma-separated Token IDs"
                />
              </div>
              <div>
                <label class="block text-teal-100 mb-2">Authorized Addresses</label>
                <input
                  disabled
                  type="text"
                  :value="store.state.configuration?.addresses.map((m) => m.toString())"
                  class="w-full bg-white bg-opacity-20 text-white p-3 rounded-lg focus:outline-none focus:ring-2 focus:ring-teal-300"
                  placeholder="Comma-separated addresses"
                />
              </div>
            </div>
          </div>
          <!-- <div class="bg-teal-900 bg-opacity-50 p-6 rounded-xl backdrop-blur-lg">
            <H2>Recent Activity</H2>

            <div class="mt-8">
              <div class="space-y-3">
                <div class="bg-white bg-opacity-20 p-4 rounded-lg">
                  <div class="flex justify-between items-center">
                    <span class="text-teal-100">Token Generated</span>
                    <span class="text-sm text-teal-200">2 min ago</span>
                  </div>
                  <div class="text-white text-sm mt-1">Duration: 1000 rounds</div>
                </div>
                <div class="bg-white bg-opacity-20 p-4 rounded-lg">
                  <div class="flex justify-between items-center">
                    <span class="text-teal-100">Config Updated</span>
                    <span class="text-sm text-teal-200">1 hour ago</span>
                  </div>
                  <div class="text-white text-sm mt-1">Added 2 new App IDs</div>
                </div>
              </div>
            </div>
          </div> -->
        </div>
      </main>
    </div>
  </AuthScreen>
</template>
