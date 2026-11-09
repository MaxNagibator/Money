<script setup lang="ts">
import { computed } from 'vue'

const props = withDefaults(defineProps<{
  absolute?: boolean
  flexItem?: boolean
  light?: boolean
  vertical?: boolean
  dividerType?: 'fullwidth' | 'inset' | 'middle'
}>(), { dividerType: 'fullwidth' })

const classes = computed(() => [
  'mud-divider',
  {
    'mud-divider-absolute': props.absolute,
    'mud-divider-flexitem': props.flexItem,
    'mud-divider-light': props.light,
    'mud-divider-vertical': props.vertical,
    [`mud-divider-${props.dividerType}`]: props.dividerType !== 'fullwidth' || !props.vertical,
  },
])
</script>

<template>
  <hr :class="classes">
</template>
