import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import { ChatProvider } from "./ChatContext";
import { EmoteProvider } from "./EmoteContext";
import { ThemeProvider } from "./theme";
import App from "./App";
import "./index.css";

createRoot(document.getElementById("root")!).render(
    <StrictMode>
        <BrowserRouter>
            <ThemeProvider>
                <ChatProvider>
                    <EmoteProvider>
                        <App />
                    </EmoteProvider>
                </ChatProvider>
            </ThemeProvider>
        </BrowserRouter>
    </StrictMode>
);