import axios from './axios';

export interface SchedulerTask {
  title: string;
  estimatedHours?: number;
  dueDate?: string;
  dependencies: string[];
}

export interface SchedulerOrderRequest {
  tasks: SchedulerTask[];
  strategy?: 'DepsDueSjf' | 'DepsDueFifo' | 'DepsOnly';
}

export interface SchedulerOrderResponse {
  recommendedOrder: string[];
  strategyUsed: string;
}

export const schedulerApi = {
  getRecommendedOrder: async (request: SchedulerOrderRequest): Promise<SchedulerOrderResponse> => {
    const response = await axios.post('/scheduler/order', request);
    return response.data;
  }
};
