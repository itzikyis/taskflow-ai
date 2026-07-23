import axios from 'axios';

export interface SlackCommandConfigDto {
  isConfigured: boolean;
}

export interface SlackConfig {
  configured: boolean;
  enabled: boolean;
  webhookUrlMasked: string;
  forwardCreated: boolean;
  forwardStatusChanged: boolean;
  forwardComments: boolean;
  forwardOther: boolean;
}

export interface SaveSlackConfig {
  webhookUrl: string;
  enabled: boolean;
  forwardCreated: boolean;
  forwardStatusChanged: boolean;
  forwardComments: boolean;
  forwardOther: boolean;
}

const BASE = '/api/integrations/slack';

export const slackService = {
  get: async (): Promise<SlackConfig> => (await axios.get<SlackConfig>(BASE)).data,
  save: async (config: SaveSlackConfig): Promise<void> => { await axios.put(BASE, config); },
  test: async (): Promise<void> => { await axios.post(`${BASE}/test`); },
  remove: async (): Promise<void> => { await axios.delete(BASE); },
  getCommandConfig: async (): Promise<SlackCommandConfigDto> =>
    (await axios.get<SlackCommandConfigDto>(`${BASE}/command-config`)).data,
};
