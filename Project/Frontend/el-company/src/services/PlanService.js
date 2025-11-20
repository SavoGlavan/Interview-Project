
import axios from 'axios';

const API_BASE = import.meta.env.VITE_API_URL;
const API_URL = `${API_BASE}/plan`;

export const planService = {
  getAll: async () => {
    const token = localStorage.getItem("token");
    const response = await axios.get(`${API_URL}`, {
      headers: {
        Authorization: `Bearer ${token}`,
      } 
    });
    return response.data;
  },
  getById: async (id) => {
    const response = await axios.get(`${API_URL}/${id}`);
    return response.data;
  },
  getRecommendation: async (consumption, taxGroupId) => {
    const token = localStorage.getItem("token"); 
    const response = await axios.post(`${API_URL}/recommend`,  
      { consumption, taxGroupId },
      {
        headers: {
          Authorization: `Bearer ${token}`, // <-- attach token
        },
      });
    return response.data;
  }
  ,
  createPlan: async (plan) => {
    const token = localStorage.getItem("token");
    const response = await axios.post(`${API_URL}`, plan, {
      headers: { Authorization: `Bearer ${token}`},
    });
    return response.data;
  },
  updatePlan: async (id, updatedPlan) => {
    const token = localStorage.getItem("token");
    const response = await axios.put(`${API_URL}/${id}`, updatedPlan, {
      headers: { Authorization: `Bearer ${token}` },
    });
    return response.data;
  },
  deletePlan: async (id) => {
  const token = localStorage.getItem("token");
  await axios.delete(`${API_URL}/${id}`, {
    headers: { Authorization: `Bearer ${token}` },
  });
},
  
};
