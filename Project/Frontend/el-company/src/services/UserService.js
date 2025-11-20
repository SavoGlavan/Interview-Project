import axios from 'axios';

const API_BASE = import.meta.env.VITE_API_URL;
const API_URL = `${API_BASE}/users`;

export const userService = {
  
  setUserPlan: async (id, taxGroupId, planId, consumption) => {
    
    const token = localStorage.getItem("token");
    const response = await axios.put(`${API_URL}/${id}`, {taxGroupId, planId, consumption},
      {
        headers: {
          Authorization: `Bearer ${token}`,
      } 
      });
    return response.data;
  },
  getById: async (id)=>{
    const token = localStorage.getItem("token");
    const response = await axios.get(`${API_URL}/${id}`,
      {
        headers: {
          Authorization: `Bearer ${token}`,
      } 
      });
    return response.data;
  },
  getUserCountByPlan: async ()=>{
    const token = localStorage.getItem("token");
    const response = await axios.get(`${API_URL}/count-by-plan`,
      {
        headers: {
          Authorization: `Bearer ${token}`,
      } 
      });
    return response.data;
  },
   getUserCountByTaxGroup: async ()=>{
    const token = localStorage.getItem("token");
    const response = await axios.get(`${API_URL}/count-by-taxgroup`,
      {
        headers: {
          Authorization: `Bearer ${token}`,
      } 
      });
    return response.data;
  }

  
};
