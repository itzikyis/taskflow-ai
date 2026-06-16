export interface Attachment {
  id: string;
  taskId: string;
  uploadedBy: string;
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
  storageUrl: string;
  uploadedAt: string;
}

export interface AddAttachmentPayload {
  uploadedBy: string;
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
  storageUrl: string;
}

export interface DeleteAttachmentPayload {
  requesterId: string;
}
