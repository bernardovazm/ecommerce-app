import axios from "axios";

const getApiBaseUrl = () => {
  if (import.meta.env.VITE_API_URL) {
    return import.meta.env.VITE_API_URL;
  }

  const isDocker =
    window.location.hostname !== "localhost" &&
    window.location.hostname !== "127.0.0.1";
  const isProd = import.meta.env.PROD;

  if (isProd || isDocker) {
    return "http://localhost:7000/api";
  }

  return "http://localhost:5220/api";
};

const API_BASE_URL = getApiBaseUrl();

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
  timeout: 10000,
});

api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem("token");
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

api.interceptors.response.use(
  (response) => {
    return response;
  },
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem("token");
      localStorage.removeItem("tokenExpiry");
      if (window.location.pathname !== "/login") {
        window.location.href = "/login";
      }
    }
    return Promise.reject(error);
  }
);

export const productService = {
  getFeatured: () => api.get("/products/featured"),
  getAll: (params = {}) => api.get("/products", { params }),
  getById: (id) => api.get(`/products/${id}`),
  search: (searchTerm, category, page = 1, pageSize = 12) =>
    api.get("/products", {
      params: { search: searchTerm, category, page, pageSize },
    }),
};

export const categoryService = {
  getAll: () => api.get("/categories"),
};

export const orderService = {
  create: (orderData) => api.post("/orders", orderData),
  createGuest: (orderData) => api.post("/orders/guest", orderData),
  getById: (id) => api.get(`/orders/${id}`),
  getByCustomer: (customerId) => api.get(`/orders/customer/${customerId}`),
};

export const shippingService = {
  calculateShipping: (shippingData) =>
    api.post("/shipping/calculate", shippingData),
  validateZipCode: (zipCode) =>
    api.get(`/shipping/zip-code/${zipCode}/validate`),
};

export const authService = {
  login: (credentials) => api.post("/auth/login", credentials),
  register: (userData) => api.post("/auth/register", userData),
  getProfile: () => api.get("/auth/profile"),
  logout: () => api.post("/auth/logout"),
  forgotPassword: (email) => api.post("/auth/forgot-password", { email }),
  resetPassword: (token, newPassword) =>
    api.post("/auth/reset-password", { token, newPassword }),
  getOrders: () => api.get("/auth/orders"),
};

export default api;
