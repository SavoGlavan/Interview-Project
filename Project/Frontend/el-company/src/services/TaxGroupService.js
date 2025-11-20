import axios from 'axios';


const API_BASE = import.meta.env.VITE_API_URL;
const API_URL = `${API_BASE}/taxgroups`;
export const taxGroupService = {
  
  getAll: async () => {
    const token = localStorage.getItem("token");
    const response = await axios.get(API_URL,
      {
        headers: {
          Authorization: `Bearer ${token}`,
      } 
      }
    );
    return response.data;
  },

  getById: async (id) => {
    const token = localStorage.getItem("token");
    const response = await axios.get(`${API_URL}/${id}`,
      {
        headers: {
          Authorization: `Bearer ${token}`,
      } 
      });
    return response.data;
  },

  create: async (taxGroup) => {
    const token = localStorage.getItem("token");
    console.log(taxGroup);
    const response = await axios.post(API_URL, {name:taxGroup.name, vat:taxGroup.vat, eco_tax:taxGroup.ecoTax},
      {
        headers: {
          Authorization: `Bearer ${token}`,
      } 
      });
    return response.data;
  },

  update: async (id, taxGroup) => {
    const token = localStorage.getItem("token");
    
    const response = await axios.put(`${API_URL}/${id}`, {name: taxGroup.name,vat:taxGroup.vat, eco_tax:taxGroup.ecoTax},
      {
        headers: {
          Authorization: `Bearer ${token}`,
      } 
      });
    return response.data;
  },

  delete: async (id) => {
    const token = localStorage.getItem("token");
    const response = await axios.delete(`${API_URL}/${id}`,
      {
        headers: {
          Authorization: `Bearer ${token}`,
      } 
      });
    return response.data;
  },
};
