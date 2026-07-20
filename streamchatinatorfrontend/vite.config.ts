import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';

export default defineConfig({
    plugins: [plugin()],
    server: {
        port: 53401,
        proxy: {
            '/hubs': {
                target: 'http://localhost:17455',
                ws: true,         // needed: SignalR upgrades to a WebSocket after negotiating
                changeOrigin: true,
            },
            '/api': {
                target: 'http://localhost:17455',
                changeOrigin: true,
            },
        },
    },
});