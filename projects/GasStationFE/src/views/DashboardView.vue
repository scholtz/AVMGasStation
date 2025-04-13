<script setup lang="ts"></script>

<template>
  <div class="min-h-screen">
    <nav class="container mx-auto px-6 py-4">
      <div class="flex items-center justify-between">
        <div class="flex items-center space-x-2">
          <div class="relative w-8 h-8">
            <i class="pi pi-cloud absolute text-white text-3xl"></i>
            <i class="pi pi-car absolute text-white text-sm" style="top: 9px; left: 5px"></i>
          </div>
          <span class="text-white text-xl font-bold">AVM Gas Station</span>
        </div>
        <div class="flex items-center space-x-4">
          <span class="text-white">Balance: 1,000 ALGO</span>
          <button
            class="bg-white text-teal-900 px-4 py-2 rounded-lg font-semibold hover:bg-opacity-90 transition-all"
          >
            Deposit
          </button>
        </div>
      </div>
    </nav>

    <main class="container mx-auto px-6 py-8">
      <div class="grid md:grid-cols-2 gap-8">
        <div class="bg-teal-900 bg-opacity-50 p-6 rounded-xl backdrop-blur-lg">
          <h2 class="text-2xl font-bold text-white mb-6">Configuration</h2>
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
            <button
              class="w-full bg-teal-500 text-white py-3 rounded-lg font-semibold hover:bg-teal-600 transition-all"
            >
              Save Configuration
            </button>
          </div>
        </div>

        <div class="bg-teal-900 bg-opacity-50 p-6 rounded-xl backdrop-blur-lg">
          <h2 class="text-2xl font-bold text-white mb-6">Authentication Token</h2>
          <div class="space-y-4">
            <div>
              <label class="block text-teal-100 mb-2">Token Duration (rounds)</label>
              <input
                type="number"
                value="1000"
                class="w-full bg-white bg-opacity-20 text-white p-3 rounded-lg focus:outline-none focus:ring-2 focus:ring-teal-300"
                placeholder="Enter number of rounds"
              />
            </div>
            <button
              class="w-full bg-teal-500 text-white py-3 rounded-lg font-semibold hover:bg-teal-600 transition-all"
            >
              Generate Token
            </button>
          </div>

          <div class="mt-8">
            <h3 class="text-xl font-semibold text-white mb-4">Recent Activity</h3>
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
        </div>
      </div>
    </main>
  </div>
</template>
