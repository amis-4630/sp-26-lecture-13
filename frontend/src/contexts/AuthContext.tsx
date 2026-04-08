import {
  createContext,
  useContext,
  useState,
  useEffect,
  useCallback,
  type ReactNode,
} from "react";
import client from "../api/client";

type User = {
  id: number;
  email: string;
  role: string;
};

type AuthState = {
  user: User | null;
  token: string | null;
  status: "idle" | "loading" | "authenticated" | "error";
  error: string | null;
};

type AuthContextType = AuthState & {
  login: (email: string, password: string) => Promise<void>;
  register: (
    email: string,
    password: string,
    fullName: string,
  ) => Promise<void>;
  logout: () => void;
};

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, setState] = useState<AuthState>({
    user: null,
    token: null,
    status: "idle",
    error: null,
  });

  // Rehydrate from sessionStorage on mount.
  // We use sessionStorage (not localStorage) because sessionStorage is cleared
  // when the tab closes — reducing the XSS window for stolen tokens (W13 concern).
  useEffect(() => {
    const token = sessionStorage.getItem("token");
    const userJson = sessionStorage.getItem("user");
    if (token && userJson) {
      const user = JSON.parse(userJson) as User;
      setState({ user, token, status: "authenticated", error: null });
    }
  }, []);

  const login = useCallback(async (email: string, password: string) => {
    setState((s) => ({ ...s, status: "loading", error: null }));
    try {
      const { data } = await client.post("/api/auth/login", {
        email,
        password,
      });
      const user: User = {
        id: data.userId,
        email: data.email,
        role: data.role,
      };
      sessionStorage.setItem("token", data.token);
      sessionStorage.setItem("user", JSON.stringify(user));
      setState({
        user,
        token: data.token,
        status: "authenticated",
        error: null,
      });
    } catch (err: unknown) {
      const message =
        (err as { response?: { data?: { message?: string } } })?.response?.data
          ?.message ?? "Login failed";
      setState({ user: null, token: null, status: "error", error: message });
      throw new Error(message);
    }
  }, []);

  const register = useCallback(
    async (email: string, password: string, fullName: string) => {
      setState((s) => ({ ...s, status: "loading", error: null }));
      try {
        await client.post("/api/auth/register", { email, password, fullName });
        // Auto-login after registration
        await login(email, password);
      } catch (err: unknown) {
        const message =
          (err as { response?: { data?: { title?: string } } })?.response?.data
            ?.title ?? "Registration failed";
        setState({ user: null, token: null, status: "error", error: message });
        throw new Error(message);
      }
    },
    [login],
  );

  const logout = useCallback(() => {
    sessionStorage.removeItem("token");
    sessionStorage.removeItem("user");
    setState({ user: null, token: null, status: "idle", error: null });
  }, []);

  return (
    <AuthContext.Provider value={{ ...state, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
}
