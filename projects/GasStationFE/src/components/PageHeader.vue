<script setup lang="ts">
import MainButton from './MainButton.vue'
</script>

<template>
  <nav class="container mx-auto px-6 py-4">
    <div class="flex items-center justify-between">
      <div class="flex items-center space-x-2 w-100">
        <div class="relative w-8 h-8">
          <i class="pi pi-cloud absolute text-white text-3xl"></i>
          <i class="pi pi-car absolute text-white text-sm" style="top: 9px; left: 5px"></i>
        </div>
        <RouterLink to="/">
          <span class="text-white text-xl font-bold"> AVM Gas Station</span>
        </RouterLink>
      </div>
      <div class="flex items-center space-x-4 w-100">
        <span class="text-white w-full">Balance: 1,000 ALGO</span>

        <RouterLink to="initial-deposit" class="w-full">
          <MainButton class="px-4 py-2"> Deposit </MainButton>
        </RouterLink>
      </div>
    </div>
  </nav>
</template>
