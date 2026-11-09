import type { Ref } from 'vue'
import { onBeforeUnmount, onMounted, watch } from 'vue'
import './vendor/MudBlazor.min.js'

interface MudPopoverInterop {
  initialize: (containerClass: string, flipMargin: number, overflowPadding: number) => void
  connect: (id: string) => void
  disconnect: (id: string) => void
}

const containerClass = 'mud-popover-provider'
const flipMargin = 0
const overflowPadding = 24

export const mudPopoverContainerSelector = `.${containerClass}`

let sequence = 0
let container: HTMLElement | undefined

const interop = (): MudPopoverInterop | undefined =>
  (window as unknown as { mudPopover?: MudPopoverInterop }).mudPopover

export function ensureMudPopoverContainer(): void {
  if (container?.isConnected === true) {
    return
  }

  const existing = document.querySelector<HTMLElement>(mudPopoverContainerSelector)
  if (existing) {
    container = existing
    return
  }

  container = document.createElement('div')
  container.className = containerClass
  document.body.appendChild(container)
  interop()?.initialize(containerClass, flipMargin, overflowPadding)
}

export function createMudPopoverId(): string {
  sequence += 1
  return `mud-popover-${sequence}`
}

export function useMudPopover(id: string, hasContent: Ref<boolean>): void {
  ensureMudPopoverContainer()

  let connected = false

  function connect(): void {
    if (connected || !hasContent.value) {
      return
    }

    interop()?.connect(id)
    connected = true
  }

  onMounted(connect)

  watch(hasContent, value => {
    if (value) {
      connect()
    }
  }, { flush: 'post' })

  onBeforeUnmount(() => {
    if (!connected) {
      return
    }

    interop()?.disconnect(id)
    connected = false
  })
}
