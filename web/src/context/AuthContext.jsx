import { createContext, useState, useEffect, useCallback } from "react";
import { authService } from "../services/api";

const AuthContext = createContext();
export default AuthContext;

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [token, setToken] = useState(localStorage.getItem("token"));
  const [loading, setLoading] = useState(false);
  const [isAuthenticated, setIsAuthenticated] = useState(!!token);

  const logout = useCallback(() => {
    setUser(null);
    setToken(null);
    setIsAuthenticated(false);
    localStorage.removeItem("token");
    localStorage.removeItem("tokenExpiry");
  }, []);

  const loadUserProfile = useCallback(async () => {
    if (!token) return;

    try {
      setLoading(true);
      const response = await authService.getProfile();
      if (response.data) {
        setUser(response.data);
        setIsAuthenticated(true);
      }
    } catch (error) {
      // Failed to load user profile, logout user
      logout();
    } finally {
      setLoading(false);
    }
  }, [token, logout]);

  useEffect(() => {
    if (token) {
      loadUserProfile();
    }
  }, [token, loadUserProfile]);

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
      alert("Login failed:" + error);
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
      alert("Registration failed: " + error.response?.data?.message);
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


