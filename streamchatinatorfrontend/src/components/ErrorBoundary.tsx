import { Component } from "react";
import type { ErrorInfo, ReactNode } from "react";

interface Props {
    children: ReactNode;
}

interface State {
    hasError: boolean;
}

/**
 * Catches render errors in its children and shows a recovery screen instead of
 * letting React unmount the whole tree to a blank page. Without one, a single
 * render error (malformed event data, an editor/virtualization glitch) takes
 * down the entire app.
 */
class ErrorBoundary extends Component<Props, State> {
    state: State = { hasError: false };

    static getDerivedStateFromError(): State {
        return { hasError: true };
    }

    componentDidCatch(error: Error, info: ErrorInfo) {
        console.error("Uncaught render error:", error, info);
    }

    handleReload = () => {
        this.setState({ hasError: false });
    };

    render() {
        if (this.state.hasError) {
            return (
                <div className="auth-screen">
                    <div className="auth-card">
                        <h1>Something went wrong</h1>
                        <p>The app hit an unexpected error while rendering. Reloading may fix it.</p>
                        <button type="button" className="btn btn-primary" onClick={this.handleReload}>
                            Reload
                        </button>
                    </div>
                </div>
            );
        }
        return this.props.children;
    }
}

export default ErrorBoundary;
