import api from './axios';

export interface CreateFeedbackDto {
  rating: number;
  comment?: string;
}

export interface FeedbackDto {
  id: string;
  rating: number;
  comment?: string;
  createdAt: string;
}

export const feedbackApi = {
  createFeedback: async (data: CreateFeedbackDto): Promise<FeedbackDto> => {
    const response = await api.post<FeedbackDto>('/feedback', data);
    return response.data;
  },

  getFeedback: async (): Promise<FeedbackDto[]> => {
    const response = await api.get<FeedbackDto[]>('/feedback');
    return response.data;
  }
};
