interface ImportMetaEnv {
  readonly VITE_API_BASE_URL?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}

declare const __APP_BUILD__: {
  readonly commit: string
  readonly builtAt: string
}

declare const __PWA_SELF_DESTROY__: boolean
