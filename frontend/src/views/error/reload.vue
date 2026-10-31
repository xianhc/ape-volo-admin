<template>
  <div />
</template>

<script setup lang="ts">
  import { useRouter } from 'vue-router'

  defineOptions({
    name: 'Reload'
  })

  const router = useRouter()
  router.go(-1)
</script>
