"use client";

import { create } from "zustand";
import { authApi } from "@/lib/api/auth";
import { usersApi } from "@/lib/api/users";
import { session } from "@/lib/session";
import type { AuthUser, LoginInput, RegisterInput } from "@/types/auth";

interface AuthState {
  user: AuthUser | null;
  hydrated: boolean;
  hydrate: () => void;
  login: (input: LoginInput) => Promise<AuthUser>;
  register: (input: RegisterInput) => Promise<void>;
  logout: () => Promise<void>;
}

async function enrichWithProfile(user: AuthUser): Promise<AuthUser> {
  try {
    const profile = await usersApi.getProfile(user.id);
    const enriched: AuthUser = {
      ...user,
      firstName: profile.firstName,
      lastName: profile.lastName
    };

    session.setUser(enriched);
    return enriched;
  } catch {
    return user;
  }
}

export const useAuthStore = create<AuthState>((set, get) => ({
  user: null,
  hydrated: false,

  hydrate: () => {
    if (get().hydrated) return;

    const storedUser = session.getUser();
    const hasToken = Boolean(session.getAccessToken());
    const user = storedUser && hasToken ? storedUser : null;

    set({ user, hydrated: true });

    if (user) void enrichWithProfile(user).then((enriched) => set({ user: enriched }));
  },

  login: async (input) => {
    const auth = await authApi.login(input);
    const user: AuthUser = {
      id: auth.userId,
      email: auth.email,
      role: auth.role
    };

    session.save(auth, user);
    set({ user, hydrated: true });

    const enriched = await enrichWithProfile(user);
    set({ user: enriched });

    return enriched;
  },

  register: async (input) => {
    await authApi.register(input);
  },

  logout: async () => {
    const refreshToken = session.getRefreshToken();

    try {
      if (refreshToken) await authApi.logout(refreshToken);
    } catch {
      // حتی اگر بک‌اند در دسترس نباشد، خروج محلی انجام می‌شود
    } finally {
      session.clear();
      set({ user: null, hydrated: true });
    }
  }
}));

export function useAuth() {
  const user = useAuthStore((state) => state.user);
  const hydrated = useAuthStore((state) => state.hydrated);
  const login = useAuthStore((state) => state.login);
  const register = useAuthStore((state) => state.register);
  const logout = useAuthStore((state) => state.logout);

  return {
    user,
    hydrated,
    isAuthenticated: Boolean(user),
    login,
    register,
    logout
  };
}
