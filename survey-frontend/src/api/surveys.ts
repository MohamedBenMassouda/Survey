import api from "./api";
import type { 
  CreateSurveyRequest, 
  SurveyDto, 
  SurveyAnalyticsDto, 
  SendInvitationRequest, 
  InvitationResponse,
  PaginationParams,
  PaginatedResponse 
} from "../types";

export const createSurvey = async (surveyData: CreateSurveyRequest): Promise<SurveyDto> => {
  try {
    const response = await api.post("/Surveys", surveyData);
    return response.data;
  } catch (error) {
    console.error("Create survey error:", error);
    throw error;
  }
};

export const getSurveys = async (params?: PaginationParams): Promise<PaginatedResponse<SurveyDto>> => {
  try {
    console.log('Fetching all surveys with params:', params);
    const response = await api.get("/Surveys", { params });
    console.log('All surveys API response:', response);
    console.log('All surveys data:', response.data);
    return response.data;
  } catch (error) {
    console.error("Get surveys error:", error);
    throw error;
  }
};

export const getPublishedSurveys = async (params?: PaginationParams): Promise<PaginatedResponse<SurveyDto>> => {
  try {
    console.log('Fetching published surveys with params:', params);
    const response = await api.get("/Surveys/published", { params });
    console.log('Published surveys API response:', response);
    console.log('Published surveys data:', response.data);
    return response.data;
  } catch (error) {
    console.error("Get published surveys error:", error);
    throw error;
  }
};

export const getSurveyById = async (id: string): Promise<SurveyDto> => {
  try {
    const response = await api.get(`/Surveys/${id}`);
    return response.data;
  } catch (error) {
    console.error("Get survey by ID error:", error);
    throw error;
  }
};

export const getSurveyAnalytics = async (id: string): Promise<SurveyAnalyticsDto> => {
  try {
    const response = await api.get(`/Surveys/${id}/analytics`);
    return response.data;
  } catch (error) {
    console.error("Get survey analytics error:", error);
    throw error;
  }
};

export const sendInvitations = async (invitationData: SendInvitationRequest): Promise<InvitationResponse> => {
  try {
    console.log("Sending invitations to:", "/Surveys/invitations");
    console.log("Request data:", JSON.stringify(invitationData, null, 2));
    const response = await api.post("/Surveys/invitations", invitationData);
    console.log("Send invitations response:", response.data);
    return response.data;
  } catch (error) {
    console.error("Send invitations error:", error);
    console.error("Error details:", error);
    throw error;
  }
};

// NOTE: This endpoint is not available in the current API specification
// The API needs to be updated to include a PATCH /api/Surveys/{id}/publish endpoint
export const publishSurvey = async (id: string): Promise<SurveyDto> => {
  try {
    const response = await api.post(`/Surveys/${id}/publish`);
    return response.data;
  } catch (error) {
    console.error("Publish survey error:", error);
    throw error;
  }
};