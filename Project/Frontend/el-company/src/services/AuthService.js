
import axios from 'axios';
import {jwtDecode} from "jwt-decode";
const API_BASE = import.meta.env.VITE_API_URL;
const API_URL = `${API_BASE}/auth`;

export const authService = {
  login: async (username, password) => {
    const response = await axios.post(`${API_URL}/login`, { username, password });
    return response.data;
  },
  register: async (request) => {
    const response = await axios.post(`${API_URL}/register`, request);
    return response.data;
  },
  getUserFromToken: () => {
    const token = localStorage.getItem("token");
    if (!token) return null;

    try {
      const decoded = jwtDecode(token);
      return {
        id: decoded.sub,
        username: decoded.unique_name,
        role: decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || decoded.role,
      };
    } catch (err) {
      console.error("Invalid or expired token:", err);
      return null;
    }
  }
};
