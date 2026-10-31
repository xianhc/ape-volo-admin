/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_CLI_PORT: string
  readonly VITE_SERVER_PORT: string
  readonly VITE_BASE_API: string
  readonly VITE_BASE_PATH: string
  readonly VITE_POSITION: string
  readonly VITE_EDITOR: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
