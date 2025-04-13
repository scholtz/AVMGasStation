<script setup lang="ts">
import { RouterView } from 'vue-router'
// import { useAppStore } from '@/stores/appState'
// const store = useAppStore()
import { Gradient } from 'whatamesh'
import { onMounted, watch } from 'vue'

let gradient = new Gradient()
onMounted(() => {
  console.log('gradient init')
  gradient.initGradient('#gradient-canvas')
  console.log('gradient done')
})

// watch(
//   store.state,
//   () => {
//     setTimeout(() => {
//       gradient.initGradient('#gradient-canvas')
//     }, 200)
//   },
//   { deep: true },
// )
</script>
<template>
  <canvas id="gradient-canvas" data-transition-in />

  <RouterView />
</template>
<style>
#gradient-canvas {
  width: 100%;
  height: 100%;
  --gradient-color-1: #043d5d;
  --gradient-color-2: #032e46;
  --gradient-color-3: #23b684;
  --gradient-color-4: #0f595e;
  position: fixed;
  top: 0;
  left: 0;
  z-index: -10000;
}
</style>
