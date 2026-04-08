import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, act } from "@testing-library/react";
import { AuthProvider, useAuth } from "../../contexts/AuthContext";
import client from "../../api/client";

// Mock the axios client
vi.mock("../../api/client", () => ({
  default: {
    post: vi.fn(),
    interceptors: {
      request: { use: vi.fn() },
    },
  },
}));

function AuthConsumer() {
  const { status, user, login } = useAuth();
  return (
    <div>
      <span data-testid="status">{status}</span>
      <span data-testid="email">{user?.email ?? "none"}</span>
      <button onClick={() => login("test@buckeye.edu", "pass")}>Login</button>
    </div>
  );
}

describe("AuthContext", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    sessionStorage.clear();
  });

  it("starts in idle status with no user", () => {
    render(
      <AuthProvider>
        <AuthConsumer />
      </AuthProvider>,
    );

    expect(screen.getByTestId("status")).toHaveTextContent("idle");
    expect(screen.getByTestId("email")).toHaveTextContent("none");
  });

  it("transitions to authenticated after successful login", async () => {
    vi.mocked(client.post).mockResolvedValueOnce({
      data: {
        token: "fake-jwt",
        expiresAt: "2026-12-01T00:00:00Z",
        userId: 1,
        email: "test@buckeye.edu",
        role: "User",
      },
    });

    render(
      <AuthProvider>
        <AuthConsumer />
      </AuthProvider>,
    );

    await act(async () => {
      screen.getByRole("button", { name: "Login" }).click();
    });

    expect(screen.getByTestId("status")).toHaveTextContent("authenticated");
    expect(screen.getByTestId("email")).toHaveTextContent("test@buckeye.edu");
  });
});
