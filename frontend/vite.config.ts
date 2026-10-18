import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// The app is published at https://razvangolan.github.io/study-rooms/, so its files live under /study-rooms/.
export default defineConfig(({ command }) => ({
  plugins: [react()],
  base: command === 'build' ? '/study-rooms/' : '/',
  server: { port: 5173, strictPort: true },
}))
