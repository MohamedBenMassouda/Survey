// Admin types
export interface AdminLoginDto {
  email: string;
  password: string;
}

export interface CreateAdminDto {
  email: string;
  password: string;
  fullName: string;
}

export interface Admin {
  id: string;
  email: string;
  fullName: string;
  createdAt: string;
}

// Survey types
export interface CreateOptionRequest {
  optionText: string;
  displayOrder?: number;
}

export interface CreateQuestionRequest {
  questionText: string;
  questionType: string;
  isRequired: boolean;
  displayOrder?: number;
  options?: CreateOptionRequest[];
}

export interface CreateSurveyRequest {
  title: string;
  description: string;
  startDate?: string;
  endDate?: string;
  status?: string;
  questions: CreateQuestionRequest[];
}

export interface SurveyDto {
  id: string;
  createdAt: string;
  updatedAt: string;
  title: string;
  status: string;
  createdByName: string;
  questionCount: number;
  responseCount: number;
}

export interface SurveyAnalyticsDto {
  surveyId: string;
  surveyTitle: string;
  totalQuestions: number;
  tokensGenerated: number;
  totalResponses: number;
  responseRate: number;
  averageCompletionTime: number;
}

// Invitation types
export interface SendInvitationRequest {
  surveyId: string;
  recipientEmails: string[];
  customMessage?: string;
}

export interface InvitationError {
  email: string;
  errorMessage: string;
}

export interface InvitationResponse {
  totalInvitations: number;
  successfulInvitations: string[];
  failedInvitations: InvitationError[];
}

// Pagination
export interface PaginationParams {
  pageNumber?: number;
  pageSize?: number;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

// Question types (matching backend enum values)
export const QuestionType = {
  MULTIPLE_CHOICE: 'MultipleChoice',
  TEXT: 'Text',
  CHECKBOX: 'Checkbox',
  RATING: 'Rating',
  YES_NO: 'YesNo',
  DROPDOWN: 'Dropdown',
  SCALE: 'Scale'
} as const;

export type QuestionType = typeof QuestionType[keyof typeof QuestionType];

// Survey statuses
export const SurveyStatus = {
  DRAFT: 'Draft',
  PUBLISHED: 'Published',
  CLOSED: 'Closed'
} as const;

export type SurveyStatus = typeof SurveyStatus[keyof typeof SurveyStatus];