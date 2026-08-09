import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import { ChatProvider } from "./ChatContext";
import { EmoteProvider } from "./EmoteContext";
import App from "./App";
import "./index.css";

createRoot(document.getElementById("root")!).render(
    <StrictMode>
        <BrowserRouter>
            <ChatProvider>
                <EmoteProvider>
                    <App />
                </EmoteProvider>
            </ChatProvider>
        </BrowserRouter>
    </StrictMode>
);