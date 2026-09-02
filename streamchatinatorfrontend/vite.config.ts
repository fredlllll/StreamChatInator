import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';
import { editorTypesPlugin } from './scripts/generateEditorTypes.mjs';

export default defineConfig({
    plugins: [plugin(), editorTypesPlugin()],
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
    build: {
        // The standalone frontend-perf harness is a separate HTML entry (it
        // mounts the real render path with no backend and is driven by
        // Playwright). Build it alongside the app so `vite preview` can serve it.
        rollupOptions: {
            input: {
                app: 'index.html',
                harness: 'harness.html',
            },
        },
    },
});