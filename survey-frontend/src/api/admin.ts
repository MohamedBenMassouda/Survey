import api from "./api";
import type { AdminLoginDto, CreateAdminDto, Admin, PaginationParams, PaginatedResponse } from "../types";

export const loginAdmin = async (loginData: AdminLoginDto) => {
  try {
    const response = await api.post("/Admins/login", loginData);
    return response.data;
  } catch (error) {
    console.error("Admin login error:", error);
    throw error;
  }
};

export const createAdmin = async (adminData: CreateAdminDto) => {
  try {
    const response = await api.post("/Admins", adminData);
    return response.data;
  } catch (error) {
    console.error("Create admin error:", error);
    throw error;
  }
};

export const getAdmins = async (params?: PaginationParams): Promise<PaginatedResponse<Admin>> => {
  try {
    const response = await api.get("/Admins", { params });
    return response.data;
  } catch (error) {
    console.error("Get admins error:", error);
    throw error;
  }
};

export const getAdminById = async (id: string): Promise<Admin> => {
  try {
    const response = await api.get(`/Admins/${id}`);
    return response.data;
  } catch (error) {
    console.error("Get admin by ID error:", error);
    throw error;
  }
};