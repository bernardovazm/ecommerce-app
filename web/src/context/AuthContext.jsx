import React, { createContext, useContext, useState, useEffect } from "react";
import { authService } from "../services/api";

const AuthContext = createContext();

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
};

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [token, setToken] = useState(localStorage.getItem("token"));
  const [loading, setLoading] = useState(false);
  const [isAuthenticated, setIsAuthenticated] = useState(!!token);

  useEffect(() => {
    if (token) {
      loadUserProfile();
    }
  }, [token]);
  const loadUserProfile = async () => {
    if (!token) return;

    try {
      setLoading(true);
      const response = await authService.getProfile();
      if (response.data) {
        setUser(response.data);
        setIsAuthenticated(true);
      }
    } catch (error) {
      console.error("Failed to load user profile:", error);
      logout();
    } finally {
      setLoading(false);
    }
  };

  const login = async (email, password) => {
    try {
      setLoading(true);
      const response = await authService.login({ email, password });

      if (response.data.token) {
        const { token: authToken, expiresAt } = response.data;
        setToken(authToken);
        localStorage.setItem("token", authToken);
        localStorage.setItem("tokenExpiry", expiresAt);
        setIsAuthenticated(true);

        await loadUserProfile();

        return { success: true };
      }
    } catch (error) {
      console.error("Login failed:", error);
      return {
        success: false,
        message:
          error.response?.data?.message || "Login failed. Please try again.",
      };
    } finally {
      setLoading(false);
    }
  };

  const register = async (userData) => {
    try {
      setLoading(true);
      const response = await authService.register(userData);

      if (response.data.token) {
        const { token: authToken, expiresAt } = response.data;
        setToken(authToken);
        localStorage.setItem("token", authToken);
        localStorage.setItem("tokenExpiry", expiresAt);
        setIsAuthenticated(true);

        await loadUserProfile();

        return { success: true };
      }
    } catch (error) {
      console.error("Registration failed:", error);
      return {
        success: false,
        message:
          error.response?.data?.message ||
          "Registration failed. Please try again.",
      };
    } finally {
      setLoading(false);
    }
  };

  const logout = () => {
    setUser(null);
    setToken(null);
    setIsAuthenticated(false);
    localStorage.removeItem("token");
    localStorage.removeItem("tokenExpiry");
  };

  const updateProfile = (updatedUser) => {
    setUser(updatedUser);
  };

  const isTokenExpired = () => {
    const expiry = localStorage.getItem("tokenExpiry");
    if (!expiry) return true;
    return new Date() >= new Date(expiry);
  };

  const value = {
    user,
    token,
    isAuthenticated,
    loading,
    login,
    register,
    logout,
    updateProfile,
    isTokenExpired,
    loadUserProfile,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};
