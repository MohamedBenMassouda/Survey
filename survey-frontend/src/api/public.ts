import axios from "axios";
import api from "./api";

// Public survey interfaces (matching OpenAPI SurveyDetailDto)
export interface PublicSurveyQuestion {
  id: string;
  questionText: string;
  questionType: string;
  isRequired: boolean;
  displayOrder: number;
  options?: {
    id: string;
    optionText: string;
    displayOrder: number;
  }[];
}

export interface PublicSurvey {
  id: string;
  createdAt: string;
  updatedAt: string;
  title: string;
  description: string;
  status: string;
  startDate?: string;
  endDate?: string;
  createdBy: {
    id: string;
    email: string;
    fullName: string;
  };
  questions: PublicSurveyQuestion[];
}

// Interface matching the API specification
export interface SubmitAnswerRequest {
  questionId: string;
  answerText?: string;
  selectedOptionIds?: string[];
}

export interface SubmitSurveyResponseRequest {
  token: string;
  answers: SubmitAnswerRequest[];
}

// Fetch public survey by surveyId and token
export const getPublicSurvey = async (
  surveyIdWithToken: string
): Promise<PublicSurvey> => {
  try {
    // Parse surveyId and token from the combined string
    const [surveyId, queryString] = surveyIdWithToken.split("?");
    const urlParams = new URLSearchParams(queryString);
    const token = urlParams.get("token");

    console.log(
      "Making API call to:",
      `/Surveys/${surveyId}`,
      "with token:",
      token
    );

    const response = await api.get(`/Surveys/${surveyId}`, {
      params: {
        token: token,
      },
    });

    console.log("API response:", response.data);
    return response.data;
  } catch (error) {
    console.error("Get public survey error:", error);
    throw error;
  }
};

// Submit survey response
export const submitSurveyResponse = async (
  token: string,
  answers: SubmitAnswerRequest[]
) => {
  try {
    const requestBody: SubmitSurveyResponseRequest = {
      token,
      answers,
    };

    console.log("Submitting survey response:", requestBody);

    const response = await api.post(`/Surveys/responses`, requestBody);
    return response.data;
  } catch (error) {
    console.error("Submit survey response error:", error);
    throw error;
  }
};
