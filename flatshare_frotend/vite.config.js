import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')

  const backendType = env.VITE_BACKEND_TYPE ?? 'team1'
  const proxyTarget = env.VITE_PROXY_TARGET || 'http://localhost:5282'

  return {
    plugins: [react()],
    server: backendType === 'team2'
      ? {
          proxy: {
            '/api': {
              target: proxyTarget,
              changeOrigin: true,
              secure: false,
            },
          },
        }
      : {},
  }
})
