import axios from 'axios';

export type AutomationTriggerType = 'TaskStatusChanged' | 'TaskCreated' | 'TaskPriorityChanged';
export type AutomationActionType = 'SendNotification' | 'PostComment' | 'ChangeStatus';

export interface AutomationRuleDto {
  id: string;
  projectId: string;
  name: string;
  isEnabled: boolean;
  triggerType: AutomationTriggerType;
  triggerValue: string;
  actionType: AutomationActionType;
  actionValue: string;
  createdAt: string;
}

export interface CreateAutomationRulePayload {
  projectId: string;
  name: string;
  triggerType: AutomationTriggerType;
  triggerValue: string;
  actionType: AutomationActionType;
  actionValue: string;
}

const BASE = '/api/automations';

export const automationService = {
  getByProject: async (projectId: string): Promise<AutomationRuleDto[]> => {
    const { data } = await axios.get<AutomationRuleDto[]>(`${BASE}/projects/${projectId}`);
    return data;
  },

  create: async (payload: CreateAutomationRulePayload): Promise<string> => {
    const { data } = await axios.post<string>(BASE, payload);
    return data;
  },

  toggle: async (id: string, isEnabled: boolean): Promise<void> => {
    await axios.patch(`${BASE}/${id}/toggle`, { isEnabled });
  },

  delete: async (id: string): Promise<void> => {
    await axios.delete(`${BASE}/${id}`);
  },
};
