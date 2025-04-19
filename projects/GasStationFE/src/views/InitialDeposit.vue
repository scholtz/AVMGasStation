<script setup lang="ts">
import MainPanel from '@/components/MainPanel.vue'
import PageHeader from '@/components/PageHeader.vue'
import Text from '@/components/Text.vue'
</script>

<template>
  <PageHeader></PageHeader>
  <MainPanel class="container mx-auto px-6 py-8">
    <h2 class="text-2xl font-bold text-white mb-6">Initial deposit</h2>
    <Text class="mb-4">
      You can setup the configuration when the submittion of the transaction fails on the specific
      application call, specific token or address the system will execute the funding of the failed
      tx and will submit the tx once again to the chain. If you want to allow only specific token
      transactions to be funded fill in the ASA id to the token id field and leave all other fields
      empty. This will allow gas station to fund the MBR for the account which is trying to opt in
      to your asset and fund the gas fees for this transaction.
    </Text>
    <div class="space-y-4">
      <div>
        <label class="block text-teal-100 mb-2">Application IDs</label>
        <input
          type="text"
          value="123456789, 987654321"
          class="w-full bg-white bg-opacity-20 text-white p-3 rounded-lg focus:outline-none focus:ring-2 focus:ring-teal-300"
          placeholder="Enter comma-separated App IDs"
        />
      </div>
      <div>
        <label class="block text-teal-100 mb-2">Token IDs</label>
        <input
          type="text"
          value="45678912, 32145678"
          class="w-full bg-white bg-opacity-20 text-white p-3 rounded-lg focus:outline-none focus:ring-2 focus:ring-teal-300"
          placeholder="Enter comma-separated Token IDs"
        />
      </div>
      <div>
        <label class="block text-teal-100 mb-2">Authorized Addresses</label>
        <input
          type="text"
          value="ALGO1234..., ALGO5678..."
          class="w-full bg-white bg-opacity-20 text-white p-3 rounded-lg focus:outline-none focus:ring-2 focus:ring-teal-300"
          placeholder="Enter comma-separated addresses"
        />
      </div>
      <div>
        <label class="block text-teal-100 mb-2">Initial deposit (Algo)</label>
        <input
          type="number"
          value="10"
          class="w-full bg-white bg-opacity-20 text-white p-3 rounded-lg focus:outline-none focus:ring-2 focus:ring-teal-300"
          placeholder="Enter number of rounds"
        />
      </div>
      <button
        class="w-full bg-teal-500 text-white py-3 rounded-lg font-semibold hover:bg-teal-600 transition-all"
      >
        Save Configuration
      </button>
    </div>
  </MainPanel>
</template>
