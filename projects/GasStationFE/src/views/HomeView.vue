<script setup lang="ts">
import MainButton from '@/components/MainButton.vue'
</script>

<template>
  <div class="flex flex-col items-center justify-center min-h-screen">
    <nav class="container mx-auto px-6 py-4">
      <div class="flex items-center justify-between">
        <div class="flex items-center space-x-2">
          <div class="relative w-8 h-8">
            <i class="pi pi-cloud absolute text-white text-3xl"></i>
            <i class="pi pi-car absolute text-white text-sm" style="top: 9px; left: 5px"></i>
          </div>
          <RouterLink to="/">
            <span class="text-white text-xl font-bold"> AVM Gas Station</span>
          </RouterLink>
        </div>
        <RouterLink to="/dashboard">
          <MainButton class="px-6 py-2"> Connect Wallet </MainButton>
        </RouterLink>
      </div>
    </nav>

    <main class="flex flex-grow-1 container mx-auto px-6 py-16">
      <div class="grid md:grid-cols-2 gap-12 items-center">
        <div>
          <h1 class="text-5xl font-bold text-white mb-6">
            Power Your Algorand dApps with Seamless Gas Coverage
          </h1>
          <p class="text-teal-100 text-lg mb-8">
            Enable users to interact with your dApps without worrying about ALGO balance. Fund
            transactions, cover minimum balance requirements, and boost adoption.
          </p>
          <div class="flex space-x-4">
            <RouterLink
              to="/dashboard"
              class="bg-white text-teal-900 px-8 py-3 rounded-lg font-semibold hover:bg-opacity-90 transition-all flex items-center"
            >
              Become a Funder
              <i class="pi pi-arrow-right ml-2 text-lg"></i>
            </RouterLink>
            <button
              class="border-2 border-white text-white px-8 py-3 rounded-lg font-semibold hover:bg-white hover:bg-opacity-10 transition-all"
            >
              Learn More
            </button>
          </div>
        </div>
        <div class="grid grid-cols-2 gap-6">
          <div
            class="bg-teal-900 hover:bg-opacity-90 bg-opacity-50 p-6 rounded-xl backdrop-blur-lg"
          >
            <i class="pi pi-shield text-4xl text-teal-300 mb-4"></i>
            <h3 class="text-white font-semibold text-xl mb-2">Secure Access</h3>
            <p class="text-teal-100">
              Authorize specific apps and tokens for seamless transactions
            </p>
          </div>
          <div
            class="bg-teal-900 hover:bg-opacity-90 bg-opacity-50 p-6 rounded-xl backdrop-blur-lg"
          >
            <i class="pi pi-dollar text-4xl text-teal-300 mb-4"></i>
            <h3 class="text-white font-semibold text-xl mb-2">Flexible Funding</h3>
            <p class="text-teal-100">
              Deposit and manage funds with complete control using smart contract
            </p>
          </div>
          <div
            class="bg-teal-900 hover:bg-opacity-90 bg-opacity-50 p-6 rounded-xl backdrop-blur-lg"
          >
            <i class="pi pi-home text-4xl text-teal-300 mb-4"></i>
            <h3 class="text-white font-semibold text-xl mb-2">Multi AVM chains support</h3>
            <p class="text-teal-100">
              AVM Gas Station is deployed to Algorand, Voi and Aramid chain
            </p>
          </div>
          <div
            class="bg-teal-900 hover:bg-opacity-90 bg-opacity-50 p-6 rounded-xl backdrop-blur-lg"
          >
            <i class="pi pi-cog text-4xl text-teal-300 mb-4"></i>
            <h3 class="text-white font-semibold text-xl mb-2">Custom Config</h3>
            <p class="text-teal-100">Configure allowed applications, tokens, and addresses</p>
          </div>
        </div>
      </div>
    </main>
  </div>
</template>
