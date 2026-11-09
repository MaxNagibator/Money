<script setup lang="ts">
import { computed, onBeforeUnmount, ref, watch } from 'vue'
import { mdCheckCircleOutline, mdClose, mdErrorOutline, mdEvent, mdInfo, mdReportProblem } from '@/shared/icons'
import MudIcon from './MudIcon.vue'
import MudIconButton from './MudIconButton.vue'
import type { MudSeverity } from './types'

defineOptions({ inheritAttrs: false })

const props = withDefaults(defineProps<{
  message: string
  severity?: MudSeverity
  visibleStateDuration?: number
}>(), {
  severity: 'normal',
  visibleStateDuration: 5000,
})

const visible = defineModel<boolean>({ default: false })

const icons: Record<MudSeverity, string> = {
  normal: mdEvent,
  info: mdInfo,
  success: mdCheckCircleOutline,
  warning: mdReportProblem,
  error: mdErrorOutline,
}

type SnackbarPhase = 'hidden' | 'showing' | 'visible' | 'hiding'

const showTransitionDuration = 1000
const hideTransitionDuration = 2000
const maximumOpacity = '95%'

const phase = ref<SnackbarPhase>('hidden')
const paused = ref(false)
const transitionCancellable = ref(true)
const userHasInteracted = ref(false)
let hideOnResume = false
let timer: ReturnType<typeof setTimeout> | undefined

const stopTimer = (): void => {
  if (timer !== undefined) {
    clearTimeout(timer)
    timer = undefined
  }
}

const timerElapsed = (): void => {
  timer = undefined

  if (paused.value) {
    if (phase.value === 'visible' || phase.value === 'hiding') {
      hideOnResume = true
    }

    return
  }

  if (phase.value === 'showing') {
    transitionTo('visible')
  } else if (phase.value === 'visible') {
    transitionTo('hiding')
  } else if (phase.value === 'hiding') {
    phase.value = 'hidden'
    visible.value = false
  }
}

const startTimer = (duration: number): boolean => {
  if (duration <= 0) {
    return false
  }

  stopTimer()
  timer = setTimeout(timerElapsed, duration)

  return true
}

function transitionTo(next: SnackbarPhase, animate = true, cancellable = true): void {
  if (!cancellable) {
    paused.value = false
  } else if (!transitionCancellable.value) {
    return
  }

  stopTimer()
  phase.value = next
  transitionCancellable.value = cancellable

  if (next === 'showing') {
    if (!animate || !startTimer(showTransitionDuration)) {
      transitionTo('visible')
    }
  } else if (next === 'visible') {
    if (!animate || !startTimer(props.visibleStateDuration)) {
      transitionTo('hiding')
    }
  } else if (next === 'hiding') {
    if (!animate || !startTimer(hideTransitionDuration)) {
      phase.value = 'hidden'
      visible.value = false
    }
  }
}

const pauseTransitions = (pause: boolean): void => {
  if (!transitionCancellable.value) {
    paused.value = false
    return
  }

  paused.value = pause

  if (pause) {
    if (phase.value === 'showing') {
      transitionTo('visible')
    } else if (phase.value === 'hiding') {
      stopTimer()
      phase.value = 'visible'
      hideOnResume = true
    }
  } else if (hideOnResume) {
    hideOnResume = false
    transitionTo('hiding')
  }
}

const closeIconClicked = (): void => {
  if (userHasInteracted.value) {
    return
  }

  userHasInteracted.value = true
  transitionTo('hiding', true, false)
}

const styles = computed<Record<string, string>>(() => {
  if (phase.value === 'showing') {
    return { opacity: maximumOpacity, animation: `${showTransitionDuration}ms linear mud-snackbar-showing` }
  }

  if (phase.value === 'hiding') {
    return { opacity: '0', animation: `${hideTransitionDuration}ms linear mud-snackbar-hiding` }
  }

  if (phase.value === 'visible') {
    return { opacity: maximumOpacity }
  }

  return {}
})

watch(visible, isVisible => {
  if (!isVisible) {
    stopTimer()
    phase.value = 'hidden'
    return
  }

  if (phase.value !== 'hidden') {
    return
  }

  paused.value = false
  hideOnResume = false
  userHasInteracted.value = false
  transitionCancellable.value = true
  transitionTo('showing')
}, { immediate: true })

onBeforeUnmount(stopTimer)
</script>

<template>
  <div
    v-if="visible"
    id="mud-snackbar-container"
    class="mud-snackbar-location-bottom-right"
  >
    <div
      role="alert"
      aria-live="polite"
      :class="['mud-snackbar', `mud-alert-filled-${severity}`]"
      :style="styles"
      v-bind="$attrs"
      @pointerenter="pauseTransitions(true)"
      @pointerleave="pauseTransitions(false)"
      @touchstart="pauseTransitions(true)"
      @touchend="pauseTransitions(false)"
      @touchcancel="pauseTransitions(false)"
    >
      <div class="mud-snackbar-icon">
        <MudIcon
          :icon="icons[severity]"
          color="inherit"
          size="medium"
        />
      </div>

      <div class="mud-snackbar-content-message">
        {{ message }}
      </div>

      <div class="mud-snackbar-content-action">
        <MudIconButton
          aria-label="Закрыть"
          class="mud-snackbar-close-button ms-2"
          :icon="mdClose"
          size="small"
          @click="closeIconClicked"
        />
      </div>
    </div>
  </div>
</template>

<style>
@keyframes mud-snackbar-showing {
  from { opacity: 0%; }
  to { opacity: 95%; }
}

@keyframes mud-snackbar-hiding {
  from { opacity: 95%; }
  to { opacity: 0%; }
}
</style>
