import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import LoginPage from "../../pages/LoginPage";
import { AuthProvider } from "../../contexts/AuthContext";

// Mock the axios client so AuthProvider doesn't make real requests
vi.mock("../../api/client", () => ({
  default: {
    post: vi.fn(),
    interceptors: {
      request: { use: vi.fn() },
    },
  },
}));

describe("LoginPage", () => {
  it("shows an error when submitting with empty fields", async () => {
    const user = userEvent.setup();

    render(
      <MemoryRouter>
        <AuthProvider>
          <LoginPage />
        </AuthProvider>
      </MemoryRouter>,
    );

    const submitButton = screen.getByRole("button", { name: /sign in/i });
    await user.click(submitButton);

    expect(
      await screen.findByText("Email and password are required."),
    ).toBeInTheDocument();
  });

  it("renders the email and password fields", () => {
    render(
      <MemoryRouter>
        <AuthProvider>
          <LoginPage />
        </AuthProvider>
      </MemoryRouter>,
    );

    expect(screen.getByLabelText("Email")).toBeInTheDocument();
    expect(screen.getByLabelText("Password")).toBeInTheDocument();
  });
});
