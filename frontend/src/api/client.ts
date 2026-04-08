import axios from "axios";

const API_BASE = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5000";

const client = axios.create({
    baseURL: API_BASE,
    headers: { "Content-Type": "application/json" },
});

// Request interceptor — attach the JWT from sessionStorage on every request
client.interceptors.request.use((config) => {
    const token = sessionStorage.getItem("token");
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

export default client;
