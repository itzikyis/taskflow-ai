export const DEV_REF_TYPES = ['Branch', 'Commit', 'PullRequest'] as const;
export type DevelopmentRefType = (typeof DEV_REF_TYPES)[number];

export const DEV_LINK_STATUSES = ['None', 'Open', 'Draft', 'Merged', 'Closed'] as const;
export type DevelopmentLinkStatus = (typeof DEV_LINK_STATUSES)[number];

export interface DevelopmentLink {
  id: string;
  taskId: string;
  repository: string;
  refType: DevelopmentRefType;
  title: string;
  url: string;
  status: DevelopmentLinkStatus;
  externalId: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateDevelopmentLinkPayload {
  repository: string;
  refType: DevelopmentRefType;
  title: string;
  url: string;
  status?: DevelopmentLinkStatus;
  externalId?: string;
}
