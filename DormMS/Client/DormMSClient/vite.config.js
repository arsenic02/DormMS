import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    open: true,
    // Dev proxy: relativni /api i /uploads se prosledjuju lokalnom backend-u.
    // Tako isti klijentski kod (relativne putanje) radi i lokalno i u Docker-u.
    proxy: {
      '/api': {
        target: 'http://localhost:5153',
        changeOrigin: true,
      },
      '/uploads': {
        target: 'http://localhost:5153',
        changeOrigin: true,
      },
    },
  },
})
